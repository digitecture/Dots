using System;
using System.Collections.Generic;

using Rhino.Geometry;

namespace UFG
{
    public class BspObj
    {
        List<Curve> FCURVE;
        string MSG = "";
        int NUM_PARCELS_REQ;

        public BspObj() { }

        public BspObj(List<Curve> crvs, int num_parcels_req) 
        { 
            FCURVE = crvs;
            NUM_PARCELS_REQ = num_parcels_req;
        }

        public List<Curve> GetCrvs() { return FCURVE; }

        public double GetDevMeanAr() 
        {
            double sum = 0.0;
            for(int i=0; i<FCURVE.Count; i++)
            {
                sum+=Rhino.Geometry.AreaMassProperties.Compute(FCURVE[i]).Area;
            }
            double mean_ar = sum / FCURVE.Count;
            double max_dev = -1.0;
            for (int i = 0; i < FCURVE.Count; i++)
            {
                double ar= Rhino.Geometry.AreaMassProperties.Compute(FCURVE[i]).Area;
                double dev = Math.Sqrt(Math.Pow(mean_ar - ar, 2));
                if (dev > max_dev)
                {
                    max_dev = dev;
                }
            }
            return max_dev; 
        }

        public double GetDevArRatio() 
        {
            double min_ratio = 1.00;
            for (int i = 0; i < FCURVE.Count; i++)
            {
                double ar_crv = Rhino.Geometry.AreaMassProperties.Compute(FCURVE[i]).Area;
                var B = FCURVE[i].GetBoundingBox(true);
                Point3d a = B.Min;
                Point3d c = B.Max;
                Point3d b = new Point3d(c.X, a.Y, 0);
                // Point3d d = new Point3d(a.X, c.Y, 0);
                double u = a.DistanceTo(b);
                double v = b.DistanceTo(c);
                double ar_B = u * v;
                double ratio = ar_crv / ar_B;
                // domain : 0 < ratio < 1
                if (ratio < min_ratio) {  min_ratio = ratio; }
            }
            return min_ratio; 
        }

        public double GetScore()
        {
            // 3 constraints from GUI, file : BSP file
            // MAX_DEV_MEAN_AREA : mean of all parcels / ar of each parcel 
            // MAX_DEV_RATIO_AREA : bounding box area to curve area
            // MAX_DEV_DIM : hor dim / ver dim || vice-versa
            double DEV_MEAN_AR = Math.Round(GetDevMeanAr(), 2);
            double DEV_AR_RATIO = Math.Round(GetDevArRatio(), 2);
            double score = (DEV_MEAN_AR + DEV_AR_RATIO) / 2;
            double SCORE = Math.Round(score, 2);
            MSG = FCURVE.Count + "/" +NUM_PARCELS_REQ+ ", dev_ar_mean: " +DEV_MEAN_AR.ToString() + "x" + DEV_AR_RATIO.ToString() + " = " + SCORE;
            return SCORE;
        }

        public string GetMsg() { return MSG; }



        //  waste //

        /*
        public void recSplit(Curve crv, int counter)
        {
            // start with curve, get bounding box
            // compare hor - ver. ratio : send to hor - ver split -> returns 2 bounding box
            // find the region within each box inside the site -> result
            var T = crv.GetBoundingBox(true);
            Point3d a = T.Min;
            Point3d c = T.Max;
            Point3d b = new Point3d(c.X, a.Y, 0);
            Point3d d = new Point3d(a.X, c.Y, 0);
            double horDi = a.DistanceTo(b);
            double verDi = a.DistanceTo(d);

            List<Point3d[]> polyPts = new List<Point3d[]>(); //persistent data

            Point3d[] iniPts = { a, b, c, d, a };

            if (horDi > verDi) { polyPts = verSplit(iniPts); }
            else { polyPts = horSplit(iniPts); }

            // 2 bounding box of the input curve from recursive split function
            PolylineCurve crv1 = new PolylineCurve(polyPts[0]);
            PolylineCurve crv2 = new PolylineCurve(polyPts[1]);

            // get intersection with main site crv
            Curve[] crvs1 = Curve.CreateBooleanIntersection(SiteCrv, crv1);
            Curve[] crvs2 = Curve.CreateBooleanIntersection(SiteCrv, crv2);

            int num_crvs_got = FCURVE.Count + crvs1.Length + crvs2.Length;
            MSG += counter.ToString() + ">" + NUM_PARCELS_REQ.ToString() + "," + NUM_PARCELS.ToString() + "\n";

            if (counter < NUM_PARCELS_REQ) // from GUI ; file: BSP
            {
                try
                {
                    if (crvs1.Length > 0) { for (int i = 0; i < crvs1.Length; i++) { counter++; recSplit(crvs1[i], counter); } }
                }
                catch (Exception) { }

                try
                {
                    if (crvs2.Length > 0) { for (int i = 0; i < crvs2.Length; i++) { counter++; recSplit(crvs2[i], counter); } }
                }
                catch (Exception) { }
            }

            else
            {
                for (int i = 0; i < crvs1.Length; i++) { FCURVE.Add(crvs1[i]); }
                for (int i = 0; i < crvs2.Length; i++) { FCURVE.Add(crvs2[i]); }
            }
        }
        */
    }
}
