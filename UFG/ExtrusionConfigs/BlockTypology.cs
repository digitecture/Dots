using System;
using System.Collections.Generic;

using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace Config
{
    public class BlockTypology
    {
        private Curve SiteCrv=null;
        private double Setback = 0.0;
        private List<double> StepbackArr;
        private double FloorDepth = 0.0;
        private double BayGap = 0.0; // courtyard, distance between two double-loaded bays
        private double FSR = 2.75;
        private List<double> StagerredFSR;
        public BlockTypology()
        {
           
        }
    }
}
