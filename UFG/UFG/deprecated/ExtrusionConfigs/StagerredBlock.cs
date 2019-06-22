using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj.SourceCode.UFG.ExtrusionConfigs
{
    public class StagerredBlock : GH_Component
    {
        public StagerredBlock()
          : base("Stagerred Massing", "stagerred-mass",
              "Generates stagerred massing",
             "DOTS", "UFG")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0. SITE CURVE
            pManager.AddCurveParameter("Site Curve", "site-crv", "Enter the closed planar site boundary curve", GH_ParamAccess.item);
            // 1. FSR / FAR
            pManager.AddNumberParameter("FSR  required", "fsr/far", "Floor Space Ratio or Floor area ratio (FAR)", GH_ParamAccess.item);
            // 2. FLOOR HEIGHT
            pManager.AddNumberParameter("Floor height required", "flr-ht", "Floor Height Required", GH_ParamAccess.item);
            // 3. STEPBACKS FOR MASSING
            pManager.AddTextParameter("Setback & Stepbacks for Massing", "stepbacks", "enter as text using panels; array of stepbacks based on height", GH_ParamAccess.item);
            // 4. HEIGHTS THAT CORRESPOND TO STEPBACKS
            pManager.AddTextParameter("Heights corresponding to Stepbacks", "heights", "enter as text using panels; array of heights that correspond to stepback dimensions", GH_ParamAccess.item);
            
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0. floor plate curves
            pManager.AddCurveParameter("Output Floor Curves", "floors", "output floor plates", GH_ParamAccess.list);
            // 1. brep as massing
            pManager.AddBrepParameter("Output Massing Breps", "massing", "output massing from floor plates", GH_ParamAccess.list);
            // 2. msg from system
            pManager.AddTextParameter("debug text", "debug", "msg from system", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve siteCrv = null;
            double fsr = double.NaN;
            double flrHt = double.NaN;
            string stepbackstr = "";
            string htstr = "";
            List<double> stepbackLi = new List<double>();
            List<double> htLi = new List<double>();

            if (!DA.GetData(0, ref siteCrv)) return;
            if (!DA.GetData(1, ref fsr)) return;
            if (!DA.GetData(2, ref flrHt)) return;
            if (!DA.GetData(3, ref stepbackstr)) return;
            if (!DA.GetData(4, ref htstr)) return;

            string[] stepbackArr = stepbackstr.Split(',');
            for(int i=0; i<stepbackArr.Length; i++)
            {
                double x = Convert.ToDouble(stepbackArr[i]);
                stepbackLi.Add(x);
            }

            string[] htArr = htstr.Split(',');
            for (int i = 0; i < htArr.Length; i++)
            {
                double x = Convert.ToDouble(htArr[i]);
                htLi.Add(x);
            }
            List<string> flrReqLi = new List<string>();
            List<Brep> brepLi = new List<Brep>();
            List<Curve> flrCrvLi = new List<Curve>();
            double spineht = 0.0;
            double flrItr = 0.0;
            for(int i=0; i<stepbackLi.Count; i++)
            {
                Curve c0 = siteCrv.DuplicateCurve();
                Point3d cen = AreaMassProperties.Compute(c0).Centroid;
                double di = stepbackLi[i];
                Curve[] c1 = c0.Offset(cen, Vector3d.ZAxis, di, 0.01, CurveOffsetCornerStyle.Sharp);
                double ht = htLi[i];
                double numFlrs = ht / flrHt;
                for(int j=0; j<numFlrs; j++)
                {
                    Curve c2 = c1[0].DuplicateCurve();
                    Rhino.Geometry.Transform xform_itr = Rhino.Geometry.Transform.Translation(0,0,flrItr);
                    c2.Transform(xform_itr);
                    flrCrvLi.Add(c2);
                    flrItr += flrHt;
                }
                flrReqLi.Add(numFlrs.ToString());
                Brep brep = Rhino.Geometry.Extrusion.Create(c1[0], -ht, true).ToBrep();
                Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0, 0, spineht);
                brep.Transform(xform);
                brepLi.Add(brep);
                //crvLi.Add(c1[0]);
                spineht += ht;
            }

            DA.SetDataList(0, flrCrvLi);
            DA.SetDataList(1, brepLi);
            DA.SetDataList(2, flrReqLi);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.ufgstagerredExtr;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("49df1540-3e34-4552-939e-1a8ce311bc26"); }
        }
    }
}