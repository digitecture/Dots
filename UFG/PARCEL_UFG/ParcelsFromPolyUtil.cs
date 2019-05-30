using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace UFG
{
    public class ParcelsFromPolyUtil
    {
        Curve SITE;
        Curve INT_CRV;
        Polyline POLY;
        List<Point3d> PT_LI;
        List<Line> lineSeg;

        public ParcelsFromPolyUtil(Curve site, Curve intCrv)
        {
            SITE = site;
            INT_CRV = intCrv;
            var t = INT_CRV.TryGetPolyline(out Polyline poly2);
            POLY = poly2;
            IEnumerator<Point3d> pts = POLY.GetEnumerator();
            PT_LI = new List<Point3d>();
            while (pts.MoveNext())
            {
                PT_LI.Add(pts.Current);
            }
        }

        public void getHalfPlaneSegs()
        {
            var B = SITE.GetBoundingBox(true);
            Point3d a = B.Min;
            Point3d c = B.Max;
            Point3d b = new Point3d(c.X, a.Y, 0);
            Point3d d = new Point3d(a.X, c.Y, 0);
            Point3d[] pts = { a, b, c, d, a };
            PolylineCurve poly = new PolylineCurve(pts);

        }
    }
}
