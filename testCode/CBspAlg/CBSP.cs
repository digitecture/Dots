using System;
using System.Collections.Generic;
using System.Text;

using System.Linq;

namespace CBspAlg
{
    class CBSP
    {
        Random rnd = new Random();
        private List<double> arLi = new List<double>();

        private List<ArListObj> arliobj;
        private List<List<double>> bspLi = new List<List<double>>();

        private int recursion;

        public CBSP() { }

        private int itrNum;

        public CBSP(List<double> arLi_, int num)
        {
            recursion = 0;
            itrNum = num;
            arLi = arLi_;
            arliobj = new List<ArListObj>();
            runRecursions();
        }



        public void runRecursions()
        {
            int gen = 0;
            for (int i = 0; i < itrNum; i++)
            {
                bspLi=new List<List<double>>();
                List<double> inp2 = randomShuffle(arLi);
                SPLIT(inp2);
                ArListObj obj = new ArListObj(inp2, bspLi, gen);
                arliobj.Add(obj);
                gen++;
            }
            
            Console.WriteLine("\n\n\n----------------------");
            for(int i=0; i<arliobj.Count; i++)
            {
                string s=arliobj[i].Display();
                Console.WriteLine(s);
            }
        }

        public void SPLIT(List<double> inLi)
        {
            int t = rnd.Next(inLi.Count);
            List<double> le = new List<double>();
            List<double> ri = new List<double>();
            for(int i=0; i<t; i++) { le.Add(inLi[i]); }
            for(int i=t; i<inLi.Count; i++) { ri.Add(inLi[i]); }
            
            // add to global list
            if (le.Count>0)
            {
                bool tx=checkDup(le);
                if (tx == false)
                {
                    bspLi.Add(le);
                }
            }

            if (ri.Count > 0)
            {
                bool tx = checkDup(ri);
                if (tx == false)
                {
                    bspLi.Add(ri);
                }                
            }
            
            // go into recursion
            if (le.Count > 1) { recursion++; SPLIT(le); }
            if (ri.Count > 1) { recursion++; SPLIT (ri); }
        }

        public List<double> randomShuffle(List<double> li)
        {
            List<double> newli =new List<double>();
            for(int i=0; i<li.Count; i++)
            {
                int r = rnd.Next(i,li.Count);
                double me = li[i];
                double other = li[r];
                double tmp = me;
                li[i] = other;
                li[r] = tmp;
                newli.Add(other);
            }
            return newli;
        }

        public bool checkDup(List<double> inp)
        {
            bool t = false; // not a duplicate
            double[] inpArr = inp.ToArray();
            for (int i = 0; i < bspLi.Count; i++)
            {
                double[] bspArr = bspLi[i].ToArray();
                bool T = inpArr.SequenceEqual(bspArr);
                if (T == true)
                {
                    t = true; //duplicate found
                    break;
                }
            }
            return t;
        }
    }    
}
