using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace dots_dev
{
    public class BspGeom
    {
        List<Polyline> fPolys = new List<Polyline>();
        List<PolylineCurve> fPolyCrv = new List<PolylineCurve>();
        List<string> rVal = new List<string>();
        Random rnd = new Random();

        public BspGeom() { }

        public List<PolylineCurve> GetFPolys()
        {
            return fPolyCrv;
        }
        public List<string> GetRVals()
        {
            return rVal;
        }

        public Polyline GenerateInitialCurve(List<GeomEntry> geomobjli)
        {

            double sum = 0.0;
            for (int i = 0; i < geomobjli.Count; i++)
            {
                double ar = geomobjli[i].getArea();
                int num = geomobjli[i].getNumber();
                sum += (ar * num);
            }

            double di = Math.Sqrt(sum);
            Point3d[] pts = new Point3d[5];
            pts[0] = new Point3d(0, 0, 0);
            pts[1] = new Point3d(di, 0, 0);
            pts[2] = new Point3d(di, di, 0);
            pts[3] = new Point3d(0, di, 0);
            pts[4] = new Point3d(0, 0, 0);
            Polyline poly = new Polyline(pts);
            return poly;
        }

        //public void RunRecursions(PolylineCurve poly, int counter, List<PolylineCurve> POLYS)
        public void RunRecursions(Point3d[] inpPolyPts, int counter)
        {
            counter++;
            double t = rnd.NextDouble();
            List<Point3d[]> retPolyPts;
            if (t < 0.5) { retPolyPts = VerSplit(inpPolyPts); }
            else { retPolyPts = HorSplit(inpPolyPts); }

            // recursion call
            if (counter < 3)
            {
                RunRecursions(retPolyPts[0], counter);
                RunRecursions(retPolyPts[1], counter);
            }
            else
            {
                // first polyline curve
                PolylineCurve crv0 = new PolylineCurve(retPolyPts[0]);
                fPolyCrv.Add(crv0);

                // second polyline curve
                PolylineCurve crv1 = new PolylineCurve(retPolyPts[1]);
                fPolyCrv.Add(crv1);
            }
        }

        public List<Point3d[]> VerSplit(Point3d[] arr)
        {
            double t = rnd.NextDouble();
            if (t < 0.2) t = 0.2;
            rVal.Add(t.ToString());

            // input
            Point3d a = arr[0];
            Point3d b = arr[1];
            Point3d c = arr[2];
            Point3d d = arr[3];

            // vertical split
            Point3d e = new Point3d(a.X, (a.Y + d.Y) * t, 0);
            Point3d f = new Point3d(b.X, e.Y, 0);
            Point3d[] pts1 = { a, b, f, e, a };
            Point3d[] pts2 = { e, f, c, d, e };

            List<Point3d[]> ptLi = new List<Point3d[]> { pts1, pts2 };
            return ptLi;
        }

        public List<Point3d[]> HorSplit(Point3d[] arr)
        {
            double t = rnd.NextDouble();
            if (t < 0.2) t = 0.2;
            rVal.Add(t.ToString());

            // input 
            Point3d a = arr[0];
            Point3d b = arr[1];
            Point3d c = arr[2];
            Point3d d = arr[3];

            // horizontal split
            Point3d e = new Point3d((a.X +b.X)*t, a.Y, 0);
            Point3d f = new Point3d(e.X, d.Y , 0);
            Point3d[] pts1 = { a, e, f, d, a };
            Point3d[] pts2 = { e, b, c, f, e };

            List<Point3d[]> ptLi = new List<Point3d[]>{ pts1, pts2 };
            return ptLi;
        }
    }
}



