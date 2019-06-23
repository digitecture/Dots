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
        List<GeomEntry> geomObjLi = new List<GeomEntry>();

        public CBSP()
          : base("cbsp", "input-CBSP",
              "floor plan automation using binary partition",
              "DOTS", "CBSP")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File Path Adjacency", "iAdjacencyPath", "adjacency matrix: csv file", GH_ParamAccess.item);
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
            string adjFilePath = "null";
            string geomFilePath = "null";

            DA.GetData(0, ref adjFilePath);
            DA.GetData(1, ref geomFilePath);

            CsvParser csvParserAdj = new CsvParser(adjFilePath);
            adjMatrixStr = csvParserAdj.readFile(); // list of strings - not fields
            adjObjLi=csvParserAdj.GetAdjObjLi(adjMatrixStr); // list of adj objs

            CsvParser csvParserGeom = new CsvParser(geomFilePath);
            geomSpaceStr = csvParserGeom.readFile(); // list of strings - not fields
            geomObjLiStr = csvParserGeom.GetGeomObjLi(geomSpaceStr); // overloaded: list of geom objs
            List<GeomEntry> geomObjLi = csvParserGeom.GetGeomObjLi();

            List<string> fstr = new List<string>();
            for(int i=0; i<geomObjLi.Count; i++) 
            { fstr.Add(geomObjLi[i].displayString()); }

            DA.SetDataList(0, adjObjLi);
            DA.SetDataList(1, geomObjLiStr);
            DA.SetDataList(2, fstr);

            GenCBspGeom cbspgeom = new GenCBspGeom(adjObjLi, geomObjLi); // class for geom methods
            Polyline poly = cbspgeom.GenerateInitialCurve(geomObjLi); DA.SetData(3, poly);
            List<Point3d> ptLi = new List<Point3d>();
            IEnumerator<Point3d> pts = poly.GetEnumerator();
            while (pts.MoveNext())
            {
                ptLi.Add(pts.Current);
            }
           

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
