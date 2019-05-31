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
        PolylineCurve SITE_POLY;
        List<Point3d> SITE_PT_LI;
        List<Line> lineSeg;

        public ParcelsFromPolyUtil(Curve site, Curve intCrv)
        {
            SITE = site;
            INT_CRV = intCrv;
            var t = INT_CRV.TryGetPolyline(out Polyline poly2);
            IEnumerator<Point3d> pts = poly2.GetEnumerator();
            SITE_PT_LI = new List<Point3d>();
            while (pts.MoveNext())
            {
                SITE_PT_LI.Add(pts.Current);
            }
            SITE_POLY = new PolylineCurve(SITE_PT_LI);
        }

        public PolylineCurve GetPolyFromB(Curve crv)
        {
            var B = crv.GetBoundingBox(true);
            Point3d[] ptArr = new Point3d[5];
            Point3d a = B.Min;
            Point3d c = B.Max;
            Point3d b = new Point3d(c.X, a.Y, 0);
            Point3d d = new Point3d(a.X, c.Y, 0);
            Point3d[] pts = { a, b, c, d, a };
            PolylineCurve poly = new PolylineCurve(pts);
            return poly;
        }

        public void getHalfPlaneSegs()
        {

        }
    }
}
