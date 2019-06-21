using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj.SourceCode.UFG
{
    public class Analysis : GH_Component
    {
        public Analysis()
          : base("Analysis", "Isovist",
              "Isovist",
              "DOTS", "Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("914e9703-e55a-4702-8c37-8ae71da747e0"); }
        }
    }
}