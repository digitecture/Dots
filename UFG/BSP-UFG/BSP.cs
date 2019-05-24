using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace UFG

{
    public class BSP : GH_Component
    {
        public BSP()
          : base("Parcel-Partition Algorithm", "bsp",
              "Street Grid Algorithm -1",
              "DOTS", "UFG")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0. input curves
            pManager.AddCurveParameter("input-site", "site", "street grids on site", GH_ParamAccess.item);
            // 1. standard deviation to restrict area of individual partition & boundary
            pManager.AddNumberParameter("std-dev-area", "std-dev Area", "standard deviation in AREA to restict parcels", GH_ParamAccess.item);
            // 2. standard deviation to restrict dimension of sides 
            pManager.AddNumberParameter("std-dev-dim", "std-dev Dimension", "standard deviation in DIMENSION to restrict parcels", GH_ParamAccess.item);
            // 3. min ratio of actual curve and boundary
            pManager.AddNumberParameter("ratio-crv-boundary", "ratio-crv-boundary", "ratio of actual parcel AREA vs convex hull to restrict parcels", GH_ParamAccess.item);
            // 4. rotation 
            pManager.AddAngleParameter("angle-alignment", "angle-alignment", "rotate the entire parcel generation", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("input sites", "site", "street grids on site", GH_ParamAccess.list);
            pManager.AddTextParameter("Text output debug", "debug", "test the algorithm", GH_ParamAccess.item);
            pManager.AddPointParameter("Points for debugging", "debug", "debug points for the partitions", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve SiteCrv = null;
            if(!DA.GetData(0, ref SiteCrv)) return;

            BSPAlg bspalg = new BSPAlg(SiteCrv);
            bspalg.RUN_BSP_ALG();
            List<Curve> crvs = bspalg.GetBspResults();
            DA.SetDataList(0, crvs);

            string msg = bspalg.getMSG();
            DA.SetData(1, msg);

            var T = SiteCrv.GetBoundingBox(true);
            Point3d A = T.Min;
            Point3d C = T.Max;
            Point3d a = new Point3d(A.X, A.Y, 0);
            Point3d c = new Point3d(C.X, C.Y, 50);
            Point3d b = new Point3d(c.X, a.Y, 100);
            Point3d d = new Point3d(a.X, c.Y, 150);
            Point3d[] pts = { a, b, c, d };
            DA.SetDataList(2, pts);
        }

        protected override System.Drawing.Bitmap Icon { get { return null; } }

        public override Guid ComponentGuid { get { return new Guid("3c14e4dd-7f66-4bc8-95d6-e53593d4ae10"); } }
    }
}