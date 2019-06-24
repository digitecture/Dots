using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Rhino.Geometry;

namespace DotsProj

{
    class BspUfgAlg
    {
        string MSG = "";
        public List<Curve> DebugBBX { get; set; }

        List<Curve> BspTreeCrvs = new List<Curve>();
        List<Curve> FCURVE = new List<Curve>();
        List<Curve> ExtractFCrvs = new List<Curve>();
        List<Line> partitionLines = new List<Line>();
        Curve SiteCrv;
        List<Curve> IntCrv;

        Random rnd = new Random();

        // constraints from gui //
        double MAX_DEV_MEAN; // {0,1}
        int NUM_PARCELS=0;
        int NUM_PARCELS_REQ=0;
        double ROTATION; // {0, 2.PI}
        Point3d CEN;

        BspUfgObj myBspObj;

        public BspUfgAlg(Curve crv, int numParcels, double devMean, double rot)
        {
            SiteCrv = crv;
            NUM_PARCELS = (int)((Math.Log(numParcels) / Math.Log(2.0)) + 1);
            NUM_PARCELS_REQ = numParcels;
            MAX_DEV_MEAN = devMean;
            ROTATION = rot;

            IntCrv = new List<Curve>();
            CEN = Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Centroid;
            var xform = Rhino.Geometry.Transform.Rotation(ROTATION, CEN);
            SiteCrv.Transform(xform);
        }

        public BspUfgAlg(Curve crv, List<Curve> intcrv, int numParcels, 
            double devMean, double rot)
        {
            SiteCrv = crv;
            IntCrv = intcrv;
            NUM_PARCELS = (int)((Math.Log(numParcels) / Math.Log(2.0))+1);
            NUM_PARCELS_REQ = numParcels;
            MAX_DEV_MEAN = devMean;
            ROTATION = rot;

            CEN = Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Centroid;
            var xform = Rhino.Geometry.Transform.Rotation(ROTATION, CEN);
            SiteCrv.Transform(xform);
        }

        public Point3d getCentroid() { return Rhino.Geometry.AreaMassProperties.Compute(SiteCrv).Centroid; }

        public string getMSG() { return MSG; }

        public List<Line> getPartitionLines() { return partitionLines; }

        public void RUN_BSP_ALG()
        {
            DebugBBX = new List<Curve>();
            //run the bsp algorithm
            Curve crv = SiteCrv.DuplicateCurve();
            FCURVE = new List<Curve>();
            BspTreeCrvs= new List<Curve>();
            BspTreeCrvs.Add(crv);
            recSplit(0);

            // transform all the curves
            var xform2 = Rhino.Geometry.Transform.Rotation(-ROTATION, CEN);
            SiteCrv.Transform(xform2);
            for(int i=0; i<FCURVE.Count; i++) { FCURVE[i].Transform(xform2); }

            // subtract the internal curves
            ExtractFCrvs = new List<Curve>();
            if (IntCrv.Count > 0) { subtractCrv(); }
            else { ExtractFCrvs = FCURVE; }

            // new bsp object
            // myBspObj = new BspUfgObj(FCURVE, NUM_PARCELS_REQ);
            myBspObj = new BspUfgObj(ExtractFCrvs, NUM_PARCELS_REQ);

        }

        public BspUfgObj GetBspObj() { return myBspObj; }

        public List<Curve> GetBspResults() { return ExtractFCrvs; } //return FCURVE; }

        public void subtractCrv()
        {
            for (int i = 0; i < FCURVE.Count; i++)
            {
                Curve crv = FCURVE[i];
                for (int j = 0; j < IntCrv.Count; j++)
                {
                    Curve crv2 = IntCrv[j];
                    Curve[] crvDiffArr = Curve.CreateBooleanDifference(crv, crv2);//Curve[] crvDiffArr = Curve.CreateBooleanDifference(crv, crv2, 0.01);
                    for (int k = 0; k < crvDiffArr.Length; k++)
                    {
                        ExtractFCrvs.Add(crvDiffArr[k]);
                    }
                }
            }
        }

        public void recSplit(int recCounter)
        {
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

            // get intersection with main (rotated) site crv
            Curve[] crvs1 = Curve.CreateBooleanIntersection(SiteCrv, crv1);// Curve[] crvs1 = Curve.CreateBooleanIntersection(SiteCrv, crv1, 0.01); 
            Curve[] crvs2 = Curve.CreateBooleanIntersection(SiteCrv, crv2);// Curve[] crvs2 = Curve.CreateBooleanIntersection(SiteCrv, crv2, 0.01);

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

            PolylineCurve poly1 = new PolylineCurve(le);
            PolylineCurve poly2 = new PolylineCurve(ri);
            DebugBBX.Add(poly1);
            DebugBBX.Add(poly2);


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

            PolylineCurve poly1 = new PolylineCurve(up);
            PolylineCurve poly2 = new PolylineCurve(dn);
            DebugBBX.Add(poly1);
            DebugBBX.Add(poly2);

            return pts;
        }
    }
}
