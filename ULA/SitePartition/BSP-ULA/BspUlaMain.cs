using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj

{
    public class BSP_UFG : GH_Component
    {
        // List<double> scoreLi = new List<double>();
        // List<string> scoreLiMsg = new List<string>();
        List<BspUfgObj> bspObjLi = new List<BspUfgObj>();
        List<Curve> thisFCRVS = new List<Curve>();
        // List<List<Curve>> allFCRVS = new List<List<Curve>>();

        public BSP_UFG()
          : base("parcels-by-subdivision", "parcel-gen-1a",
              "Generate Parcels from site boundary",
              "DOTS", "Partition")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0. input curves
            pManager.AddCurveParameter("input-site", "site", "street grids on site", GH_ParamAccess.item);
            // 1. Number of Parcels 
            pManager.AddIntegerParameter("number-parcels", "num-of-parcels ", "The number of parcels required", GH_ParamAccess.item);
            // 2. standard deviation to restrict area of individual partition & boundary
            pManager.AddNumberParameter("dev-dim (0,1)", "dev-mean-(0,1)", "standard deviation in DIMENSION to restrict parcels", GH_ParamAccess.item);
            // 3. rotation 
            pManager.AddAngleParameter("angle-alignment (degrees 0, 360)", "angle-alignment", "rotate the entire parcel generation", GH_ParamAccess.item);
            // 4. show this iteration
            pManager.AddIntegerParameter("show-this-iterations", "this-itr", "showing the iteration to show - optimization", GH_ParamAccess.item);
            // 5. reset values
            pManager.AddBooleanParameter("reset-all-values", "reset-vals", "set everything to 0 and clear all values", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("lowest deviation solution", "min-output-geom", "output-street configuration on site with lowest score", GH_ParamAccess.list);
            pManager.AddCurveParameter("output from required iteration", "required-output-geom", "output street configurations from required iteration", GH_ParamAccess.list);
            // pManager.AddCurveParameter("debug output", "debug poly", "debug poly", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve SiteCrv = null;
            int numParcels=0;
            double devMean = double.NaN;
            double rot = double.NaN;
            int showItr = 0;
            bool reset = false;

            if (!DA.GetData(0, ref SiteCrv)) return;
            if (!DA.GetData(1, ref numParcels)) return;
            if (!DA.GetData(2, ref devMean)) return;
            if (!DA.GetData(3, ref rot)) return;
            if (!DA.GetData(4, ref showItr)) return;
            if (!DA.GetData(5, ref reset)) return;

            /// global variables to keep track of iterations
            List<Curve> lowestDevCrv = new List<Curve>();
            double minScore = 100000.00;
            int minIndex = 0;

            if (reset == true)
            {
                bspObjLi = new List<BspUfgObj>();
                lowestDevCrv = new List<Curve>();
                minIndex = 0;
                thisFCRVS = new List<Curve>();
            }

            double Rotation = Rhino.RhinoMath.ToRadians(rot);

            BspUfgAlg bspalg = new BspUfgAlg(SiteCrv, numParcels, devMean, Rotation);
            bspalg.RUN_BSP_ALG(); // RECURSIVELY PARTITION
            BspUfgObj mybspobj = bspalg.GetBspObj();

            bspObjLi.Add(mybspobj);

            for (int i = 0; i < bspObjLi.Count; i++)
            {
                double score2 = bspObjLi[i].GetScore();
                if (score2 < minScore)
                {
                    minScore = score2;
                    minIndex = i;
                }
            }

            try { thisFCRVS = bspObjLi[showItr].GetCrvs(); } catch(Exception) { }
            try { lowestDevCrv = bspObjLi[minIndex].GetCrvs(); } catch(Exception) { }
            try { DA.SetDataList(0, lowestDevCrv); } catch (Exception) { }
            try { DA.SetDataList(1, thisFCRVS); } catch (Exception) { }

            /*
            try
            {
                List<Curve> DebugBBX = bspalg.DebugBBX;
                DA.SetDataList(2, DebugBBX);
            }
            catch (Exception) { }
            */
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.revbspSimple; } }

        public override Guid ComponentGuid { get { return new Guid("3c14e4dd-7f66-4bc8-95d6-e53593d4ae10"); } }
    }
}







