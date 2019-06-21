using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Rhino;
using Rhino.Geometry;

namespace DotsProj
{
    class SmoothCurveSolver
    {
        private Curve c2;
        private Curve OFFSET_CRV;
        private int numDiv;
        private int numTowers;
        private double flrHt;
        private double numFlrs;
        private double OFFSET_INP;
        private double baseFsr;
        private double towerFsr;
        private double SITE_AR;

        public List<Curve> globalBaseCrvLi { get; set; } // global parameter
        public List<Curve> globalTowerCrvLi { get; set; } // global parameter
        public List<Point3d> globalPtCrvLi { get; set; } // normal / subdiv points

        Random rnd = new Random();

        public SmoothCurveSolver() { }

        public SmoothCurveSolver(Curve c2_, Curve OFFSET_CRV_, int numDiv_, int numTowers_,
            double flrHt_, double OFFSET_INP_, double baseFsr_, double towerFsr_, double site_ar_)
        {
            this.c2 = c2_;
            this.OFFSET_CRV = OFFSET_CRV_;
            this.numDiv = numDiv_;
            this.numTowers = numTowers_;
            this.flrHt = flrHt_;
            this.OFFSET_INP = OFFSET_INP_;
            this.baseFsr = baseFsr_;
            this.towerFsr = towerFsr_;
            this.SITE_AR = site_ar_;
        }

        public List<Brep> Compute()
        {
            globalBaseCrvLi = new List<Curve>(); // global parameter
            globalTowerCrvLi = new List<Curve>(); // global parameter
            globalPtCrvLi = new List<Point3d>();
            double[] p = c2.DivideByCount(numDiv, true);
            List<Point3d> ptLi = new List<Point3d>();         // points on the site boundary
            for (int i = 0; i < p.Length; i++)
            {
                Point3d pts = c2.PointAt(p[i]);
                ptLi.Add(pts);
                globalPtCrvLi.Add(pts);
            }

            // GENERATE NORMALS FROM THE POINT - SCALE=SETBACK DISTANCE
            List<LineCurve> lineLi = new List<LineCurve>();
            for (int i = 0; i < ptLi.Count; i++)
            {
                Point3d P = Point3d.Unset;
                Point3d Q = Point3d.Unset;
                if (i == 0)
                {
                    P = ptLi[ptLi.Count - 1];
                    Q = ptLi[0];
                }
                else
                {
                    P = ptLi[i - 1];
                    Q = ptLi[i];
                }
                double sc = 1 / P.DistanceTo(Q);
                double sc2 = OFFSET_INP;
                double dx = P.X - (Q.Y - P.Y) * sc;
                double dy = P.Y + (Q.X - P.X) * sc;
                Point3d u = new Point3d(dx, dy, 0);
                double dx2 = P.X - (Q.Y - P.Y) * sc * sc2;
                double dy2 = P.Y + (Q.X - P.X) * sc * sc2;
                double ex2 = P.X + (Q.Y - P.Y) * sc * sc2;
                double ey2 = P.Y - (Q.X - P.X) * sc * sc2;
                Point3d u2 = new Point3d(dx2, dy2, 0);
                Point3d v2 = new Point3d(ex2, ey2, 0);
                Rhino.Geometry.PointContainment contU = c2.Contains(u, Plane.WorldXY, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                if (contU.ToString() == "Inside")
                {
                    LineCurve linePu = new LineCurve(P, u2);
                    lineLi.Add(linePu);
                }
                else
                {
                    LineCurve linePu = new LineCurve(P, v2);
                    lineLi.Add(linePu);
                }
            }

            // FIND INTX - NORMAL x SETBACK CURVE; REMOVE OTHER NORMALS
            List<LineCurve> fLineLi = new List<LineCurve>();
            for (int i = 0; i < lineLi.Count; i++)
            {
                double t = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
                var evt = Rhino.Geometry.Intersect.Intersection.CurveCurve(OFFSET_CRV, lineLi[i], t, t);
                try
                {
                    Point3d A = lineLi[i].PointAtStart;
                    Point3d pA = evt[0].PointA;
                    LineCurve line = new LineCurve(A, pA);
                    fLineLi.Add(line);
                }
                catch (Exception) { }
            }

            // JOIN ADJACENT NORMAL SEGMENTS TO FORM POLYGONS
            // 1st point is point on site, 2nd = normal @ setback dist
            List<PolylineCurve> polyCrvLi = new List<PolylineCurve>();
            for (int i = 0; i < lineLi.Count; i++)
            {
                if (i == 0)
                {
                    LineCurve A = lineLi[lineLi.Count - 1];
                    LineCurve B = lineLi[0];
                    Point3d a0 = A.PointAtStart;
                    Point3d a1 = A.PointAtEnd;
                    Point3d b0 = B.PointAtStart;
                    Point3d b1 = B.PointAtEnd;
                    Point3d[] pts = { a0, b0, b1, a1, a0 };
                    PolylineCurve poly = new PolylineCurve(pts);
                    polyCrvLi.Add(poly);
                }
                else
                {
                    LineCurve A = lineLi[i - 1];
                    LineCurve B = lineLi[i];
                    Point3d a0 = A.PointAtStart;
                    Point3d a1 = A.PointAtEnd;
                    Point3d b0 = B.PointAtStart;
                    Point3d b1 = B.PointAtEnd;
                    Point3d[] pts = { a0, b0, b1, a1, a0 };
                    PolylineCurve poly = new PolylineCurve(pts);
                    polyCrvLi.Add(poly);
                }
            }
            double baseExtrHt = 0.0;
            double spineHt = 0.0;
            List<Brep> fbrepLi = new List<Brep>();
            try
            {   // BASE PROCESSES= EXTRUSION & COPY
                double c2Area = AreaMassProperties.Compute(c2).Area;
                double offsetArea = AreaMassProperties.Compute(OFFSET_CRV).Area;
                double flrAr = c2Area - offsetArea;
                int numFlrs2 = (int)(SITE_AR * baseFsr / flrAr) + 1;
                baseExtrHt = numFlrs2 * flrHt;
                for (int i = 0; i < numFlrs2; i++)
                {
                    Curve c2_copy = c2.DuplicateCurve();
                    Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0, 0, spineHt);
                    c2_copy.Transform(xform);
                    Curve OFFSET_CRV_copy = OFFSET_CRV.DuplicateCurve();
                    OFFSET_CRV_copy.Transform(xform);
                    spineHt += flrHt;
                    globalBaseCrvLi.Add(c2_copy);
                    globalBaseCrvLi.Add(OFFSET_CRV_copy);
                }

                // base extrusions
                Extrusion outerExtr = Rhino.Geometry.Extrusion.Create(c2, baseExtrHt, true);
                var t0 = outerExtr.GetBoundingBox(true);
                if (t0.Max.Z <= 0)
                {
                    outerExtr = Rhino.Geometry.Extrusion.Create(c2, baseExtrHt, true);
                }
                Brep outerBrep = outerExtr.ToBrep();

                Extrusion innerExtr = Rhino.Geometry.Extrusion.Create(OFFSET_CRV, -baseExtrHt, true);
                var t1 = innerExtr.GetBoundingBox(true);
                if (t1.Max.Z <= 0)
                {
                    innerExtr = Rhino.Geometry.Extrusion.Create(OFFSET_CRV, baseExtrHt, true);
                }
                Brep innerBrep = innerExtr.ToBrep();
                Brep[] netBrepArr = Brep.CreateBooleanDifference(outerBrep, innerBrep, 0.01);
                Brep netBrep = netBrepArr[0];
                fbrepLi.Add(netBrep);
            }
            catch (Exception) { }

            // Tower POLY FOR THE TOWERS
            List<Curve> towerPolyLi = new List<Curve>();
            List<PolylineCurve> fPolyLi = new List<PolylineCurve>();
            int numSel = numTowers;

            double cumuArPoly = 0.0;
            for (int i = 0; i < numSel; i++)
            {
                int idx = rnd.Next(polyCrvLi.Count);
                fPolyLi.Add(polyCrvLi[idx]);
                cumuArPoly += AreaMassProperties.Compute(polyCrvLi[idx]).Area;
            }
            // Tower POLY EXTRUSION
            double towerHtReq = SITE_AR * towerFsr / cumuArPoly;
            for (int i = 0; i < fPolyLi.Count; i++)
            {
                PolylineCurve crv = fPolyLi[i];
                Extrusion extr0 = Extrusion.Create(crv, -towerHtReq * 3, true);
                var t0 = extr0.GetBoundingBox(true);
                if (t0.Max.Z <= 0)
                {
                    extr0 = Extrusion.Create(crv, towerHtReq * 3, true);
                }
                Brep brep = extr0.ToBrep();
                Transform xform = Rhino.Geometry.Transform.Translation(0, 0, baseExtrHt);
                brep.Transform(xform);
                fbrepLi.Add(brep);
            }
            // Tower POLY FLOOR COPIES
            // spineHt initialized above & updated from base curve copies
            int numFlrs = (int)(SITE_AR * towerFsr / cumuArPoly) + 1;
            for (int i = 0; i < numFlrs; i++)
            {
                for (int j = 0; j < fPolyLi.Count; j++)
                {
                    Curve crv = fPolyLi[j].DuplicateCurve();
                    Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0, 0, spineHt);
                    crv.Transform(xform);
                    globalTowerCrvLi.Add(crv);
                }
                spineHt += flrHt;
            }
            return fbrepLi;
        }
    }
}
