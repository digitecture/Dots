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
        Polyline POLY;
        public ParcelsFromPolyUtil(Curve site, Curve poly)
        {
            SITE = site;
            var t = poly.TryGetPolyline(out Polyline poly2);
            POLY = poly2;
        }

        public void extendPolyVecs()
        {

        }
    }
}
