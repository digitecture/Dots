using System;
using System.Collections.Generic;

using Rhino.Geometry;

namespace UFG
{
    public class InputProc
    {
        protected List<string> STREET_NAMES;
        protected List<double> SETBACKS;
        protected List<LineCurve> STREETLINES;
        protected List<List<LineCurve>> STREETLINESPROC;
        protected List<ProcObj> PROCOBJLI;

        public InputProc() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UFG.InputProc"/> class.
        /// </summary>
        /// <param name="streets">Streets.</param>
        /// <param name="setbacks">Setbacks.</param>
        public InputProc(string streets, string setbacks)
        {
            STREET_NAMES = new List<string>();
            SETBACKS = new List<double>();
            STREETLINES = new List<LineCurve>();
            PROCOBJLI = new List<ProcObj>();

            STREET_NAMES = ProcessStringlist(streets);
            List<string> setbackStr = ProcessStringlist(setbacks);
            SETBACKS = ProcSetbacks(setbackStr);

            //updates global protected variables
            GetLayerGeom(); // STREETLINES, STREETLINESPROC updated from GetLayerGeom() method
            GenProcObj(); // PROCOBJLI updated from GetLayerGeom() method
        }

        /// <summary>
        /// Processes the stringlist.
        /// generate list of strings by splitting a string
        /// </summary>
        /// <returns>The stringlist.</returns>
        /// <param name="input">Input.</param>
        public List<string> ProcessStringlist(string input)
        {
            List<string> names = new List<string>();
            string[] W = input.Split(',');
            for (int i = 0; i < W.Length; i++)
            {
                names.Add(W[i].Trim().ToUpper());
            }
            return names;
        }

        /// <summary>
        /// Procs the setbacks.
        /// generate list of double for setbacks from strings 
        /// </summary>
        /// <returns>The setbacks.</returns>
        /// <param name="input">Input.</param>
        public List<double> ProcSetbacks(List<string> input)
        {
            List<double> setbacks = new List<double>();
            for (int i=0; i< input.Count; i++)
            {
                double e = Convert.ToDouble(input[i]);
                setbacks.Add(e);
            }
            return setbacks;
        }

        /// <summary>
        /// generate list of lists for street lines 
        /// corresponding to 
        /// layer name
        /// setback distances
        /// </summary>
        public void GetLayerGeom()
        {
            List<String> names = STREET_NAMES;
            STREETLINES = new List<LineCurve>();
            STREETLINESPROC= new List<List<LineCurve>>();
            for (int i=0; i< names.Count; i++)
            {
                Rhino.DocObjects.RhinoObject[] rhobjs = Rhino.RhinoDoc.ActiveDoc.Objects.FindByLayer(names[i]);
                try
                {
                    List<LineCurve> templi = new List<LineCurve>();
                    for (int j = 0; j < rhobjs.Length; j++)
                    {
                        LineCurve line = (LineCurve) rhobjs[j].Geometry;
                        STREETLINES.Add(line);
                        templi.Add(line);
                    }
                    STREETLINESPROC.Add(templi);
                }
                catch (Exception){}
            }
        }

        /// <summary
        /// <returns> generate list of proc obj with </returns>
        /// layer name, 
        /// street lines, 
        /// setback distance
        /// </summary>
        public void GenProcObj()
        {
            for (int i = 0; i < STREETLINESPROC.Count; i++)
            {
                List<LineCurve> crvs = STREETLINESPROC[i];
                string name = STREET_NAMES[i];
                double dist = SETBACKS[i];
                ProcObj procObj = new ProcObj(crvs, name, dist);
                PROCOBJLI.Add(procObj);
            }
        }


        /// <summary>
        /// Gets the proc object li string.
        /// </summary>
        /// <returns>The proc object li string.</returns>
        public List<string> GetProcObjLiString()
        {
            List<string> PROCOBJLIStr = new List<string>();
            for (int i = 0; i < PROCOBJLI.Count; i++) { 
                PROCOBJLIStr.Add(PROCOBJLI[i].DisplayProc()); 
            }
            return PROCOBJLIStr;
        }

        public List<string> GetStreetNames() { return STREET_NAMES; }
        public List<double> GetSetbacks() { return SETBACKS; }
        public List<LineCurve> GetLinesFromLayers() { return STREETLINES; }
        public List<ProcObj> GetProcObjs() { return PROCOBJLI; }
    }
}
