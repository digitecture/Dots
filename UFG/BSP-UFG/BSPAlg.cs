using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Rhino.Geometry;

namespace UFG

{
    class BSPAlg
    {
        string MSG = "";
        List<Curve> FCURVE = new List<Curve>();
        List<Line> partitionLines = new List<Line>();
        Curve SiteCrv;
        int redoCounter = 0;

        Random rnd = new Random();

        public BSPAlg(Curve crv)
        {
            SiteCrv = crv;
        }

        public Point3d getCentroid()
        {
            Point3d pt = Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Centroid;
            return pt;
        }

        public string getMSG()
        {
            return MSG;
        }

        public List<Line> getPartitionLines()
        {
            return partitionLines;
        }

        public void RUN_BSP_ALG()
        {
            Curve[] arr = FCURVE.ToArray();
            FCURVE = new List<Curve>();

            //run the bsp algorithm
            Curve crv = SiteCrv.DuplicateCurve();
            recSplit(crv, 0);

            // RECURSIVELY optimize the parcel generation strategy
            redoCounter++;
            
            bool t = PostProcess(); 
            if (t == true && redoCounter < 100) { RUN_BSP_ALG(); }
        }

        public List<Curve> GetBspResults() { return FCURVE; }

        public List<Curve> getIntxCrv(Curve crv)
        {
            //only take the part of a curve inside the site region
            List<Curve> retCrv=new List<Curve>();
            Curve[] intxCrv = Curve.CreateBooleanIntersection(SiteCrv, crv);
            if (intxCrv.Length > 0)
            {
                Curve maxCrv = intxCrv[0];
                double maxAr = 0.0;
                for (int i = 0; i < intxCrv.Length; i++)
                {
                    double ar = Rhino.Geometry.AreaMassProperties.Compute(intxCrv[i]).Area;
                    if (ar > maxAr)
                    {
                        maxAr = ar;
                        maxCrv = intxCrv[i];
                    }
                }
                for(int i=0; i<intxCrv.Length; i++)
                {
                    if(intxCrv[i]!= null)
                    {
                        retCrv.Add(intxCrv[i]);
                    }                    
                }                
               // return maxCrv;
            }
            return retCrv;
        }

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
            
            // PolylineCurve poly0 = new PolylineCurve(iniPts); 
            // initial bounding box
            // FCURVE.Add(poly0);

            if (horDi > verDi) { MSG += ".H"; polyPts = verSplit(iniPts); }
            else { MSG += ".V"; polyPts = horSplit(iniPts); }

            // returned = 2 bounding box: find the portion of curve it intersects with
            PolylineCurve crv1 = new PolylineCurve(polyPts[0]);
            PolylineCurve crv2 = new PolylineCurve(polyPts[1]);
            //Curve fcrv1 = getIntxCrv(crv1); // single crv from intx
            //Curve fcrv2 = getIntxCrv(crv2); // single crv from intx
            List<Curve> crvs1= getIntxCrv(crv1);
            List<Curve> crvs2 = getIntxCrv(crv2);
            counter++;
            if (counter < 3)
            {
                //if (fcrv1 != null) { recSplit(fcrv1, counter); } //singe return from intersection
                //if (fcrv2 != null) { recSplit(fcrv2, counter); } //singe return from intersection
                for(int i=0; i<crvs1.Count; i++)
                {
                    if (crvs1[i] != null)
                    {
                        recSplit(crvs1[i], counter);
                    }
                }
                for (int i = 0; i < crvs2.Count; i++)
                {
                    if (crvs2[i] != null)
                    {
                        recSplit(crvs2[i], counter);
                    }
                }
            }
            else
            {
                // if (fcrv1 != null) { FCURVE.Add(fcrv1); } //single curve from intersection
                // if (fcrv2 != null) { FCURVE.Add(fcrv2); } //single curve from intersection
                for (int i = 0; i < crvs1.Count; i++)
                {
                    if (crvs1[i] != null)
                    {
                        FCURVE.Add(crvs1[i]);
                    }
                }
                for (int i = 0; i < crvs2.Count; i++)
                {
                    if (crvs2[i] != null)
                    {
                        FCURVE.Add(crvs2[i]);
                    }
                }
            }   
        }

        public List<Point3d[]> verSplit(Point3d[] T)
        {
            // take the curve bounding box, split & return list of point-array : two
            Point3d a = T[0];
            Point3d b = T[1];
            Point3d c = T[2];
            Point3d d = T[3];

            double t = rnd.NextDouble();
            Point3d e = new Point3d(a.X + (b.X - a.X) * t, a.Y, 0);
            Point3d f = new Point3d(d.X + (c.X - d.X) * t, d.Y, 0);

            Point3d[] le = { a, e, f, d, a };
            Point3d[] ri = { e, b, c, f, e };

            //Line line = new Line(e, f);
            //partitionLines.Add(line);

            List<Point3d[]> pts = new List<Point3d[]> { le, ri };
            return pts;
        }

        public List<Point3d[]> horSplit(Point3d[] T)
        {
            // take the curve bounding box, split & return list of point-array : two
            Point3d a = T[0];
            Point3d b = T[1];
            Point3d c = T[2];
            Point3d d = T[3];

            double t = rnd.NextDouble();
            Point3d e = new Point3d(a.X, a.Y + (d.Y - a.Y) * t, 0);
            Point3d f = new Point3d(b.X, e.Y, 0);

            Point3d[] up = { a, b, f, e, a };
            Point3d[] dn = { e, f, c, d, e };

            //Line line = new Line(e, f);
            //partitionLines.Add(line);

            List<Point3d[]> pts = new List<Point3d[]> { up, dn };
            return pts;
        }

        public bool PostProcess() {
            double MINARRATIO = 0.2; // froom BSP file 

            bool REDO = false;
            double ar = 0.0;
            for (int i = 0; i < FCURVE.Count; i++)
            {
                try
                {
                    ar += Rhino.Geometry.AreaMassProperties.Compute(FCURVE[i]).Area;
                }
                catch (Exception) { }
            }
            double meanAr = ar / FCURVE.Count();
            double minArPer = MINARRATIO * meanAr; // con 1: from BSP file 

            // condition 1 
            for (int i=0; i<FCURVE.Count; i++)
            {
                double Ar = Rhino.Geometry.AreaMassProperties.Compute(FCURVE[i]).Area;
                if (Ar < minArPer) // con 1: from BSP file 
                {
                    REDO = true;
                    MSG += "\ncondition. 1:" + redoCounter.ToString();
                    break;
                }
            }
            
            for(int i=0; i<FCURVE.Count; i++)
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
                // condition 2
                if (horDi/verDi < MINARRATIO || verDi / horDi < MINARRATIO) // con 2: from BSP file 
                {
                    REDO = true;
                    MSG += "\ncondition. 2:" + redoCounter.ToString();
                    break;
                }
                double ArBB = Rhino.Geometry.AreaMassProperties.Compute(crv).Area; // area of bounding box
                double ArCrv = Rhino.Geometry.AreaMassProperties.Compute(FCURVE[i]).Area; // actual area of the curve
                // condition 3
                if (ArCrv / ArBB < MINARRATIO) // con 3: from BSP file 
                {
                    REDO = true;
                    MSG += "\ncondition. 3:" + redoCounter.ToString();
                    break;
                }
            }
            return REDO;
        }
    }
}
