using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj
{
    public class StagerredCourtyard : GH_Component
    {
        Random rnd = new Random();
        List<Point3d> globalPtCrvLi;
        public StagerredCourtyard()
          : base("StagerredCourtyardDotsComponent", "stagerred-courtyard",
            "StagerredCourtyardDotsComponent description",
            "DOTS", "Massing")
        {
            globalPtCrvLi = new List<Point3d>();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0. site curve
            pManager.AddCurveParameter("site", "site", "site", GH_ParamAccess.item);
            // 1. Setback 
            pManager.AddNumberParameter("Setback", "setback", "setback distance from site boundary", GH_ParamAccess.item);
            // 2. offset input
            pManager.AddNumberParameter("OFFSET_INP", "OFFSET_INP", "OFFSET_INP", GH_ParamAccess.item);
            // 3. number of sub-divisions
            pManager.AddIntegerParameter("number of divisions - per seg (poly) & overall (smooth curves)", "div", "number of divisions of the site curve: if smooth, provide overall, else (if poly) provide num/seg", GH_ParamAccess.item);
            // 4. num peaks
            pManager.AddIntegerParameter("number of Peaks", "peaks", "number of Peaks of the site curve", GH_ParamAccess.item);
            // 5. min area of curve
            pManager.AddNumberParameter("minimum-area-crv", "min-ar-curve", "Restraint: Minimum area if the curve", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0
            pManager.AddCurveParameter("offsetcrv", "offsetCurve", "", GH_ParamAccess.item);
            // 1
            pManager.AddPointParameter("points on crv", "ptsOnCrv", "", GH_ParamAccess.list);
            // 2
            pManager.AddCurveParameter("line normal", "nor", "line normal at point of division", GH_ParamAccess.list);
            // 3
            pManager.AddCurveParameter("poly from sub-div of crvs", "poly", "polygons from the sub-divions of curves", GH_ParamAccess.list);
            // 4
            pManager.AddTextParameter("debug", "debug", "", GH_ParamAccess.item);
            // 5
            pManager.AddBrepParameter("final Boundary representation", "final brep", "Generated boundary representation", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve site_ = null;
            double setback = double.NaN;
            double OFFSET_INP = double.NaN;
            int numDiv = 10;
            int numPeaks = 10;
            double minAr = double.NaN;

            if (!DA.GetData(0, ref site_)) return;
            if (!DA.GetData(1, ref setback)) return;
            if (!DA.GetData(2, ref OFFSET_INP)) return;
            if (!DA.GetData(3, ref numDiv)) return;
            if (!DA.GetData(4, ref numPeaks)) return;
            if (!DA.GetData(5, ref minAr)) return;

            Curve site = Rhino.Geometry.Curve.ProjectToPlane(site_, Plane.WorldXY);

            //CHECKS FOR THE SITE-SETBACK AND MIN SITE AREA1
            double siteAr = AreaMassProperties.Compute(site).Area;
            Point3d site_cen = AreaMassProperties.Compute(site).Centroid;
            if (siteAr < minAr) return; // area restraint
            Curve[] site_setback_crv = site.Offset(site_cen, Vector3d.ZAxis, setback,
                Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, CurveOffsetCornerStyle.Sharp);
            if (site_setback_crv.Length != 1) return; // rational offset restraint

            //OFFSET FROM THE SITE BOUNDARY
            Curve c2 = site_setback_crv[0].DuplicateCurve(); // duplicate of setback curve : outer boundary of building
            Curve[] c2Offs;
            Point3d cen = AreaMassProperties.Compute(c2).Centroid;
            Rhino.Geometry.PointContainment cont = site.Contains(cen, Plane.WorldXY,
                Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            if (cont.ToString() == "Inside")
            {
                c2Offs = c2.Offset(cen, Vector3d.ZAxis, OFFSET_INP, 0.01, CurveOffsetCornerStyle.Sharp);
            }
            else
            {
                c2Offs = c2.Offset(cen, Vector3d.ZAxis, -OFFSET_INP, 0.01, CurveOffsetCornerStyle.Sharp);
            }
            DA.SetData(4, cont);            // ----------------------------------------------
            Curve OFFSET_CRV = c2Offs[0]; //inner boundary of building
            if (c2Offs.Length != 1) return;
            DA.SetData(0, OFFSET_CRV);

            List<Curve> crv_li = new List<Curve> {c2, OFFSET_CRV };
            DA.SetDataList(2, crv_li);

            Polyline outer_crv;
            Polyline inner_crv;

            bool t0 = c2.TryGetPolyline(out outer_crv);
            bool t1 = OFFSET_CRV.TryGetPolyline(out inner_crv);


            List<Brep> fbrepLi = new List<Brep>();

            if (t0 == true)
            {
                List<Point3d> outerPtLi = new List<Point3d>();
                IEnumerator<Point3d> outerPts = outer_crv.GetEnumerator();
                while (outerPts.MoveNext())
                {
                    outerPtLi.Add(outerPts.Current);
                }
                List<Point3d> innerPtLi = new List<Point3d>();
                IEnumerator<Point3d> innerPts = inner_crv.GetEnumerator();
                while (innerPts.MoveNext())
                {
                    innerPtLi.Add(innerPts.Current);
                }

                PolylineCurve outerPoly = new PolylineCurve(outerPtLi);
                try
                {
                    double t = (double)(1.00 / numDiv);
                    if (t < 1.0 && t > 0.005)
                    {
                        int numDivisionPts = (int)numDiv;
                        fbrepLi = SolveForPolyLineCrv(outerPtLi, innerPtLi, numDivisionPts, numPeaks, OFFSET_INP);
                    }
                }
                catch (Exception) { }
                
                
            }
            else
            {
                try
                {
                    fbrepLi = SolveForSmoothCrv(c2, OFFSET_CRV, numDiv, numPeaks, OFFSET_INP);
                }
                catch (Exception) { }
                
            }
            DA.SetDataList(4, (1/numDiv).ToString());       // ----------------------------------------------
            DA.SetDataList(5, fbrepLi);                     // ----------------------------------------------
            DA.SetDataList(1, globalPtCrvLi);               // ----------------------------------------------
        }

        public List<Brep> SolveForPolyLineCrv(List<Point3d> outerPtLi, List<Point3d> innerPtLi,  int numDiv, int numPeaks, double offset_inp)
        {
            globalPtCrvLi = new List<Point3d>();
            List<PolylineCurve> polyLi = new List<PolylineCurve>();
            for (int i = 0; i < outerPtLi.Count - 1; i++)
            {
                Point3d p = outerPtLi[i];
                Point3d q = outerPtLi[i + 1];
                Point3d a = innerPtLi[i];
                Point3d b = innerPtLi[i + 1];
                double t = (double)(1.00 / numDiv);
                List<Point3d> inner_subLi = new List<Point3d>();
                List<Point3d> outer_subLi = new List<Point3d>();
                for (double j = 0.0; j < 1.0; j += t)
                {
                    double x = a.X + (b.X - a.X) * j;
                    double y = a.Y + (b.Y - a.Y) * j;
                    Point3d A = new Point3d(x, y, 0); //a+j*(b-a)
                    globalPtCrvLi.Add(A);
                    inner_subLi.Add(A);
                }
                for (double j = 0.0; j < 1.0; j += t)
                {
                    double x = p.X + (q.X - p.X) * j;
                    double y = p.Y + (q.Y - p.Y) * j;
                    Point3d A = new Point3d(x, y, 0); //a+j*(b-a)
                    globalPtCrvLi.Add(A);
                    outer_subLi.Add(A);
                }
                for(int j=0; j<outer_subLi.Count-1; j++)
                {
                    Point3d A = inner_subLi[j];
                    Point3d B = inner_subLi[j + 1];
                    Point3d P = outer_subLi[j];
                    Point3d Q = outer_subLi[j + 1];
                    List<Point3d> pts = new List<Point3d> { A, B, Q, P, A };
                    PolylineCurve poly = new PolylineCurve(pts);
                    polyLi.Add(poly);
                }
            }

            List<Brep> fBrepLi = new List<Brep>();
            for (int i=0; i<polyLi.Count; i++)
            {
                PolylineCurve poly = polyLi[i];
                Extrusion extr = Rhino.Geometry.Extrusion.Create(poly, 10, true);
                Brep brep = extr.ToBrep();
                fBrepLi.Add(brep);
            }

            return fBrepLi;
        }

        public List<Brep> SolveForSmoothCrv(Curve c2, Curve OFFSET_CRV, int numDiv, int numPeaks, double OFFSET_INP) {
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
                Rhino.Geometry.PointContainment contU = c2.Contains(u);//, Plane.WorldXY, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
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
            List<PolylineCurve> fPolyLi = new List<PolylineCurve>();
            int numSel = numPeaks;
            for (int i = 0; i < numSel; i++)
            {
                int idx = rnd.Next(polyCrvLi.Count);
                fPolyLi.Add(polyCrvLi[idx]);
            }


            List<Brep> fbrepLi = new List<Brep>();
            for (int i = 0; i < fPolyLi.Count; i++)
            {
                PolylineCurve crv = fPolyLi[i];
                Extrusion extr0 = Extrusion.Create(crv, -OFFSET_INP * 3, true);
                var t0 = extr0.GetBoundingBox(true);
                if (t0.Max.Z <= 0)
                {
                    extr0 = Extrusion.Create(crv, OFFSET_INP * 3, true);
                }
                Brep brep = extr0.ToBrep();
                //Brep brep = Rhino.Geometry.Extrusion.Create(crv, -OFFSET_INP * 3, true).ToBrep();
                Transform xform = Rhino.Geometry.Transform.Translation(0, 0, OFFSET_INP);
                brep.Transform(xform);
                fbrepLi.Add(brep);
            }
            try
            {
                Extrusion outerExtr = Rhino.Geometry.Extrusion.Create(c2, -OFFSET_INP, true);
                var t0 = outerExtr.GetBoundingBox(true);
                if (t0.Max.Z <= 0)
                {
                    outerExtr = Rhino.Geometry.Extrusion.Create(c2, OFFSET_INP, true);
                }
                Brep outerBrep = outerExtr.ToBrep();
                //Brep outerBrep = Rhino.Geometry.Extrusion.Create(site, -OFFSET_INP, true).ToBrep();

                Extrusion innerExtr = Rhino.Geometry.Extrusion.Create(OFFSET_CRV, -OFFSET_INP, true);
                var t1 = innerExtr.GetBoundingBox(true);
                if (t1.Max.Z <= 0)
                {
                    innerExtr = Rhino.Geometry.Extrusion.Create(OFFSET_CRV, OFFSET_INP, true);
                }
                Brep innerBrep = innerExtr.ToBrep();
                // Brep innerBrep = Rhino.Geometry.Extrusion.Create(OFFSET_CRV, -OFFSET_INP, true).ToBrep();
                Brep[] netBrepArr = Brep.CreateBooleanDifference(outerBrep, innerBrep, 0.01);
                Brep netBrep = netBrepArr[0];
                fbrepLi.Add(netBrep);
            }
            catch (Exception) { }
            return fbrepLi;
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.ufgStagerredCourtyardExtr; } }

        public override Guid ComponentGuid { get { return new Guid("48a70716-4b69-4b4c-8440-24be62b00616"); } }
    }
}
