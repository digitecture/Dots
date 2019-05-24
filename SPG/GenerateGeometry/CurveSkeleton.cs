using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Test2
{
    public class CurveSkeleton : GH_Component
    {
        public CurveSkeleton()
          : base("CurveSkeleton", "csa",
              "Description",
              "DOTS", "CSA")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Get Boundary", "boundary", "set the boundary", GH_ParamAccess.item);
            pManager.AddNumberParameter("Offset Amount", "offset", "amount of offset from the boundary", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Offset curve", "crv", "This is the offset output", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crvA = new PolyCurve();
            double off = double.NaN;

            DA.GetData(0, ref crvA);
            DA.GetData(1, ref off);

            Curve[] crvB = crvA.Offset(Plane.WorldXY, off, 0.01, 0);
            DA.SetDataList(0, crvB);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return null; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("8e067c9c-2187-44ba-a5ff-c2b9c85982b1"); }
        }
    }
}
