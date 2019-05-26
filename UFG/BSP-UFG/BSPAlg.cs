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

        // constraints from gui //
        double MAX_DEV_MEAN_AREA; // {0,1}
        double MAX_DEV_RATIO_AREA; // {0,1}
        double MAX_DEV_DIM; // {0,1}
        int NUM_PARCELS;
        int MAX_ITERATION;
        double ROTATION; // {0, 2.PI}
        Point3d CEN;

        List<BspObj> bspObjs;
        BspObj myBspObj;

        public BSPAlg(Curve crv, int numParcels, double devMeanAr, double devDim, double ratioAr, double rot, int numItrs)
        {
            SiteCrv = crv;
            NUM_PARCELS = (int)(Math.Log(numParcels) / Math.Log(2.0));
            MAX_DEV_MEAN_AREA = devMeanAr;
            MAX_DEV_DIM = devDim;
            MAX_DEV_RATIO_AREA = ratioAr;
            ROTATION = rot;
            MAX_ITERATION = numItrs;

            CEN = Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Centroid;
            var xform = Rhino.Geometry.Transform.Rotation(ROTATION, CEN);
            SiteCrv.Transform(xform);

            bspObjs = new List<BspObj>();
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
            bspObjs =new List<BspObj>();
            FCURVE = new List<Curve>();

            //run the bsp algorithm
            Curve crv = SiteCrv.DuplicateCurve();
            recSplit(crv, 0);

            // RECURSIVELY optimize the parcel generation strategy
            /*
            
            // multiple iterations     
            bspObjs.Add(new BspObj(FCURVE, 
                MAX_DEV_MEAN_AREA, 
                MAX_DEV_DIM, 
                MAX_DEV_RATIO_AREA, 
                redoCounter));

            redoCounter++;

            if (redoCounter < MAX_ITERATION) { RUN_BSP_ALG(); }
            else
            {
                var xform2 = Rhino.Geometry.Transform.Rotation(-ROTATION, CEN);
                SiteCrv.Transform(xform2);
                for(int i=0; i<FCURVE.Count; i++)
                {
                    FCURVE[i].Transform(xform2);
                }
            }
            */


            // single itreration

            // single bsp Object
            myBspObj = new BspObj(FCURVE, MAX_DEV_MEAN_AREA, MAX_DEV_DIM, MAX_DEV_RATIO_AREA, redoCounter);
            //single transformation
            var xform2 = Rhino.Geometry.Transform.Rotation(-ROTATION, CEN);
            SiteCrv.Transform(xform2);
            for (int i = 0; i < FCURVE.Count; i++)
            {
                FCURVE[i].Transform(xform2);
            }
        }

        public BspObj GetBspObj()
        {
            return myBspObj;
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

            if (horDi > verDi) { MSG += ".H"; polyPts = verSplit(iniPts); }
            else { MSG += ".V"; polyPts = horSplit(iniPts); }

            // returned = 2 bounding box: find the portion of curve it intersects with
            PolylineCurve crv1 = new PolylineCurve(polyPts[0]);
            PolylineCurve crv2 = new PolylineCurve(polyPts[1]);
            List<Curve> crvs1= getIntxCrv(crv1);
            List<Curve> crvs2 = getIntxCrv(crv2);
            counter++;
            if (counter < NUM_PARCELS) // from GUI ; file: BSP
            {
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

    }
}
