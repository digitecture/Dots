using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace UFG
{
    public class UFGInputs : GH_Component
    {
        public UFGInputs()
          : base("UFG", "iUfg",
            "Ensure Layer names are CAPITALIZED & match with A.0.0\nGenerates a set of solid geometry based on setbacks from street-types, fsr calculations and range of min-max heights",
            "DOTS", "UFG")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0.
            pManager.AddTextParameter("Input-Streets", "street-layer-names", "Input for streets: Layer names (CAPITALIZED) separated by comma \nPreferred Input Type: write text in PANEL", GH_ParamAccess.item);
            // 1.
            pManager.AddTextParameter("Input-Setbacks", "corresponding-street-setbacks", "Corresponding Input for setbacks: Enter numbers separated by comms\nPreferred Input Type: write text in PANEL", GH_ParamAccess.item);
            // 2.
            pManager.AddCurveParameter("Input-Site-Curves", "inp-site-crvs", "Select all site boundaries: closed planar curves (polylines, nurbs, etc)\nPreferred Input Type: CRV & select multiple curves", GH_ParamAccess.list);
            // 3. 
            pManager.AddNumberParameter("Input-FSR", "input-fsr", "Default Value=2.5\nEnter a number for FSR calculations\nPreferred Input Type: NUMERIC SLIDER", GH_ParamAccess.item);
            }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0.
            pManager.AddGeometryParameter("Output-Solid-Geometry", "out-solid-Geom", "outputs solid geometry -breps, surfaces, mesh convertible", GH_ParamAccess.list);
            // 1.
            pManager.AddTextParameter("Output-SITE-Numbers", "debug-numbers", "valid site numbers", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string streetLayerNames = "";
            string setbacks = "";

            DA.GetData(0, ref streetLayerNames); // GET THE STRING : LAYER NAMES
            DA.GetData(1, ref setbacks); // GET THE STRING : SETBACK DISTANCES

            InputProc inputproc = new InputProc(streetLayerNames, setbacks);

            inputproc.GetLayerGeom();
            List<ProcObj> PROCOBJLI = inputproc.GetProcObjs();
            List<Curve> tempSITES = new List<Curve>();
            double FSR = 2.5;
            double MAXHT = 50.0;
            double MINHT = 0.0;
            DA.GetDataList(2, tempSITES);
            DA.GetData(3, ref FSR);
            List<Curve> SITES = new List<Curve>();

            // check and eliminate bad curves
            for (int i=0; i<tempSITES.Count; i++)
            {
                try
                {
                    Curve crv = tempSITES[i];
                    var t = crv.TryGetPolyline(out Polyline poly);
                    IEnumerator<Point3d> pts = poly.GetEnumerator();
                    List<Point3d> ptLi = new List<Point3d>();
                    while (pts.MoveNext())
                    {
                        ptLi.Add(pts.Current);
                    }
                    if (ptLi.Count > 3)
                    {
                        SITES.Add(crv);
                    }
                }
                catch (Exception) { }
            }
            DA.SetData(1, SITES.Count.ToString());

            int NUMRAYS = 4; 
            double MAGNITUDERAYS = 2.0;
            ProcessIntx processintx = new ProcessIntx(
                PROCOBJLI, 
                SITES, 
                FSR, MINHT, MAXHT,
                NUMRAYS,MAGNITUDERAYS);

            processintx.GenRays();

            List<SiteObj> SITEOBJ = processintx.GetSiteObjList();
            List<Extrusion> solids = new List<Extrusion>();
            for (int i=0; i< SITEOBJ.Count; i++)
            {
                solids.Add(SITEOBJ[i].GetOffsetExtrusion());
            }
            DA.SetDataList(0, solids);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return null; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("f00a11e0-ac24-4755-80a0-08a5cd2e5671"); } 
        }
    }
}
