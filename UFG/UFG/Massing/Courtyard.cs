using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj.SourceCode.UFG.ExtrusionConfigs
{
    public class Courtyard : GH_Component
    {
        public Courtyard()
          : base("Courtyard Massing", "courtyard-mass",
              "Generates Courtyard Massing",
              "DOTS", "Massing")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0. SITE CURVE
            pManager.AddCurveParameter("Site Curve", "site-crv", "Enter the site boundary curve", GH_ParamAccess.item);
            // 1. FSR / FAR
            pManager.AddNumberParameter("FSR / FAR", "fsr/far", "Floor space ratio / floor area ratio", GH_ParamAccess.item);
            // 2. FLR HT
            pManager.AddNumberParameter("Floor Height", "flr-ht", "floor to floor height / storey height", GH_ParamAccess.item);
            // 3. SETBACK
            pManager.AddNumberParameter("Setback", "setback", "setback required", GH_ParamAccess.item);
            // 4. BAY DEPTH
            pManager.AddNumberParameter("Bay Depth", "bay depth", "max depth of the bay", GH_ParamAccess.item);
            // 5. Slenderness Ratio
            pManager.AddNumberParameter("Max ht/floor-area", "ht/area", "Restriction: Slenderness Ratio Total Height / Area of 1 Floor", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0. floor plate curves
            pManager.AddCurveParameter("Output Floor Curves", "floors", "output floor plates", GH_ParamAccess.list);
            // 1. brep as massing
            pManager.AddBrepParameter("Output Massing Breps", "massing brep", "output massing from floor plates", GH_ParamAccess.list);
            // 2. msg from system
            // pManager.AddTextParameter("debug text 2", "debug 2", "msg from system", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve siteCrv = null;
            double fsr = double.NaN;
            double flrHt = double.NaN;
            double setback = double.NaN;
            double bayDepth = double.NaN;
            double slendernessRatio = double.NaN;

            if (!DA.GetData(0, ref siteCrv)) return;
            if (!DA.GetData(1, ref fsr)) return;
            if (!DA.GetData(2, ref flrHt)) return;
            if (!DA.GetData(3, ref setback)) return;
            if (!DA.GetData(4, ref bayDepth)) return;
            if (!DA.GetData(5, ref slendernessRatio)) return;

            Curve c0_ = siteCrv.DuplicateCurve();
            Curve c0 = Curve.ProjectToPlane(c0_, Plane.WorldXY);
            Point3d cen = AreaMassProperties.Compute(c0).Centroid;
            Curve[] outerCrvArr = c0.Offset(cen, Vector3d.ZAxis, setback, 0.01, CurveOffsetCornerStyle.Sharp);
            Curve[] innerCrvArr = outerCrvArr[0].Offset(cen, Vector3d.ZAxis, bayDepth, 0.01, CurveOffsetCornerStyle.Sharp);

            string debugMsg = "";
            try
            {
                if (innerCrvArr.Length != 1) { debugMsg += "\ninner crv error"; return; }
                if (outerCrvArr.Length != 1) { debugMsg += "\nouter crv error"; return; }
                double siteAr = AreaMassProperties.Compute(siteCrv).Area;
                double GFA = siteAr * fsr;
                double outerAr = AreaMassProperties.Compute(outerCrvArr[0]).Area;
                double innerAr = AreaMassProperties.Compute(innerCrvArr[0]).Area;
                double netAr = outerAr - innerAr;
                double numFlrs = GFA / netAr;
                double reqHt = numFlrs * flrHt;
                double gotSlendernessRatio = reqHt / netAr;
                if (gotSlendernessRatio < slendernessRatio) return;
                if (setback > 0 && bayDepth > 0 && flrHt > 0)
                {
                    List<string> numFlrReqLi = new List<string>();
                    List<Curve> crvLi = new List<Curve>();
                    double flrCounter = 0.0;
                    for (int i = 0; i < numFlrs; i++)
                    {
                        Curve c0crv = outerCrvArr[0].DuplicateCurve();
                        Curve c1crv = innerCrvArr[0].DuplicateCurve();
                        Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0, 0, flrCounter);
                        c0crv.Transform(xform);
                        c1crv.Transform(xform);
                        crvLi.Add(c0crv);
                        crvLi.Add(c1crv);
                        flrCounter += flrHt;
                    }
                    numFlrReqLi.Add(flrCounter.ToString());

                    List<Brep> brepLi = new List<Brep>();
                    Extrusion outerMass = Rhino.Geometry.Extrusion.Create(outerCrvArr[0], reqHt, true);
                    var B = outerMass.GetBoundingBox(true);
                    if (B.Max.Z < 0.01)
                    {
                        outerMass = Extrusion.Create(outerCrvArr[0], -reqHt, true);
                    }
                    Brep outerBrep = outerMass.ToBrep();

                    Extrusion innerMass = Rhino.Geometry.Extrusion.Create(innerCrvArr[0], reqHt, true);
                    var B2 = innerMass.GetBoundingBox(true);
                    if (B2.Max.Z < 0.01)
                    {
                        innerMass = Extrusion.Create(innerCrvArr[0], -reqHt, true);
                    }
                    Brep innerBrep = innerMass.ToBrep();

                    Brep[] netBrep = Brep.CreateBooleanDifference(outerBrep, innerBrep, 0.1);
                    try { brepLi.Add(netBrep[0]); }
                    catch (Exception)
                    {
                        debugMsg += "Error in brep subtraction";
                    }
                    DA.SetDataList(0, crvLi);
                    DA.SetDataList(1, brepLi);
                }
            }
            catch (Exception)
            {
                debugMsg += "Error in system";
            }
            // DA.SetDataList(2, debugMsg);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.ufgCourtyardExtr; } }

        public override Guid ComponentGuid { get { return new Guid("51d6b4bf-b51e-4469-a0f0-2313dfd7542b"); } }
    }
}