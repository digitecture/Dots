using System;
using System.Collections.Generic;

using Rhino.Geometry;

namespace UFG
{
    public class BspObj
    {
        List<Curve> FCURVE;

        double MAX_DEV_MEAN_AREA = 0.5; // {0,1}
        double MAX_DEV_RATIO_AREA = 0.5; // {0,1}
        double MAX_DEV_DIM = 0.5; // {0,1}

        string MSG = "";
        int redoCounter = 0;
        double SCORE = 0.0;

        double DEV_MEAN_AR;
        double DEV_DIM;
        double DEV_AR_RATIO;

        public BspObj() { }

        public BspObj(List<Curve> crvs, double devMeanAr, double devDim, double devRatioAr, int redo)
        {
            FCURVE = crvs;
            MAX_DEV_MEAN_AREA = devMeanAr;
            MAX_DEV_RATIO_AREA = devRatioAr;
            MAX_DEV_DIM = devDim;
            redoCounter = redo;
        }

        public List<Curve> GetCrvs() { return FCURVE; }

        public double GetDevMeanAr() { return DEV_MEAN_AR; }

        public double GetDevDim() { return DEV_DIM; }

        public double GetDevArRatio() { return DEV_AR_RATIO; }

        public void PostProcess()
        {
            // 3 constraints from GUI, file : BSP file
            // MAX_DEV_MEAN_AREA : mean of all parcels / ar of each parcel 
            // MAX_DEV_DIM : hor dim / ver dim || vice-versa
            // MAX_DEV_RATIO_AREA : bounding box area to curve area

            double ar = 0.0;
            for (int i = 0; i < FCURVE.Count; i++)
            {
                try
                {
                    ar += Rhino.Geometry.AreaMassProperties.Compute(FCURVE[i]).Area;
                }
                catch (Exception) { }
            }
            double meanAr = ar / FCURVE.Count;
            double minArPer = (1 - MAX_DEV_MEAN_AREA) * meanAr;
            double maxArPer = (1 + MAX_DEV_MEAN_AREA) * meanAr;
            // condition 1 
            for (int i = 0; i < FCURVE.Count; i++)
            {
                double Ar = Rhino.Geometry.AreaMassProperties.Compute(FCURVE[i]).Area;
                if (Ar < minArPer || Ar > maxArPer) // con 1: from BSP file 
                {
                    MSG += "\ncondition. 1:" + redoCounter.ToString();
                    break;
                }
            }
            double ar_ratio = 0.0;
            double dim_ratio = 0.0;
            for (int i = 0; i < FCURVE.Count; i++)
            {
                var T = FCURVE[i].GetBoundingBox(true);
                var a = T.Min;
                var c = T.Max;
                var b = new Point3d(c.X, a.Y, 0);
                var d = new Point3d(a.X, c.Y, 0);
                Point3d[] pts = { a, b, c, d, a };
                PolylineCurve crv = new PolylineCurve(pts);
                double verDi = a.DistanceTo(d);
                double horDi = a.DistanceTo(b);
                // condition 2 : from GUI, file : BSP file
                double dim_ratio1 = horDi / verDi;
                double dim_ratio2 = verDi / horDi;
                if (dim_ratio1 < MAX_DEV_DIM || dim_ratio2 < MAX_DEV_DIM) // con 2: from BSP file 
                {
                    if (dim_ratio1 < dim_ratio2) { dim_ratio = dim_ratio1; }
                    else dim_ratio = dim_ratio2;
                    MSG += "\ncondition. 2:" + redoCounter.ToString();
                    break;
                }
                double ArBB = Rhino.Geometry.AreaMassProperties.Compute(crv).Area; // area of bounding box
                double ArCrv = Rhino.Geometry.AreaMassProperties.Compute(FCURVE[i]).Area; // actual area of the curve
                // condition 3 : from GUI, file : BSP file
                ar_ratio = ArCrv / ArBB;
                if (ar_ratio < MAX_DEV_RATIO_AREA) // con 3: from BSP file 
                {
                    MSG += "\ncondition. 3:" + redoCounter.ToString();
                    break;
                }
            }
        }
    }
}
