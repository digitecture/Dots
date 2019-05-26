using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace UFG

{
    public class BSP : GH_Component
    {
        List<double> scoreLi = new List<double>();
        List<BspObj> bspObjLi = new List<BspObj>();
        List<Curve> thisFCRVS = new List<Curve>();

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
            // 1. Number of Parcels 
            pManager.AddIntegerParameter("number-parcels", "num-of-parcels ", "The number of parcels required", GH_ParamAccess.item);
            // 2. standard deviation to restrict area of individual partition & boundary
            pManager.AddNumberParameter("dev-mean_area (0,1)", "std-dev Area", "standard deviation in AREA to restict parcels", GH_ParamAccess.item);
            // 3. standard deviation to restrict dimension of sides 
            pManager.AddNumberParameter("dev-dim (0,1)", "std-dev Dimension", "standard deviation in DIMENSION to restrict parcels", GH_ParamAccess.item);
            // 4. min ratio of actual curve and boundary
            pManager.AddNumberParameter("ratio-ar-boundary (0,1)", "ratio-crv-boundary", "ratio of actual parcel AREA vs convex hull to restrict parcels", GH_ParamAccess.item);
            // 5. rotation 
            pManager.AddAngleParameter("angle-alignment (degrees 0, 360)", "angle-alignment", "rotate the entire parcel generation", GH_ParamAccess.item);
            // 6. number of iterations
            pManager.AddIntegerParameter("show-this-iterations", "this-itr", "showing the iteration to show - optimization", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("lowest deviation solution", "output", "output-street grids on site", GH_ParamAccess.list);
            pManager.AddCurveParameter("Text output debug", "debug", "test the algorithm", GH_ParamAccess.list);
            pManager.AddNumberParameter("Points for debugging", "debug", "debug points for the partitions", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve SiteCrv = null;
            int numParcels = 4;
            double stdDevMeanAr = 0.25;
            double stdDevDim = 0.5;
            double ratioAr = 0.75;
            double rot = 0.0;
            int  showItr = 0;
            if (!DA.GetData(0, ref SiteCrv)) return;
            if (!DA.GetData(1, ref numParcels)) return;
            if (!DA.GetData(2, ref stdDevMeanAr)) return;
            if (!DA.GetData(3, ref stdDevDim)) return;
            if (!DA.GetData(4, ref ratioAr)) return;
            if (!DA.GetData(5, ref rot)) return;
            if (!DA.GetData(6, ref showItr)) return;

            int NumIters = scoreLi.Count;//(int)numItrs;
            double Rotation = Rhino.RhinoMath.ToRadians(rot);

            BSPAlg bspalg = new BSPAlg(SiteCrv, numParcels, stdDevMeanAr, stdDevDim, ratioAr, Rotation, NumIters);
            bspalg.RUN_BSP_ALG();
            BspObj obj = bspalg.GetBspObj();
            bspObjLi.Add(obj);
            scoreLi.Add(obj.GetScore());


            List<Curve> lowestDevCrv = new List<Curve>();
            double minScore = 100000.00;
            for(int i=0; i<bspObjLi.Count; i++)
            {
                double score = bspObjLi[i].GetScore();
                if (score < minScore)
                {
                    minScore = score;
                    lowestDevCrv = bspObjLi[i].GetCrv(); ;
                }
            }
            bspalg.GetBspResults();

            thisFCRVS = bspObjLi[showItr].GetCrv();
            
            DA.SetDataList(0, lowestDevCrv);
            DA.SetDataList(1, thisFCRVS);
            DA.SetDataList(2, scoreLi);
        }

        protected override System.Drawing.Bitmap Icon { get { return null; } }

        public override Guid ComponentGuid { get { return new Guid("3c14e4dd-7f66-4bc8-95d6-e53593d4ae10"); } }
    }
}