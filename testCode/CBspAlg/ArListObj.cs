using System;
using System.Collections.Generic;
using System.Text;

namespace CBspAlg
{
    class ArListObj
    {
        public List<double> inp;
        public List<List<double>> arLi;
        private int ID;
        public ArListObj(List<double> inp_, List<List<double>> arLi_, int id)
        {
            inp = inp_;
            this.arLi = arLi_;
            this.ID = id;
        }
        public string Display()
        {
            string s = "\n\nGeneration= " + ID.ToString() + " INPUT: ";
            for (int i = 0; i < inp.Count; i++)
            {
                s += inp[i].ToString() + ", ";
            }
            s += "\nSPLIT-RESULT: ";
            for (int i = 0; i < arLi.Count; i++)
            {
                s += "\n";
                for (int j = 0; j < arLi[i].Count; j++)
                {
                    s += arLi[i][j].ToString() + ",";
                }
            }
            return s;
        }
    }
}
