using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj
{
    public class GenMultipleTypologies : GH_Component
    {
        public GenMultipleTypologies()
          : base("gen. Formal Typology", "generate typologies",
              "Generates massing with numerous pre-defined formal typologies",
              "DOTS", "UFG")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0. SITE CURVE
            pManager.AddCurveParameter("Site Curve", "site-crv", "Enter the closed planar site boundary curve", GH_ParamAccess.item);
            // 1. FSR / FAR
            pManager.AddNumberParameter("FSR  required", "fsr/far", "Floor Space Ratio or Floor area ratio (FAR)", GH_ParamAccess.item);
            // 2. SETBACK FROM BOUNDARY
            // pManager.AddNumberParameter("Setback from boundary", "setback", "general setback required", GH_ParamAccess.item);
            // 3. DEPTH OF FLOOR 
            pManager.AddNumberParameter("Depth of floor", "flr-depth", "maximum depth of the floor (1 bay) required", GH_ParamAccess.item);
            // 4. GAP BETWEEN BAYS
            pManager.AddNumberParameter("Gap between bays", "bay-spacing", "maximum depth of the space between bays (corridor / courtyard / light-well, etc)", GH_ParamAccess.item);
            // 5. STEPBACKS FOR MASSING
            pManager.AddTextParameter("Setback & Stepbacks for Massing", "stepbacks", "enter as text using panels; array of stepbacks based on height", GH_ParamAccess.item);
            // 6. HEIGHTS THAT CORRESPOND TO STEPBACKS
            pManager.AddTextParameter("Heights corresponding to Stepbacks", "heights", "enter as text using panels; array of heights that correspond to stepback dimensions", GH_ParamAccess.item);
            // 7. FLOOR TO FLOOR HEIGHT
            pManager.AddNumberParameter("Floor Height", "f2f-ht", "Floor Height of each storey or floor to floor height", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Output Floor Curves", "floors", "output floor plates", GH_ParamAccess.list);
            pManager.AddBrepParameter("Output Massing Breps", "massing", "output massing from floor plates", GH_ParamAccess.list);
            pManager.AddBrepParameter("Stagerred Massing Output", "massing", "output massing by stagerred stepbacks", GH_ParamAccess.list);
            pManager.AddTextParameter("debug text", "debug", "msg from system", GH_ParamAccess.item);
            pManager.AddCurveParameter("f-crvs","crvs","stagerred block result crvs",GH_ParamAccess.list);
            pManager.AddBrepParameter("f-breps", "breps", "stagerred block result breps", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve siteCrv = null;
            double fsr = double.NaN;
            double depthFlr = double.NaN;
            double gapBays = double.NaN;
            string stepbackStr = "";
            string stepbackHtStr = "";
            List<double> stepbacks = new List<double>();
            List<double> stepbackHts = new List<double>();
            double flrHt = 3.0;

            if (!DA.GetData(0, ref siteCrv)) return;
            if (!DA.GetData(1, ref fsr)) return;
            if (!DA.GetData(2, ref depthFlr)) return;
            if (!DA.GetData(3, ref gapBays)) return;
            if (!DA.GetData(4, ref stepbackStr)) return;
            if (!DA.GetData(5, ref stepbackHtStr)) return;
            if (!DA.GetData(6, ref flrHt)) return;

            string[] stepbackArr = stepbackStr.Split(',');
            for (int i = 0; i < stepbackArr.Length; i++)
            {
                double x = Convert.ToDouble(stepbackArr[i]);
                stepbacks.Add(x);
            }

            string[] stepbackHtArr = stepbackHtStr.Split(',');
            for (int i = 0; i < stepbackHtArr.Length; i++)
            {
                double x = Convert.ToDouble(stepbackHtArr[i]);
                stepbackHts.Add(x);
            }

            string msg = "System Messages:\n";
            TypologyMethods typologyMethods = new TypologyMethods(siteCrv, fsr, depthFlr, gapBays, stepbacks, stepbackHts, flrHt);
            if (fsr > 0)
            {
                typologyMethods.GenExtrBlock();

                typologyMethods.GenerateCourtyardBlock();
                List<Brep> brepLi = typologyMethods.GetGeneratedSolids();
                DA.SetDataList(1, brepLi);

                try
                {
                    List<Brep> brepLi2 = typologyMethods.GenStagerredBlock();
                    DA.SetDataList(2, brepLi2);
                }
                catch (Exception) { msg += "\nerror in stagerred block"; }
                
                List<Curve> crvLi = typologyMethods.GetGeneratedCrvs();
                DA.SetDataList(0, crvLi);
                msg += "\nbrep found\n";
            }
            else {  msg += "\nerror in inputs\n"; }
            msg+= typologyMethods.getMsg();
            DA.SetData(3, msg);


            List<Curve> dupCrvLi = new List<Curve>();
            List<Brep> dupBrepLi = new List<Brep>();
            Curve c0 = siteCrv.DuplicateCurve();
            for(int i=0; i<stepbacks.Count; i++)
            {
                Point3d cen = AreaMassProperties.Compute(c0).Centroid;
                Curve[] crv = c0.Offset(cen, Vector3d.ZAxis, stepbacks[i], 0.01, CurveOffsetCornerStyle.Sharp);
                Brep brep = Rhino.Geometry.Extrusion.Create(crv[0], -stepbackHts[i], true).ToBrep();
                dupCrvLi.Add(crv[0]);
                //dupBrepLi.Add(brep);
            }
            DA.SetDataList(4, dupCrvLi);

            dupBrepLi = typologyMethods.GenStagerredBlock();
            DA.SetDataList(5, dupBrepLi);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.typologies; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("274f62e5-889b-4938-9646-24b80ca4a16b"); }
        }
    }
}