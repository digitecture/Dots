using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Rhino.Geometry;

namespace UFG

{
    class BspUfgAlg
    {
        string MSG = "";
        List<Curve> BspTreeCrvs = new List<Curve>();
        List<Curve> FCURVE = new List<Curve>();
        List<Line> partitionLines = new List<Line>();
        Curve SiteCrv;


        Random rnd = new Random();

        // constraints from gui //
        double MAX_DEV_MEAN; // {0,1}
        int NUM_PARCELS;
        int NUM_PARCELS_REQ=0;
        double ROTATION; // {0, 2.PI}
        Point3d CEN;

        BspUfgObj myBspObj;

        public BspUfgAlg(Curve crv, int numParcels, double devMean, double rot)
        {
            SiteCrv = crv;
            NUM_PARCELS = (int)((Math.Log(numParcels) / Math.Log(2.0))+1);
            NUM_PARCELS_REQ = numParcels;
            MAX_DEV_MEAN = devMean;
            ROTATION = rot;

            CEN = Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Centroid;
            var xform = Rhino.Geometry.Transform.Rotation(ROTATION, CEN);
            SiteCrv.Transform(xform);
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
           
            //run the bsp algorithm
            Curve crv = SiteCrv.DuplicateCurve();
            FCURVE = new List<Curve>();
            BspTreeCrvs= new List<Curve>();
            BspTreeCrvs.Add(crv);
            recSplit(0);

            // new bsp object
            myBspObj = new BspUfgObj(FCURVE, NUM_PARCELS_REQ);

            // transform all the curves
            var xform2 = Rhino.Geometry.Transform.Rotation(-ROTATION, CEN);
            SiteCrv.Transform(xform2);
            for(int i=0; i<FCURVE.Count; i++) { FCURVE[i].Transform(xform2); }
        }

        public BspUfgObj GetBspObj() { return myBspObj; }

        public List<Curve> GetBspResults() { return FCURVE; }

        public void recSplit(int recCounter)
        {
            MSG += "\nrecursion Counter=" 
                + recCounter.ToString()
                +";  crvs in stack: "
                +BspTreeCrvs.Count.ToString();

            Curve inicrv = BspTreeCrvs[recCounter];
            int N = NUM_PARCELS_REQ;
            int n = BspTreeCrvs.Count - recCounter;
            if (n < N)
            {
                recCounter++;
                sendForRecursiveSplit(inicrv, recCounter);
            }
            else
            {
                for(int i=recCounter; i<BspTreeCrvs.Count; i++)
                {
                    MSG += "\naccepted crv index: " + i.ToString();
                    Curve crv = BspTreeCrvs[i];
                    FCURVE.Add(crv);
                }
            }
        }

        public void sendForRecursiveSplit(Curve crv, int rec_counter) { 
            var T = crv.GetBoundingBox(true);
            Point3d a = T.Min; Point3d c = T.Max;
            Point3d b = new Point3d(c.X, a.Y, 0); Point3d d = new Point3d(a.X, c.Y, 0);
            double horDi = a.DistanceTo(b); double verDi = a.DistanceTo(d);
            Point3d[] iniPts = { a, b, c, d, a };

            List<Point3d[]> polyPts = new List<Point3d[]>(); //persistent data

            if (horDi > verDi) { polyPts = verSplit(iniPts); }
            else { polyPts = horSplit(iniPts); }

            // 2 bounding box of the input curve from recursive split function
            PolylineCurve crv1 = new PolylineCurve(polyPts[0]);
            PolylineCurve crv2 = new PolylineCurve(polyPts[1]);

            // get intersection with main site crv
            Curve[] crvs1 = Curve.CreateBooleanIntersection(SiteCrv, crv1); 
            Curve[] crvs2 = Curve.CreateBooleanIntersection(SiteCrv, crv2);

            if (crvs1.Length > 0) {
                for (int i = 0; i < crvs1.Length; i++) { BspTreeCrvs.Add(crvs1[i]); }
            }
            if (crvs2.Length > 0) {
                for (int i = 0; i < crvs2.Length; i++) { BspTreeCrvs.Add(crvs2[i]); }
            }

            recSplit(rec_counter);
        }   // end method



        public List<Point3d[]> verSplit(Point3d[] T)
        {
            // take the curve bounding box, split & return list of point-array : two
            Point3d a = T[0];
            Point3d b = T[1];
            Point3d c = T[2];
            Point3d d = T[3];

            int t0 = 1;
            double T0 = rnd.NextDouble();
            if (T0 > 0.5) { t0 = -1; }

            double t = 0.5 + rnd.NextDouble() * MAX_DEV_MEAN * t0;
            Point3d e = new Point3d(a.X + (b.X - a.X) * t, a.Y, 0);
            Point3d f = new Point3d(d.X + (c.X - d.X) * t, d.Y, 0);

            Point3d[] le = { a, e, f, d, a };
            Point3d[] ri = { e, b, c, f, e };

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

            int t0 = 1;
            double T0 = rnd.NextDouble();
            if (T0 > 0.5) { t0 = -1; }

            double t = 0.5 + rnd.NextDouble() * MAX_DEV_MEAN * t0;
            Point3d e = new Point3d(a.X, a.Y + (d.Y - a.Y) * t, 0);
            Point3d f = new Point3d(b.X, e.Y, 0);

            Point3d[] up = { a, b, f, e, a };
            Point3d[] dn = { e, f, c, d, e };

            List<Point3d[]> pts = new List<Point3d[]> { up, dn };
            return pts;
        }
    }
}
