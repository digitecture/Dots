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
        double MAX_DEV_MEAN; // {0,1}
        int NUM_PARCELS;
        int NUM_PARCELS_REQ;
        double ROTATION; // {0, 2.PI}
        Point3d CEN;

        BspObj myBspObj;

        public BSPAlg(Curve crv, int numParcels, double devMean, double rot)
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
            FCURVE = new List<Curve>();

            //run the bsp algorithm
            Curve crv = SiteCrv.DuplicateCurve();
            recSplit(crv, 0);

            // new bsp object
            myBspObj = new BspObj(FCURVE, NUM_PARCELS_REQ);

            // transform all the curves
            var xform2 = Rhino.Geometry.Transform.Rotation(-ROTATION, CEN);
            SiteCrv.Transform(xform2);
            for(int i=0; i<FCURVE.Count; i++) { FCURVE[i].Transform(xform2); }
        }

        public BspObj GetBspObj() { return myBspObj; }

        public List<Curve> GetBspResults() { return FCURVE; }

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

            // 2 bounding box of the input curve from recursive split function
            PolylineCurve crv1 = new PolylineCurve(polyPts[0]);
            PolylineCurve crv2 = new PolylineCurve(polyPts[1]);

            // get intersection with main site crv
            Curve[] crvs1 = Curve.CreateBooleanIntersection(SiteCrv, crv1); 
            Curve[] crvs2 = Curve.CreateBooleanIntersection(SiteCrv, crv2); 

            counter++;
            if (counter < NUM_PARCELS) // from GUI ; file: BSP
            {
                try
                {
                    if (crvs1.Length > 0) { for (int i = 0; i < crvs1.Length; i++) { recSplit(crvs1[i], counter); } }
                }
                catch (Exception) { }

                try
                {
                    if (crvs2.Length > 0) { for (int i = 0; i < crvs2.Length; i++) { recSplit(crvs2[i], counter); } }
                }
                catch (Exception) { }

            }
            else
            {
                for (int i = 0; i < crvs1.Length; i++) { FCURVE.Add(crvs1[i]); }
                for (int i = 0; i < crvs2.Length; i++) { FCURVE.Add(crvs2[i]); }
            }
        }

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
