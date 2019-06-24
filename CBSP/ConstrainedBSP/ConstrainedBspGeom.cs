using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;

using Rhino;
using Rhino.Geometry;

namespace DotsProj
{

    public class GenCBspGeom
    {
        private Curve SITE_CRV;
        private Curve rot_SITE_CRV;
        public List<string> AdjObjLi { get; set; }
        public List<GeomObj> NorGeomObjLi { get; set; }
        private double Rotation;

        private int globalRecursionCounter = 0;
        private int MaxRecursions;

        public List<Curve> BSPCrvs { get; set; } // transformed
        public List<Curve> ResultBBxPolys { get; set; } // reverse transformed final solution
        public List<Curve> ExtractedCrvs { get; set; } // get the real curve intesection - site - bbxpoly[i]
        public List<Curve> BBxCrvs { get; set; } //bbx polylines
        public List<nsSeg> PartitionSegLi { get; set; } // partition lines of the bbx
        Random rnd = new Random();

        private Rhino.Geometry.Transform XForm;
        private Rhino.Geometry.Transform reverseXForm;

        public GenCBspGeom() { }

        public GenCBspGeom(Curve site_crv, List<string> adjObjLi_, List<GeomObj> norGeomObjLi_, double rot)
        {
            SITE_CRV = site_crv;
            AdjObjLi = adjObjLi_;
            NorGeomObjLi = norGeomObjLi_;
            Rotation = Rhino.RhinoMath.ToRadians(rot);

            Point3d SITE_CEN = AreaMassProperties.Compute(SITE_CRV).Centroid;
            XForm = Rhino.Geometry.Transform.Rotation(Rotation, SITE_CEN);
            reverseXForm = Rhino.Geometry.Transform.Rotation(-Rotation, SITE_CEN);

            rot_SITE_CRV = SITE_CRV.DuplicateCurve();
            rot_SITE_CRV.Transform(XForm);

            BSPCrvs = new List<Curve>();
            ResultBBxPolys = new List<Curve>();
        }

        public void ExtractPolyFromSite()
        {
            List<Curve> crvLi = new List<Curve>();

            ExtractedCrvs = new List<Curve>();

            Curve site_crv = SITE_CRV.DuplicateCurve();
            for(int i=0; i<ResultBBxPolys.Count; i++)
            {
                Curve[] diffCrv = Curve.CreateBooleanIntersection(site_crv, ResultBBxPolys[i], Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                for(int j=0; j<diffCrv.Length; j++)
                {
                    ExtractedCrvs.Add(diffCrv[j]);
                }
            }
        }

        public void GenerateInitialCurve()
        {
            // rotate the curve and take the bounding box of rotation
            // find the partitions of the rotated bbx
            // extract the intersections of the curve x bbx

            ResultBBxPolys = new List<Curve>();
            BBxCrvs = new List<Curve>();
            
            // initialize stack
            List<Point3d> iniPtLi = new List<Point3d>();
            Point3d[] ptArr = GetBBoxPoly(rot_SITE_CRV);
            iniPtLi.AddRange(ptArr);
            PolylineCurve iniBBX = new PolylineCurve(iniPtLi);
            BSPCrvs.Add(iniBBX);

            MaxRecursions = NorGeomObjLi.Count;
            globalRecursionCounter = 0;
            runRecursions(); // run the recursions & update global vars

            for (int i = MaxRecursions; i < BSPCrvs.Count; i++)
            {
                Curve c2 = BSPCrvs[i].DuplicateCurve();
                c2.Transform(reverseXForm);
                ResultBBxPolys.Add(c2);
            }
        }

        public void runRecursions()
        {
            Curve crv = BSPCrvs[globalRecursionCounter];
            List<Point3d> iniPtLi = GetPolyPts(crv);
            Point3d a = iniPtLi[0];
            Point3d b = iniPtLi[1];
            Point3d c = iniPtLi[2];
            Point3d d = iniPtLi[3];
            if (globalRecursionCounter < MaxRecursions)
            {
                globalRecursionCounter++;
                // HOR = a-d | Ver = a-b 
                // int t = rnd.Next(0, 9);
                // if (t > 5)
                if(a.DistanceTo(b)<a.DistanceTo(d)) { HorSplit(crv); }
                else { VerSplit(crv); }
                runRecursions();
            }
        }

        public void VerSplit(Curve iniPoly)
        {
            List<Point3d> iniPtLi = GetPolyPts(iniPoly);
            Point3d a = iniPtLi[0];
            Point3d b = iniPtLi[1];
            Point3d c = iniPtLi[2];
            Point3d d = iniPtLi[3];

            double t = rnd.NextDouble() * 0.5 + 0.25;
            Point3d e = new Point3d(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, 0);
            Point3d f = new Point3d(d.X + (c.X - d.X) * t, d.Y + (c.Y - d.Y) * t, 0);

            List<Point3d> le = new List<Point3d> { a, e, f, d, a };
            List<Point3d> ri = new List<Point3d> { e, b, c, f, e };

            PolylineCurve left = new PolylineCurve(le);
            PolylineCurve right = new PolylineCurve(ri);

            BSPCrvs.Add(left);
            BSPCrvs.Add(right);
        }

        public void HorSplit(Curve iniPoly)
        {
            List<Point3d> iniPtLi = GetPolyPts(iniPoly);
            Point3d a = iniPtLi[0];
            Point3d b = iniPtLi[1];
            Point3d c = iniPtLi[2];
            Point3d d = iniPtLi[3];

            double t = rnd.NextDouble() * 0.5 + 0.25;
            Point3d e = new Point3d(a.X + (d.X - a.X) * t, a.Y + (d.Y - a.Y) * t, 0);
            Point3d f = new Point3d(b.X + (c.X - b.X) * t, b.Y + (c.Y - b.Y) * t, 0);


            nsPt nsE = new nsPt(e.X, e.Y, e.Z);
            nsPt nsF = new nsPt(f.X, f.Y, f.Z);
            nsSeg nsEF = new nsSeg(nsE, nsF);
            PartitionSegLi.Add(nsEF);

            List<Point3d> up_ = new List<Point3d> { a, b, f, e, a };
            List<Point3d> dn_ = new List<Point3d> { e, f, c, d, e };

            PolylineCurve up = new PolylineCurve(up_);
            PolylineCurve dn = new PolylineCurve(dn_);

            BSPCrvs.Add(up);
            BSPCrvs.Add(dn);
        }

        public Point3d[] GetBBoxPoly(Curve crv)
        {
            var iniB = crv.GetBoundingBox(true);
            Point3d a = iniB.Min;
            Point3d c = iniB.Max;
            Point3d b = new Point3d(c.X, a.Y, 0);
            Point3d d = new Point3d(a.X, c.Y, 0);
            Point3d[] pts = { a, b, c, d, a };
            List<Point3d> ptsLi = new List<Point3d> { a, b, c, d, a };
            PolylineCurve poly = new PolylineCurve(ptsLi);
            Curve bbxCrv = (Curve)poly;
            BBxCrvs.Add(bbxCrv);
            return pts;
        }

        public List<Point3d> GetPolyPts(Curve crv)
        {
            var t = crv.TryGetPolyline(out Polyline pts);
            IEnumerator<Point3d> ptEnum = pts.GetEnumerator();
            List<Point3d> ptLi = new List<Point3d>();
            while (ptEnum.MoveNext())
            {
                ptLi.Add(ptEnum.Current);
            }
            return ptLi;
        }
    }
}



