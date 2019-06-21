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
        List<Point3d> globalPtCrvLi= new List<Point3d>();
        List<Curve> globalBaseCrvLi = new List<Curve>();
        List<Curve> globalTowerCrvLi = new List<Curve>();
        double SITE_AR = 1.0;

        string debugMsg = "";


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
            // 4. number of towers
            pManager.AddIntegerParameter("number of towers", "num-towers", "number of towers to be extruded", GH_ParamAccess.item);
            // 5. flr Ht
            pManager.AddNumberParameter("floor ht", "floor-floor height", "floor to floor height", GH_ParamAccess.item);
            // 6. base fsr
            pManager.AddNumberParameter("base FSR (FAR)", "base-fsr", "required FSR of the base", GH_ParamAccess.item);
            // 7. tower fsr
            pManager.AddNumberParameter("tower FSR (FAR)", "tower-fsr", "required FSR of the tower", GH_ParamAccess.item);
            // 8. min area of curve
            pManager.AddNumberParameter("minimum-area-crv", "min-ar-curve", "Restraint: Minimum area if the curve", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0  offset curves
            pManager.AddCurveParameter("offsetcrv", "offsetCurve", "", GH_ParamAccess.item);
            // 1 base floors 
            pManager.AddCurveParameter("base floor curves", "base-flr-crvs", "", GH_ParamAccess.list);
            // 2 tower floors
            pManager.AddCurveParameter("tower floor curves", "tower-flr-crvs", "floor curves from the tower", GH_ParamAccess.list);
            // 3 poly from sub-div of crvs
            pManager.AddCurveParameter("poly from sub-div of crvs", "poly", "polygons from the sub-divions of curves", GH_ParamAccess.list);
            // 4 debug system msgs
            pManager.AddTextParameter("debug", "debug", "", GH_ParamAccess.item);
            // 5 final breps
            pManager.AddBrepParameter("final Boundary representation", "final brep", "Generated boundary representation", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve site_ = null;
            double setback = double.NaN;
            double OFFSET_INP = double.NaN;
            double baseFsr = double.NaN;
            double towerFsr = double.NaN;
            int numDiv = 10;
            int numTowers= 7;
            double flrHt = double.NaN;
            double minAr = double.NaN;

            if (!DA.GetData(0, ref site_)) return;
            if (!DA.GetData(1, ref setback)) return;
            if (!DA.GetData(2, ref OFFSET_INP)) return;
            if (!DA.GetData(3, ref numDiv)) return;
            if (!DA.GetData(4, ref numTowers)) return;
            if (!DA.GetData(5, ref flrHt)) return;
            if (!DA.GetData(6, ref baseFsr)) return;
            if (!DA.GetData(7, ref towerFsr)) return;
            if (!DA.GetData(8, ref minAr)) return;

            Curve site = Rhino.Geometry.Curve.ProjectToPlane(site_, Plane.WorldXY);

            

            //CHECKS FOR THE SITE-SETBACK AND MIN SITE AREA1
            double siteAr = AreaMassProperties.Compute(site).Area;
            SITE_AR= AreaMassProperties.Compute(site).Area;
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

            Polyline outer_crv;
            Polyline inner_crv;
            bool t0 = c2.TryGetPolyline(out outer_crv);
            bool t1 = OFFSET_CRV.TryGetPolyline(out inner_crv);
            List<Brep> fbrepLi = new List<Brep>();
            if (t0 == true)
            {
                debugMsg += "in poly solver";
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
                double t = (double)(1.00 / numDiv);
                if (t < 1.0 && t > 0.005)
                {
                    int numDivisionPts = (int)numDiv;
                    fbrepLi = SolveForPolyLineCrv(outerPtLi, innerPtLi, numDivisionPts, numTowers,
                        flrHt, OFFSET_INP, baseFsr, towerFsr);
                }
            }
            else
            {
                debugMsg += "in smooth solver";
                try
                {
                    //fbrepLi = SolveForSmoothCrv(c2, OFFSET_CRV, numDiv, numTowers, flrHt, OFFSET_INP, baseFsr, towerFsr);
                }
                catch (Exception) {
                    debugMsg += "error in smooth solver";
                }
                fbrepLi = SolveForSmoothCrv(c2, OFFSET_CRV, numDiv, numTowers, flrHt, OFFSET_INP, baseFsr, towerFsr);
            }
            DA.SetDataList(4, debugMsg);       // ----------------------------------------------
            DA.SetDataList(5, fbrepLi);                     // ----------------------------------------------
            //DA.SetDataList(1, globalPtCrvLi);               // ----------------------------------------------
            DA.SetDataList(1, globalBaseCrvLi);
            DA.SetDataList(2, globalTowerCrvLi);
        }

        public List<Brep> SolveForPolyLineCrv(List<Point3d> outerPtLi, List<Point3d> innerPtLi,  
            int numDiv, int numTowers, double flrHt, double offset_inp, double baseFsr, double towerFsr)
        {

            globalBaseCrvLi = new List<Curve>();
            globalTowerCrvLi = new List<Curve>();
            globalPtCrvLi = new List<Point3d>();

            PolyCurveSolver solver = new PolyCurveSolver(outerPtLi, innerPtLi, 
                numDiv, numTowers, flrHt, offset_inp, baseFsr, towerFsr, SITE_AR);
            
            //run the computations
            solver.Compute(); //updates global variables
            List<Brep> fbrepLi = solver.GetFinalBreps();
            globalBaseCrvLi = solver.globalBaseCrvLi;
            globalTowerCrvLi = solver.globalTowerCrvLi;
            globalPtCrvLi = solver.globalPtCrvLi;
            return fbrepLi;
        }

        public List<Brep> SolveForSmoothCrv(Curve c2, Curve OFFSET_CRV, int numDiv, int numTowers, 
            double flrHt, double OFFSET_INP, double baseFsr, double towerFsr) {

            SmoothCurveSolver solver = new SmoothCurveSolver(c2, OFFSET_CRV, numDiv, numTowers, 
                flrHt, OFFSET_INP, baseFsr, towerFsr, SITE_AR);

            List<Brep> fbrepLi = solver.Compute();
            globalBaseCrvLi = new List<Curve>();
            globalTowerCrvLi = new List<Curve>();
            globalPtCrvLi = new List<Point3d>();

            globalBaseCrvLi = solver.globalBaseCrvLi;
            globalTowerCrvLi = solver.globalTowerCrvLi;
            globalPtCrvLi = solver.globalPtCrvLi;
            return fbrepLi;
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.ufgStagerredCourtyardExtr; } }

        public override Guid ComponentGuid { get { return new Guid("48a70716-4b69-4b4c-8440-24be62b00616"); } }
    }
}
