using System;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

//created in InputProc
// object with list<street>, layer name, setback
namespace UFG
{
    public class ProcObj
    {
        protected List<LineCurve> STREETLINES = new List<LineCurve>(); 
        protected string LAYERNAME = "";
        protected double SETBACKDIST=1.0;

        public ProcObj(List<LineCurve> linecurves, string name, double dist)
        {
            STREETLINES = linecurves;
            LAYERNAME = name;
            SETBACKDIST = dist;
        }

        public string GetLayerName()
        {
            return LAYERNAME;
        }
        public double GetSetbackDist()
        {
            return SETBACKDIST;
        }
        public int GetStreetCount()
        {
            return STREETLINES.Count;
        }
        public List<LineCurve> GetStreetLineCurves()
        {
            return STREETLINES;
        }
        public string DisplayProc()
        {
            string s = LAYERNAME + "; ";
            s += "; number of lines = " + STREETLINES.Count;
            s += "; setback dist = " + SETBACKDIST;
            return s;
        }
    }
}
