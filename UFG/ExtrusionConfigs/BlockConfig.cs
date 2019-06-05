using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Config
{
    public class BlockConfig : GH_Component
    {
        public BlockConfig()
          : base("extrude-block", "block",
              "Extrude block from site, given setback",
              "DOTS", "UFG")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        { 
            pManager.AddCurveParameter(
                "Site Curve", 
                "crv", 
                "sites for placing the blocks", 
                GH_ParamAccess.list
                );

            pManager.AddNumberParameter(
                "Setback", 
                "setback", 
                "setback for the sites", 
                GH_ParamAccess.item
                );

            pManager.AddNumberParameter(
                "FSR", 
                "fsr/far", 
                "FSR: floor-space ratio or FAR: floor-area-ratio", 
                GH_ParamAccess.item
                );
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter(
                "output Extruded Mass",
                "Block",
                "extruded mass / blocks",
                GH_ParamAccess.list
                );
            pManager.AddTextParameter(
                "debug",
                "debug",
                "debug numbers",
                GH_ParamAccess.item
                );
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> sites = new List<Curve>();
            double fsr = double.NaN;
            double setback = double.NaN;

            if (!DA.GetDataList(0, sites)) return;
            if (!DA.GetData(1, ref setback)) return;
            if (!DA.GetData(2, ref fsr)) return;

            List<Extrusion> massLi = new List<Extrusion>();

            string msg = "";
            msg += "\nfsr: " + fsr.ToString();
            msg += "\nsetback: " + setback.ToString();

            for(int i=0; i<sites.Count; i++)
            {
                Point3d cen = Rhino.Geometry.AreaMassProperties.Compute(sites[i]).Centroid;
                var offsetCrv = sites[i].Offset(
                    cen,
                    Vector3d.ZAxis,
                    setback,
                    Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                    CurveOffsetCornerStyle.Sharp
                );
                double arSite = Rhino.Geometry.AreaMassProperties.Compute(sites[i]).Area;
                double arOffset = AreaMassProperties.Compute(offsetCrv[0]).Area;
                double ht = fsr*arSite/arOffset;
                Extrusion mass = Extrusion.Create(offsetCrv[0], ht, true);
                msg += "\nar site: " + arSite.ToString() + "ar offset: "+arOffset.ToString() + "ht: "+ht.ToString();
                massLi.Add(mass);
            }
            DA.SetDataList(0, massLi);
            DA.SetData(1, msg);
        }

        protected override System.Drawing.Bitmap Icon { get { return null; } }

        public override Guid ComponentGuid { get { return new Guid("c81f5263-86b7-48a3-b3d7-6b462f5658f9"); } }

    }
}
