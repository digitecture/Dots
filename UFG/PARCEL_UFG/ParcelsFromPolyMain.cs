﻿using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace UFG
{
    public class FormConfig : GH_Component
    {
        public FormConfig()
          : base("parcels-from-internal-poly", "parcel-gen-2",
              "Parcels generated by extending an internal polyline",
              "DOTS", "UFG")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Input site curve", "site", "curve-boundaries to be occupied by required typology", GH_ParamAccess.item);
            pManager.AddCurveParameter("internal polyine", "int-poly", "internal polyine to guide parcel generation", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) 
        {
            pManager.AddCurveParameter("output parcels", "out-polys", "output parcels generated", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA) 
        {
            Curve site = null;
            Curve poly = null;
            if (!DA.GetData(0, ref site)) return;
            if (!DA.GetData(1, ref poly)) return;

            ParcelsFromPolyUtil util = new ParcelsFromPolyUtil();
            List<Curve> iniBspTree = util.GenHalfPlanes(site, poly);
            //List<Curve> fBspTree = util.RemovePoly(iniBspTree, poly);

            DA.SetDataList(0, iniBspTree);
        }

        protected override System.Drawing.Bitmap Icon { get { return null; } } 

        public override Guid ComponentGuid { get { return new Guid("db513281-f7b3-4e79-88e6-ae9b2eafb9d5"); } }

    }
}
