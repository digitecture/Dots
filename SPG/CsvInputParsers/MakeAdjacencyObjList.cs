using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dots_dev
{
    class AdjEntry
    {
        private string nameA;
        private string nameB;
        private double val;

        public AdjEntry(string namea, string nameb, double v)
        {
            nameA = namea;
            nameB = nameb;
            val = v;
        }

        public string displayString()
        {
            string s = nameA + "," + nameB + "," + val;
            return s;
        }
    }


    class MakeAdjacencyObjList
    {
        private List<string> input;
        private List<string> adjObjLi;

        public MakeAdjacencyObjList() { }

        public MakeAdjacencyObjList(List<string> inputStrLi)
        {
            input = new List<string>();
            adjObjLi = new List<string>();

            input = inputStrLi;
        }

        public List<string> GetAdjacencyObjList()
        {
            string[] names = input[0].Split(',');
            for(int i=1; i<input.Count; i++)
            {
                string[] strArr = input[i].Split(','); // this row iterating
                string nameA = strArr[0]; //first element of the row
                for (int j = 1; j < strArr.Length; j++)
                {
                    string v = strArr[j];
                    if(nameA.Trim()!="" && v.Trim() != "")
                    {
                        string nameB = names[j]; // first row names after first element
                        try
                        {
                            double val = Convert.ToDouble(v);
                            AdjEntry adjEntry = new AdjEntry(nameA, nameB, val);
                            string str = adjEntry.displayString();
                            adjObjLi.Add(str);
                        }catch(Exception e)
                        {
                            Console.Write(e);
                            // do nothing
                        }                        
                    }
                }
            }
            return adjObjLi;
        }
    }
}
