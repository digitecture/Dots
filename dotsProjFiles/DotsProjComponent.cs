using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj
{
    public class DotsProjComponent : GH_Component
    {
        public DotsProjComponent()
          : base("Design Optimization Tool Set", "DOTS",
              "Credits & Acknowledgements",
              "DOTS", "Generate Urban Layouts & Massing")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("About", "about", "creators: Georgia Institute of Technology & Perkins + Will", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string s = "plugin development & conceptualization: nirvik saha (GIT, P+W)";
            s += "\nAcademic Advisers: Dennis Shelden (GIT), John Haymaker (P+W)";
            s += "\nDesign Collaboration & Acknowledgements (P+W) : Victor Okhoya, Tyrone Marshall, Mahdiar Ghaffarian, Marcelo Bernal, Andy Gavel, Hanna Gibson";
            DA.SetData(0, s);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.dots;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("662af2b4-68f0-4f03-b461-babac0d6172f"); }
        }
    }
}
