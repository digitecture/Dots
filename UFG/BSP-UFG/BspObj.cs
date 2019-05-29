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
        
    }
}
