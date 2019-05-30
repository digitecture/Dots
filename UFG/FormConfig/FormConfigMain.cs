using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DOTS.SourceCode.UFG.FormConfig
{
    public class FormConfig : GH_Component
    {
        public FormConfig()
          : base("Parcel-Partition Algorithm", "occupy-parcels",
              "Occupy parcels based on typologies",
              "DOTS", "UFG")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Input Curves", "iCrvs", "list of curve-boundaries to be occupied by required typology", GH_ParamAccess.list);
            pManager.AddNumberParameter("% FSR to Courtyard", "%-fsr-Courtyard", "Percentage of Fsrcourtyard", GH_ParamAccess.item);
        }
            
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        protected override System.Drawing.Bitmap Icon { get { return null; } } 

        public override Guid ComponentGuid { get { return new Guid("db513281-f7b3-4e79-88e6-ae9b2eafb9d5"); } }

    }
}
