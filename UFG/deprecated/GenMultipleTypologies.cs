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
            pManager.AddNumberParameter("Setback from boundary", "setback", "general setback required", GH_ParamAccess.item);
            // 3. DEPTH OF FLOOR 
            pManager.AddNumberParameter("Depth of floor", "flr-depth", "maximum depth of the floor (1 bay) required", GH_ParamAccess.item);
            // 4. GAP BETWEEN BAYS
            pManager.AddNumberParameter("Gap between bays", "bay-spacing", "maximum depth of the space between bays (corridor / courtyard / light-well, etc)", GH_ParamAccess.item);
            // 5. STEPBACKS FOR MASSING
            pManager.AddTextParameter("Stepbacks for Massing", "stepbacks", "enter as text using panels; array of stepbacks based on height", GH_ParamAccess.item);
            // 6. HEIGHTS THAT CORRESPOND TO STEPBACKS
            pManager.AddTextParameter("Heights corresponding to Stepbacks", "heights", "enter as text using panels; array of heights that correspond to stepback dimensions", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Output Floor Curves", "floors", "output floor plates", GH_ParamAccess.list);
            pManager.AddBrepParameter("Output Massing Breps", "massing", "output massing from floor plates", GH_ParamAccess.list);
            pManager.AddTextParameter("debug text", "debug", "msg from system", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve siteCrv = null;
            double fsr = double.NaN;
            double setback = double.NaN;
            double depthFlr = double.NaN;
            double gapBays = double.NaN;
            string stepbackStr = "";
            string stepbackHtStr = "";
            List<double> stepbacks = new List<double>();
            List<double> stepbackHts = new List<double>();

            if (!DA.GetData(0, ref siteCrv)) return;
            if (!DA.GetData(1, ref fsr)) return;
            if (!DA.GetData(2, ref setback)) return;
            if (!DA.GetData(3, ref depthFlr)) return;
            if (!DA.GetData(4, ref gapBays)) return;
            if (!DA.GetData(5, ref stepbackStr)) return;
            if (!DA.GetData(6, ref stepbackHtStr)) return;

            string str = "inputs:\n";
            str += Math.Round(fsr, 2).ToString() + "\n";
            str += Math.Round(setback, 2).ToString() + "\n";
            str += Math.Round(depthFlr, 2).ToString() + "\n";
            str += Math.Round(gapBays, 2).ToString() + "\n";

            string str2 = "Stepbacks: ";
            string[] stepbackArr = stepbackStr.Split(',');
            for (int i = 0; i < stepbackArr.Length; i++)
            {
                double x = Convert.ToDouble(stepbackArr[i]);
                stepbacks.Add(x);
                str2 += Math.Round(x, 2).ToString() + ",";
            }

            str += str2;

            string str3 = "Stepback Heights: ";
            string[] stepbackHtArr = stepbackHtStr.Split(',');
            for (int i = 0; i < stepbackHtArr.Length; i++)
            {
                double x = Convert.ToDouble(stepbackHtArr[i]);
                stepbackHts.Add(x);
                str3 += Math.Round(x, 2).ToString() + ",";
            }
            str += str3;


            TypologyMethods typologyMethods = new TypologyMethods(siteCrv, fsr, setback, depthFlr, gapBays, stepbacks, stepbackHts);

            if (setback > 0 && fsr > 0)
            {
                typologyMethods.GenExtrBlock();
                typologyMethods.GenerateCourtyardBlock();
                List<Brep> brepLi = typologyMethods.GetGeneratedSolids();
                DA.SetDataList(1, brepLi);
            }
            string msg = typologyMethods.getMsg();
            DA.SetData(2, msg);
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