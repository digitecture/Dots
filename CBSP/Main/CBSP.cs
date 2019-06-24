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
            // 3. rotation
            pManager.AddNumberParameter("Rotation-in-degrees", "Rot-degrees", "Rotation (in degrees) alignment of overall geometric forms", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0 output adjacenecy list as string
            pManager.AddTextParameter("Output Adjacency", "oAdj", "output of reading adjacency requirements", GH_ParamAccess.list);
            // 1 output function list as string
            pManager.AddTextParameter("Output Functions", "oGeom", "output of reading spatial (geometric) requirements", GH_ParamAccess.list);
            // 2 output geometry objects as string list
            pManager.AddTextParameter("Temp Geom Obj List", "oBSP", "Output list of Geom objs", GH_ParamAccess.list);
            // 3. ouput result polylines
            pManager.AddCurveParameter("Display result BBX-Poly", "result-bbx-Poly", "Output of resultant BBX-Poly", GH_ParamAccess.item);
            // 4. ouput pendant polylines
            pManager.AddCurveParameter("Display Extracted Spaces (Curves)", "extracted-spaces(crvs)", "Output list of extracted spaces as curves", GH_ParamAccess.list);
            // 5. ouput pendant polylines
            pManager.AddCurveParameter("Display Bounding Box Polyline", "bbx-Poly", "Output list of bounding-box Polylines", GH_ParamAccess.list);
            // 6. output debug string list
            pManager.AddTextParameter("Debug string", "debug-string", "debug string", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve SITE_CRV = null;
            string adjFilePath = "null";
            string geomFilePath = "null";
            double rotation = double.NaN;

            if (!DA.GetData(0, ref SITE_CRV)) return;
            if (!DA.GetData(1, ref adjFilePath)) return;
            if (!DA.GetData(2, ref geomFilePath)) return;
            if (!DA.GetData(3, ref rotation)) return;

            double SITE_AREA = AreaMassProperties.Compute(SITE_CRV).Area;

            CsvParser csvParserAdj = new CsvParser(adjFilePath);
            adjMatrixStr = csvParserAdj.readFile(); // list of strings - not fields
            adjObjLi = csvParserAdj.GetAdjObjLi(adjMatrixStr); // list of adj objs

            CsvParser csvParserGeom = new CsvParser(geomFilePath);
            geomSpaceStr = csvParserGeom.readFile(); // list of strings - not fields
            List<string> geomObjStr = csvParserGeom.GetGeomObjLi(geomSpaceStr, SITE_AREA); // read and normalize (area) the geometry

            List<string> norGeomObjstr = csvParserGeom.norGeomObjLiStr;
            List<GeomObj> norGeomObjLi = csvParserGeom.norGeomObjLi;

            DA.SetDataList(0, adjObjLi);
            DA.SetDataList(1, geomObjStr);
            DA.SetDataList(2, norGeomObjstr);

            GenCBspGeom cbspgeom = new GenCBspGeom(SITE_CRV, adjObjLi, norGeomObjLi, rotation); // class for geom methods
            cbspgeom.GenerateInitialCurve(); // run the recursions and generate the rotated bbx, reverse rotate bbx curves
            List<Curve> ResultPolys = cbspgeom.ResultBBxPolys;
            cbspgeom.ExtractPolyFromSite(); // generate the extracted curves from the site
            List<Curve> ExtractedCrvs = cbspgeom.ExtractedCrvs;
            // List<Curve> FPolys = cbspgeom.BSPCrvs;
            // List<Curve> BBxPolys = cbspgeom.BBxCrvs;
            DA.SetDataList(3, ResultPolys);
            DA.SetDataList(4, ExtractedCrvs);
            // DA.SetDataList(5, BBxPolys);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.genCrvs; } }

        public override Guid ComponentGuid { get { return new Guid("73dd4e25-553b-4853-a8e4-5d17d96afa84"); } }
    }


    public struct nsSeg
    {
        public nsPt p { get; set; }
        public nsPt q { get; set; }
        public nsSeg(nsPt p_, nsPt q_)
        {
            this.p = p_;
            this.q = q_;
        }
        public nsSeg(double x0_, double y0_, double z0_, double x1_, double y1_, double z1_)
        {
            this.p = new nsPt(x0_, y0_, z0_);
            this.q = new nsPt(x1_, y1_, z1_);
        }
        public override string ToString()
        {
            return string.Format("nsSeg : P={0},{1},{2} To Q={0},{1},{2}", p.x, p.y, p.z, q.x, q.y, q.z);
        }
    }

    public struct nsPt
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public nsPt(double x_, double y_, double z_)
        {
            this.x = x_;
            this.y = y_;
            this.z = z_;
        }
        public double dist(nsPt q)
        {
            return Math.Sqrt((x - q.x) * (x - q.x) + (y - q.y) * (y - q.y) + (z - q.z) * (z - q.z));
        }
        public override string ToString()
        {
            return string.Format("nsPt: {0},{1},{2} ", x, y, z);
        }
    }


}

