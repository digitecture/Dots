using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj
{
    public class BlockConfig : GH_Component
    {
        public BlockConfig()
          : base("extrude-block", "block",
              "Extrude block from site, given setback",
              "DOTS", "Massing")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        { 
            // 0. Site curve
            pManager.AddCurveParameter("Site Curve", "crv", "sites for placing the blocks", GH_ParamAccess.list);
            // 1. Setback 
            pManager.AddNumberParameter("Setback", "setback", "setback for the sites", GH_ParamAccess.item);
            // 2. FSR
            pManager.AddNumberParameter("FSR", "fsr/far", "FSR: floor-space ratio or FAR: floor-area-ratio", GH_ParamAccess.item);
            // 3. Floor height
            pManager.AddNumberParameter("FLR-HT", "flr-ht", "Floor to floor height (storey)", GH_ParamAccess.item);
            // 4. Restriction : min area of the floor curve
            pManager.AddNumberParameter("Min-Crv-Area", "min-crv-ar", "Restriction: Minimum area of the curve for Massing", GH_ParamAccess.item);
            // 5. Restriction : slenderness Ratio
            pManager.AddNumberParameter("ht/area-1-flr", "ht/area", "Restriction: Slenderness ratio : ht / floor area 1 storey", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0. Extruded Mass
            pManager.AddBrepParameter("output Extruded Mass","Block","extruded mass / blocks",GH_ParamAccess.list);
            // 1. Floor Curves
            pManager.AddCurveParameter("Floor Curves", "flrs", "Floor curves", GH_ParamAccess.list);
            // 2. Debug
            // pManager.AddTextParameter("debug","debug","debug numbers",GH_ParamAccess.item);
            // pManager.AddTextParameter("debug2","debug2","debug HT Z",GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> sites = new List<Curve>();
            double fsr = double.NaN;
            double setback = double.NaN;
            double flrHt = double.NaN;
            double minAr=double.NaN;
            double slendernessRatio = double.NaN;

            if (!DA.GetDataList(0, sites)) return;
            if (!DA.GetData(1, ref setback)) return;
            if (!DA.GetData(2, ref fsr)) return;
            if (!DA.GetData(3, ref flrHt)) return;
            if (!DA.GetData(4, ref minAr)) return;
            if (!DA.GetData(5, ref slendernessRatio)) return;

            List<Extrusion> massLi = new List<Extrusion>();
            List<List<Curve>> listFlrCrvLi = new List<List<Curve>>();
            List<Curve> flrCrvLi = new List<Curve>();
            string msg = "";
            msg += "\nfsr: " + fsr.ToString();
            msg += "\nsetback: " + setback.ToString();
            string MSG = "Debug BLOCK:\n";
            for (int i = 0; i < sites.Count; i++)
            {               
                //OFFSET FROM THE SITE BOUNDARY
                Curve c1 = sites[i].DuplicateCurve();
                Curve c2 = Rhino.Geometry.Curve.ProjectToPlane(c1, Plane.WorldXY);
                Curve[] c2Offs;
                Point3d cen = AreaMassProperties.Compute(c2).Centroid;
                Rhino.Geometry.PointContainment cont = sites[i].Contains(cen);
                if (cont.ToString() == "Inside")
                {
                    c2Offs = c2.Offset(cen, Vector3d.ZAxis, setback, 0.01, CurveOffsetCornerStyle.Sharp);
                }
                else
                {
                    c2Offs = c2.Offset(cen, Vector3d.ZAxis, -setback, 0.01, CurveOffsetCornerStyle.Sharp);
                }
                try
                {
                    if (c2Offs.Length == 1)
                    {
                        Curve OFFSET_CRV_ = c2Offs[0];
                        Curve OFFSET_CRV = Curve.ProjectToPlane(OFFSET_CRV_,Plane.WorldXY);
                        double arSite = Rhino.Geometry.AreaMassProperties.Compute(sites[i]).Area;
                        double arOffset = AreaMassProperties.Compute(OFFSET_CRV).Area; // 1 floor
                        double num_flrs = fsr * arSite / arOffset;
                        double ht = num_flrs*flrHt;
                        double gotSlendernessRatio = ht / arOffset;
                        if (gotSlendernessRatio<slendernessRatio)
                        {
                            msg += "\nExceeded slenderness ratio";
                        }
                        else if(arOffset<=minAr) {
                            msg += "\nar site: " + arSite.ToString() + "\nar offset: " + arOffset.ToString() + "\nht: " + ht.ToString();
                        }
                        else
                        {
                            Vector3d vec = new Vector3d(0, 0, ht);
                            Curve c3 = Rhino.Geometry.Curve.ProjectToPlane(OFFSET_CRV, Plane.WorldXY);
                            Extrusion mass = Rhino.Geometry.Extrusion.Create(c3, ht, true);
                            var B = mass.GetBoundingBox(true);
                            MSG += "Z = " + B.Max.Z.ToString() + ", " + B.Min.Z.ToString();
                            if(B.Max.Z <= 0.01)
                            {
                                mass = Extrusion.Create(c3, -ht, true);
                            }
                            massLi.Add(mass);

                            for(int j=0; j<num_flrs; j++)
                            {
                                Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0, 0, j * flrHt);
                                Curve c4 = c3.DuplicateCurve();
                                c4.Transform(xform);
                                flrCrvLi.Add(c4);
                            }
                        }
                    }
                }
                catch (Exception) { }
                //listFlrCrvLi.Add(flrCrvLi);
            }

            DA.SetDataList(0, massLi);
            DA.SetDataList(1, flrCrvLi);
            // DA.SetData(2, msg);
            // DA.SetData(3, MSG);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.ufgextrbasic; } }

        public override Guid ComponentGuid { get { return new Guid("c81f5263-86b7-48a3-b3d7-6b462f5658f9"); } }

    }
}

