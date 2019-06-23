using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj
{
    public class GenMassFromCrvs : GH_Component
    {

        public GenMassFromCrvs()
          : base("Massing", "mass",
              "Generate Building Masses From Curves",
              "DOTS", "Massing")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Overall site boundary", "site boundary", "input the site boundary", GH_ParamAccess.item);
            pManager.AddCurveParameter("Linked Set of RECTANGLES", "linked base (RECTANGLES)", "input the linked set of RECTANGLES ( : collective base curves to derive mid-crvs, bridges, towers", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number of base floors", "num-base-flrs", "number of floors for the base level", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of mid-level floors", "num-mid-flrs", "number of floors for the mid- levels - stagerred", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of bridge-floors", "num-bridge-flrs", "number of floors for the bridge -floors ", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of tower-floors", "num-tower-flrs", "number of floors for the tower", GH_ParamAccess.item);
            pManager.AddNumberParameter("Minimum Fsr", "min-fsr/far", "enter the minimum FSR required", GH_ParamAccess.item);
            pManager.AddNumberParameter("Maximum Fsr", "max-fsr/far", "enter the maximum FSR required", GH_ParamAccess.item);
            pManager.AddNumberParameter("Flr-Flr Ht", "flr-ht", "enter the floor to floor height", GH_ParamAccess.item);
            pManager.AddNumberParameter("bridge-depth", "bridge-depth", "enter the bridge depth", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("debug", "debug", "debug", GH_ParamAccess.item);
            pManager.AddTextParameter("debug", "debug", "debug", GH_ParamAccess.list);
            pManager.AddCurveParameter("Base flr curves", "base-flr-crvs", "base floor curves", GH_ParamAccess.list);
            pManager.AddPointParameter("Input curve points", "inp-pts", "Points of the input curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("Bridges between crvs", "lines", "Two bridges of the input curves", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve SITE = null;
            List<Curve> BaseCrvsInp= new List<Curve>();
            int NumBaseFlrsInp = 1;
            int NumMidFlrsInp = 1;
            int NumBridgeFlrsInp = 1;
            int NumTowerFlrsInp = 1;
            double MinFsrInp = double.NaN;
            double MaxFsrInp = double.NaN;
            double FlrHt = double.NaN;
            double BridgeDepth = double.NaN;

            if (!DA.GetDataList(1, BaseCrvsInp)) return;
            if (!DA.GetData(0, ref SITE)) return;
            if (!DA.GetData(2, ref NumBaseFlrsInp)) return;
            if (!DA.GetData(3, ref NumMidFlrsInp)) return;
            if (!DA.GetData(4, ref NumBridgeFlrsInp)) return;
            if (!DA.GetData(5, ref NumTowerFlrsInp)) return;
            if (!DA.GetData(6, ref MinFsrInp)) return;
            if (!DA.GetData(7, ref MaxFsrInp)) return;
            if (!DA.GetData(8, ref FlrHt)) return;
            if (!DA.GetData(9, ref BridgeDepth)) return;

            double SiteAr = AreaMassProperties.Compute(SITE).Area;
            double minFsr = Math.Round((SiteAr * MinFsrInp), 2);
            double maxFsr = Math.Round((SiteAr * MaxFsrInp), 2);
            GenerateMass genMass = new GenerateMass(SITE, SiteAr, minFsr, maxFsr,
            NumBaseFlrsInp, NumMidFlrsInp, NumBridgeFlrsInp, NumTowerFlrsInp, 
            BaseCrvsInp, FlrHt, BridgeDepth);

            List<Curve> baseFlrCrvs= new List<Curve>();
            List<Point3d> ptList = new List<Point3d>();
            List<List<Point3d>> ListPointList = new List<List<Point3d>>();
            List<Curve> bridges;
            string debugMsg = "";
            try
            {
                genMass.GenBaseCrvFloors();
                baseFlrCrvs= genMass.BaseFlrCrvs;
                ListPointList = genMass.BaseCrvPts;

                debugMsg = genMass.ToString();
                bridges = genMass.GenBridge();
                ptList = genMass.BridgeCrvPts;

                DA.SetDataList(2, baseFlrCrvs);
                DA.SetDataList(3, ptList);
                DA.SetDataList(4, bridges);

                DA.SetData(0, debugMsg);
            }
            catch (Exception) { }
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.genCrvs; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("ce1daa3c-24ba-427b-8053-1a6bf98a1f1a"); }
        }
    }
}
