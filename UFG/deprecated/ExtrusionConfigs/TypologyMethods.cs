﻿using System;
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
        private List<double> StepbackArr;
        private List<double> StepbackHtArr;

        private List<Brep> SolidLi;
        private List<Curve> CurveLi;

        private string MSG = "";

        public TypologyMethods(
                Curve sitecrv, 
                double fsr, double setback, double depthflr, double gapbays,
                List<double> stepbacks, List<double> stepbackhts
            )
        {
            SiteCrv = sitecrv;
            FSR = fsr;
            Setback = setback;
            StepbackArr = stepbacks;
            FloorDepth = depthflr;
            BayGap = gapbays;
            StepbackArr = stepbacks;
            StepbackHtArr = stepbackhts;
            SolidLi = new List<Brep>();
        }

        public void GenExtrBlock()
        {
            Point3d cen = Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Centroid;
            var setbackCrv = SiteCrv.Offset(
                cen,
                Vector3d.ZAxis,Setback,
                Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,
                CurveOffsetCornerStyle.Sharp
                );
            try
            {
                double siteAr = Rhino.Geometry.AreaMassProperties.Compute(setbackCrv).Area;
                double offAr = Rhino.Geometry.AreaMassProperties.Compute(setbackCrv).Area;
                double ht = siteAr * FSR / offAr;
                Brep SOLID = Extrusion.Create(setbackCrv[0], - ht, true).ToBrep();
                SolidLi.Add(SOLID);
                CurveLi.Add(setbackCrv[0]);
                MSG += "solid added";
            }
            catch (Exception) { MSG += "solid NOT added"; }
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
            double ht = siteAr * FSR / netAr;
            Brep innerSolid = Extrusion.Create(innerCrv, -ht, true).ToBrep();
            Brep outerSolid = Extrusion.Create(setbackCrv, -ht, true).ToBrep();
            Brep[] outer = { outerSolid };
            Brep[] inner = { innerSolid };
            Brep[] reqSolid= Brep.CreateBooleanDifference(outer, inner, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            try { for (int i = 0; i < reqSolid.Length; i++) { SolidLi.Add(reqSolid[i]); } }
            catch (Exception) { }
        }

        public List<Curve> GetGeneratedCrvs() { return CurveLi; }

        public List<Brep> GetGeneratedSolids() { return SolidLi; }

        public string getMsg()
        {
            MSG += "msg received";
            return MSG;
        }

    }
}