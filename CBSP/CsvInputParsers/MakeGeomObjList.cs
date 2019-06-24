using System;
using System.Collections.Generic;

namespace DotsProj
{
    class MakeGeomObjList
    {
        private List<string> input;
        private List<string> GeomObjLiStr;
        private List<GeomObj> GeomObjLi;
        private double SiteAr;

        public MakeGeomObjList() { }

        public MakeGeomObjList(List<string> inputstrli, double site_ar_)
        {
            GeomObjLiStr = new List<string>();
            GeomObjLi=new List<GeomObj>();
            input = new List<string>();
            input = inputstrli;
            this.SiteAr = site_ar_;
        }

        public double GetDoubleFromString(string Value)
        {
            double x = 0.00;
            if (Value == null)
            {
                return 0;
            }
            else
            {
                double OutVal;
                double.TryParse(Value, out OutVal);

                if (double.IsNaN(OutVal) || double.IsInfinity(OutVal))
                {
                    return 0;
                }
                return OutVal;
            }
        }

        public double GetInt16FromString(string str)
        {
            int x = 0;
            string s = str.Trim();
            try
            {
                if (String.Equals(str, "")) {
                    return x;
                }else if (str == null)
                {
                    return 0;
                }
                else
                {
                    return Convert.ToInt16(s);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public List<string> GetGeomObjListStr()
        {
            for (int i = 1; i < input.Count; i++)
            {
                int opt = 0; // if 0, use area, ratio else use length, width
                // format of the inputs: name[0], area[1], ratio (a:b)[2], number[3], length[4], width[5]
                string name = ""; 
                double Area = 0.0;
                int num = 0;
                double ratio = 0.0;
                double le = 0.0;
                double wi = 0.0;
                try
                {
                    name = input[i].Split(',')[0].Trim().ToLower();
                }
                catch (Exception)
                {
                    continue;
                }
                try
                {
                    Area = GetDoubleFromString(input[i].Split(',')[1]);
                }
                catch (Exception) { Area = 0.0; }
                try
                {
                    ratio = Convert.ToDouble(input[i].Split(',')[2]);
                }
                catch (Exception) { ratio = 0.0; }
                try
                {
                    num = Convert.ToInt32(input[i].Split(',')[3]);
                }
                catch (Exception) { num = 0; }
                try
                {
                    le = Convert.ToDouble(input[i].Split(',')[4]);
                }
                catch (Exception) { le = 0.0; }
                try
                {
                    wi = Convert.ToDouble(input[i].Split(',')[5]);
                }
                catch (Exception) { wi = 0.0; }
                if (num == 0) { continue; }
                if(Area > 0 && ratio>0)
                {
                    le = Area * ratio;
                    wi = Area / le;

                }else if(le>0 && wi > 0)
                {
                    Area = le * wi;
                }
                else
                {
                    continue;
                }
                string str = string.Format("name: {0}, area: {1}, num: {2}, length: {3}, width: {4}", name, Area, num, le, wi);
                GeomObjLiStr.Add(str);
                GeomObj geomEntry = new GeomObj(name, Area, le, wi, num);
                GeomObjLi.Add(geomEntry);
            }
            return GeomObjLiStr;
        }
        
        public List<GeomObj> GetGeomObj()
        {
            return GeomObjLi;
        }

        public List<GeomObj> NormalizeGeomObj(List<GeomObj> geomEntryObjLi, double siteAr)
        {
            List<GeomObj> norGeomEntryObjLi = new List<GeomObj>();
            double sumAr=0.0;
            for(int i=0; i<geomEntryObjLi.Count; i++)
            {
                sumAr += geomEntryObjLi[i].Area2;
            }
            for (int i = 0; i < geomEntryObjLi.Count; i++)
            {
                GeomObj newObj = geomEntryObjLi[i];
                int num = geomEntryObjLi[i].Number;
                double area3= geomEntryObjLi[i].Area2 * siteAr / sumAr;
                double ar_each = area3 / num;
                for(int j=0; j<num; j++)
                {
                    GeomObj newObj2 = geomEntryObjLi[i];
                    newObj2.Area2 = ar_each;
                    newObj2.Number = 1;
                    newObj2.RatioLW = geomEntryObjLi[i].Length / (geomEntryObjLi[i].Length + geomEntryObjLi[i].Width);
                    norGeomEntryObjLi.Add(newObj2);
                }
                // newObj.Area2= geomEntryObjLi[i].Area2 * siteAr / sumAr;
                // newObj.RatioLW = geomEntryObjLi[i].Length / (geomEntryObjLi[i].Length + geomEntryObjLi[i].Width);
                // norGeomEntryObjLi.Add(newObj);
            }
            return norGeomEntryObjLi;
        }
    }   // end of public class
}   // end of namespace
