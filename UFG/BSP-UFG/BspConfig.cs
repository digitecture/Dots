using System;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

namespace DOTS.SourceCode.UFG.BSPUFG

{
    public class BspConfig
    {
        List<Curve> CRVLI;

        public BspConfig(List<Curve> crvli)
        {
            CRVLI = new List<Curve>();
            CRVLI = crvli;
        }

    }
}
