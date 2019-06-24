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
             "DOTS", "Massing")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0. SITE CURVE
            pManager.AddCurveParameter("Site Curve", "site-crv", "Enter the closed planar site boundary curve", GH_ParamAccess.item);
            // 1. FLOOR HEIGHT
            pManager.AddNumberParameter("Floor height required", "flr-ht", "Floor Height Required", GH_ParamAccess.item);
            // 2. STEPBACKS FOR MASSING
            pManager.AddTextParameter("Setback & Stepbacks for Massing", "stepbacks", "enter as text using panels; array of stepbacks based on height", GH_ParamAccess.item);
            // 3. HEIGHTS THAT CORRESPOND TO STEPBACKS
            pManager.AddTextParameter("Heights corresponding to Stepbacks", "heights", "enter as text using panels; array of heights that correspond to stepback dimensions", GH_ParamAccess.item);
            // 4. RESTRAINT: MIN AREA 
            pManager.AddNumberParameter("Min.Area", "min-ar", "Restraint: Minimum area required", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0. floor plate curves
            pManager.AddCurveParameter("Output Floor Curves", "floors", "output floor plates", GH_ParamAccess.list);
            // 1. brep as massing
            pManager.AddBrepParameter("Output Massing Breps", "massing", "output massing from floor plates", GH_ParamAccess.list);
            // 2. brep as massing
            pManager.AddTextParameter("Output (text panel) Total FSR & GFA Achieved", "fsr_gfa", "output fsr and gfa from floor plates", GH_ParamAccess.item);
            // 3. msg from system
            pManager.AddTextParameter("debug text", "debug", "msg from system", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve siteCrv = null;
            double flrHt = double.NaN;
            double minAr = double.NaN;
            string stepbackstr = "2,3,4";
            string htstr = "2,3,4";
            string outputMsg = "";
            List<double> stepbackLi = new List<double>();
            List<double> htLi = new List<double>();

            if (!DA.GetData(0, ref siteCrv)) return;
            //if (!DA.GetData(1, ref fsr)) return;
            if (!DA.GetData(1, ref flrHt)) return;
            // if (!DA.GetData(3, ref stepbackstr)) return;
            // if (!DA.GetData(4, ref htstr)) return;
            bool t0 = DA.GetData(2, ref stepbackstr);
            bool t1 = DA.GetData(3, ref htstr);
            if (!DA.GetData(4, ref minAr)) return;

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
            double flrItr = 0.0; // continuous ht counter for floors
            try
            {
                for (int i = 0; i < stepbackLi.Count; i++)
                {
                    Curve c0_ = siteCrv.DuplicateCurve();
                    Curve c0 = Rhino.Geometry.Curve.ProjectToPlane(c0_, Plane.WorldXY);
                    double got_ar = AreaMassProperties.Compute(c0).Area;
                    if (got_ar < minAr) { continue; } // this curve has less area than min specs
                    Point3d cen = AreaMassProperties.Compute(c0).Centroid;

                    double di = stepbackLi[i];
                    if (i > 0 && stepbackLi[i] < stepbackLi[i-1]) { stepbackLi[i - 1] += 1; }
                    Curve[] c1 = c0.Offset(cen, Vector3d.ZAxis, di, 0.01, CurveOffsetCornerStyle.Sharp);
                    Curve c2_ = null;
                    Curve c2 = null;
                    if (c1.Length != 1)
                    {
                        c2 = c0;
                    }
                    else
                    {
                        c2_ = c1[0].DuplicateCurve();
                        c2 = Curve.ProjectToPlane(c2_, Plane.WorldXY);
                    }
                    double ht = htLi[i];
                    if(ht ==0) { continue; }
                    double numFlrs = ht / flrHt;
                    for (int j = 0; j < numFlrs; j++)
                    {
                        Curve c3 = c2.DuplicateCurve();
                        Rhino.Geometry.Transform xform_itr = Rhino.Geometry.Transform.Translation(0, 0, flrItr);
                        c3.Transform(xform_itr);
                        flrCrvLi.Add(c3);
                        flrItr += flrHt;
                    }
                    flrReqLi.Add(numFlrs.ToString());
                    Extrusion mass = Rhino.Geometry.Extrusion.Create(c2, ht, true);  //
                    var B = mass.GetBoundingBox(true);
                    if (B.Max.Z < 0.01)
                    {
                        mass = Extrusion.Create(c2, -ht, true);  //
                    }
                    Brep brep = mass.ToBrep();
                    Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0, 0, spineht);
                    brep.Transform(xform);
                    brepLi.Add(brep);
                    spineht += ht;
                }
                DA.SetDataList(0, flrCrvLi);
                DA.SetDataList(1, brepLi);
            }
            catch (Exception) { }

            double grossFlrAr = 0.0;
            for(int i=0; i<flrCrvLi.Count; i++)
            {
                grossFlrAr += AreaMassProperties.Compute(flrCrvLi[i]).Area;
            }
            double siteAr = AreaMassProperties.Compute(siteCrv).Area;
            double gotfsr = grossFlrAr / siteAr;
            outputMsg = "Gross Floor Area = " + Math.Round(grossFlrAr, 2).ToString() + 
                ", Got fsr / far = " + Math.Round(gotfsr, 2).ToString();
            DA.SetData(2, outputMsg);
            DA.SetDataList(3, flrReqLi);
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