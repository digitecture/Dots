using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino;
using Rhino.DocObjects;

namespace DotsProj
{
    public class CBSP : GH_Component
    { 
        // global variables
        List<string> adjMatrixStr = new List<string>(); // adjacency matrix
        List<string> geomSpaceStr = new List<String>(); // functional space - geometric requirements
        List<string> adjObjLi = new List<string>();     // final adj obj list
        List<string> geomObjLiStr = new List<string>(); // final geom object list as string
        List<GeomObj> geomObjLi = new List<GeomObj>();

        public CBSP()
          : base("cbsp", "input-CBSP",
              "floor plan automation using binary partition",
              "DOTS", "CBSP")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0. 
            pManager.AddCurveParameter("Input Site Geometry", "input site", "Select the input site geometry", GH_ParamAccess.item);
            // 1. adjacency list path
            pManager.AddTextParameter("File Path Adjacency", "iAdjacencyPath", "adjacency matrix: csv file", GH_ParamAccess.item);
            // 2. geometric list path
            pManager.AddTextParameter("File Path Geometry", "iGeomPath", "geometric requirements: csv file", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0 output adjacenecy list as string
            pManager.AddTextParameter("Output Adjacency", "oAdj", "output of reading adjacency requirements", GH_ParamAccess.list);
            // 1 output function list as string
            pManager.AddTextParameter("Output Functions", "oGeom", "output of reading spatial (geometric) requirements", GH_ParamAccess.list);
            // 2 output geometry objects as string list
            pManager.AddTextParameter("Temp Geom Obj List", "oBSP", "Output list of Geom objs", GH_ParamAccess.list);
            // 3. ouput polylines
            pManager.AddCurveParameter("Display Polyline", "oPoly", "Output list of Polyline", GH_ParamAccess.item);
            // 4 output random values used to generate polys
            pManager.AddTextParameter("rnd vals", "oRnd", "output random vals", GH_ParamAccess.list);
            // 5 output r polys
            pManager.AddCurveParameter("Recursive Polys", "rPolys", "output polys", GH_ParamAccess.list);
            // 6 output debug string list
            pManager.AddTextParameter("Debug string", "debug-string", "debug string", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve SITE_CRV = null;
            string adjFilePath = "null";
            string geomFilePath = "null";

            if(!DA.GetData(0, ref SITE_CRV)) return;
            if(!DA.GetData(1, ref adjFilePath)) return;
            if(!DA.GetData(2, ref geomFilePath)) return;

            double SITE_AREA = AreaMassProperties.Compute(SITE_CRV).Area;

            CsvParser csvParserAdj = new CsvParser(adjFilePath);
            adjMatrixStr = csvParserAdj.readFile(); // list of strings - not fields
            adjObjLi=csvParserAdj.GetAdjObjLi(adjMatrixStr); // list of adj objs

            CsvParser csvParserGeom = new CsvParser(geomFilePath);
            geomSpaceStr = csvParserGeom.readFile(); // list of strings - not fields
            List<string> geomObjStr=csvParserGeom.GetGeomObjLi(geomSpaceStr, SITE_AREA); // read and normalize (area) the geometry

            List<string> norGeomObjstr = csvParserGeom.norGeomObjLiStr;

            DA.SetDataList(0, adjObjLi);
            //DA.SetDataList(1, geomSpaceStr);
            DA.SetDataList(1, geomObjStr);
            DA.SetDataList(2, norGeomObjstr);

            GenCBspGeom cbspgeom = new GenCBspGeom(SITE_CRV, adjObjLi, geomObjLi); // class for geom methods

            Polyline poly = cbspgeom.GenerateInitialCurve(geomObjLi);
            DA.SetData(3, poly);


            List<string> geomstr = cbspgeom.AdjObjLi;

            DA.SetDataList(6, geomstr);
           

            // bspgeom.RunRecursions(ptLi.ToArray(), 0);
            // List<PolylineCurve> fpolys = bspgeom.GetFPolys();
            // List<string> rvals = bspgeom.GetRVals();
            // DA.SetDataList(4, rvals);
            // DA.SetDataList(5, fpolys);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.genCrvs; } }

        public override Guid ComponentGuid { get { return new Guid("73dd4e25-553b-4853-a8e4-5d17d96afa84"); } }
    }
}

