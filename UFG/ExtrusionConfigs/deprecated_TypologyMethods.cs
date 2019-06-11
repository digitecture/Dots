using System;
using System.Collections.Generic;

using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace DotsProj
{
    public class TypologyMethods
    {
        private Curve SiteCrv=null;
        private double Setback = 0.0;
        private double FloorDepth = 0.0;
        private double BayGap = 0.0; // courtyard, distance between two double-loaded bays
        private double FSR = 2.75;
        double FLR_HT;
        private List<double> StepbackLi;
        private List<double> StepbackHtLi;

        private List<Brep> SolidLi;
        private List<Curve> flrCrvLi;

        private string MSG = "";

        public TypologyMethods(
                Curve sitecrv, 
                double fsr, double depthflr, double gapbays,
                List<double> stepbacks, List<double> stepbackhts, double flrht
            )
        {
            SiteCrv = sitecrv;
            FSR = fsr;
            FloorDepth = depthflr;
            BayGap = gapbays;
            StepbackLi = stepbacks;
            Setback = StepbackLi[0];
            StepbackHtLi = stepbackhts;
            SolidLi = new List<Brep>();
            flrCrvLi = new List<Curve>();
            FLR_HT = flrht;
        }

        public void GenExtrBlock()
        {
            Point3d cen = Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Centroid;
            Curve[] setbackCrv = SiteCrv.Offset(
                Plane.WorldXY,
                Setback,
                Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                CurveOffsetCornerStyle.Sharp
                );
            for(int i=0; i<setbackCrv.Length; i++)
            {
                if (!setbackCrv[i].IsClosed) continue;
                double siteAr = Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Area;
                double offAr = Rhino.Geometry.AreaMassProperties.Compute(setbackCrv[i]).Area;
                double numflrs = siteAr * FSR / offAr;
                double ht = numflrs * FLR_HT;
                Brep SOLID = Extrusion.Create(setbackCrv[i], -ht, true).ToBrep();
                SolidLi.Add(SOLID);
                flrCrvLi.Add(setbackCrv[i]);
                double flrht = 0.0;
                for (int j = 0; j < numflrs; j++)
                {
                    Curve flrcrv = setbackCrv[i].DuplicateCurve();
                    Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0,0,FLR_HT*j);
                    flrcrv.Transform(xform);
                    flrCrvLi.Add(flrcrv);
                }
                MSG += "solid added";
            }
        }

        public void GenerateCourtyardBlock()
        {
            Extrusion SOLID = new Extrusion();
            Point3d cen = Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Centroid;
            var setbackCrv0 = SiteCrv.Offset(
                cen,
                Vector3d.ZAxis, Setback,
                Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                CurveOffsetCornerStyle.Sharp
            );
            Curve setbackCrv = setbackCrv0[0];
            var innerCrv0 = setbackCrv.Offset(
                cen,
                Vector3d.ZAxis,
                FloorDepth,
                Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                CurveOffsetCornerStyle.Sharp
            );
            Curve innerCrv = innerCrv0[0];
            double siteAr = AreaMassProperties.Compute(SiteCrv).Area;
            double netAr = AreaMassProperties.Compute(setbackCrv).Area - AreaMassProperties.Compute(innerCrv).Area;
            int numflrs = (int) Math.Ceiling(siteAr * FSR / netAr);
            double ht = numflrs * FLR_HT;
            for(int i=0; i<numflrs; i++)
            {
                Rhino.Geometry.Transform xform=Rhino.Geometry.Transform.Translation(0,0,i * FLR_HT);
                Curve c0 = innerCrv.DuplicateCurve();
                c0.Transform(xform);
                flrCrvLi.Add(c0);
                Curve c1 = setbackCrv.DuplicateCurve();
                c1.Transform(xform);
                flrCrvLi.Add(c1);
            }
            
            Brep innerSolid = Extrusion.Create(innerCrv, -ht, true).ToBrep();
            Brep outerSolid = Extrusion.Create(setbackCrv, -ht, true).ToBrep();
            Brep[] outer = { outerSolid };
            Brep[] inner = { innerSolid };
            Brep[] reqSolid= Brep.CreateBooleanDifference(outer, inner, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            try { for (int i = 0; i < reqSolid.Length; i++) { SolidLi.Add(reqSolid[i]); } }
            catch (Exception) { }
        }

        public List<Brep> GenStagerredBlock()
        {
            List<Brep> stagerredMassLi = new List<Brep>();
            double reqGfa = FSR*AreaMassProperties.Compute(SiteCrv).Area;
            double spineHt = 0.0;
            double arCounter = 0.0;
            Curve[] iniCrv = { SiteCrv };

            double spineht = 0.0;
            Curve c0 = SiteCrv.DuplicateCurve();
            for (int i = 0; i < StepbackLi.Count; i++)
            {
                Point3d cen = AreaMassProperties.Compute(c0).Centroid;
                double di = StepbackLi[i];
                double ht = StepbackHtLi[i];
                Curve[] c1=c0.Offset(cen, Vector3d.ZAxis, di, 0.01, CurveOffsetCornerStyle.Sharp);
                Brep brep = Rhino.Geometry.Extrusion.Create(c1[0], -ht, true).ToBrep();
                Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0, 0, spineht);
                brep.Transform(xform);
                stagerredMassLi.Add(brep);
                spineht += ht;
            }
            return stagerredMassLi;
        }

        public List<Curve> GetGeneratedCrvs() { return flrCrvLi; }

        public List<Brep> GetGeneratedSolids() { return SolidLi; }

        public string getMsg()
        {
            MSG += "msg received";
            return MSG;
        }

    }
}
