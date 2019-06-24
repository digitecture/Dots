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

        public List<Curve> BSPCrvs { get; set; } // transformed
        public List<Curve> ResultPolys { get; set; } //reverse transformed final solution
        Random rnd = new Random();

        public int RecursionCounter = 0;
        public int NumPendantPoly = 0;

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
            ResultPolys = new List<Curve>();
        }

        public void GenerateInitialCurve()
        {
            ResultPolys = new List<Curve>();
            HorSplit(rot_SITE_CRV);
            //VerSplit(rot_SITE_CRV);
            foreach (Curve crvR in BSPCrvs)
            {
                Curve c2 = crvR.DuplicateCurve();
                c2.Transform(reverseXForm);
                ResultPolys.Add(c2);
            }
        }

        public void HorSplit(Curve iniPoly)
        {
            Point3d[] B = GetBBoxPoly(iniPoly);
            Point3d a = B[0];
            Point3d b = B[1];
            Point3d c = B[2];
            Point3d d = B[3];
            double t = rnd.NextDouble()*0.5 + 0.25;
            Point3d e = new Point3d(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, 0);
            Point3d f = new Point3d(d.X + (c.X - d.X) * t, d.Y + (c.Y - d.Y) * t, 0);

            List<Point3d> le = new List<Point3d> { a, e, f, d, a };
            List<Point3d> ri = new List<Point3d> { e, b, c, f, e };

            PolylineCurve left = new PolylineCurve(le);
            PolylineCurve right = new PolylineCurve(ri);

            Curve[] leftCrv = Curve.CreateBooleanIntersection(left, rot_SITE_CRV);
            Curve[] rightCrv = Curve.CreateBooleanIntersection(right, rot_SITE_CRV);


            foreach (Curve crvL in leftCrv)
            {
                //crvL.Transform(reverseXForm);
                BSPCrvs.Add(crvL);
            }
            foreach (Curve crvR in rightCrv)
            {
                //crvR.Transform(reverseXForm);
                BSPCrvs.Add(crvR);
            }
        }
     
        public void VerSplit(Curve iniPoly)
        {
            Point3d[] B = GetBBoxPoly(iniPoly);
            Point3d a = B[0];
            Point3d b = B[1];
            Point3d c = B[2];
            Point3d d = B[3];
            double t = rnd.NextDouble() * 0.5 + 0.25;
            Point3d e = new Point3d(a.X + (d.X - a.X) * t, a.Y + (d.Y - a.Y) * t, 0);
            Point3d f = new Point3d(d.X + (c.X - b.X) * t, d.Y + (c.Y - b.Y) * t, 0);

            List<Point3d> up_ = new List<Point3d> { a, b, f, e, a };
            List<Point3d> dn_ = new List<Point3d> { e, f, c, d, e };

            PolylineCurve up = new PolylineCurve(up_);
            PolylineCurve dn = new PolylineCurve(dn_);

            Curve[] upCrv = Curve.CreateBooleanIntersection(up, rot_SITE_CRV);
            Curve[] dnCrv = Curve.CreateBooleanIntersection(dn, rot_SITE_CRV);


            foreach (Curve crvU in upCrv)
            {
                //crvL.Transform(reverseXForm);
                BSPCrvs.Add(crvU);
            }
            foreach (Curve crvD in dnCrv)
            {
                //crvR.Transform(reverseXForm);
                BSPCrvs.Add(crvD);
            }
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
            return pts;
        }
    }
}



