using System;
using System.Collections.Generic;

namespace DotsProj
{
    public class GeomObj
    {
        public string Name { get; set; }
       
        public double Area2 { get; set; } // Area is a keyword!!!
        public double RatioLW { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public int Number { get; set; }

        private string OPT = ""; // send option of the constructor

        public GeomObj() { }
        public GeomObj(string name, double area2, double length, double width, int num)
        {
            this.Name = name;
            this.Area2 = area2;
            this.Length = length;
            this.Width = width;
            this.Number = num;
            this.OPT = "final opt";
        }
        public override string ToString()
        {
            string s = Name + "," + Area2 + "," + Length + "," + Width + "," + Number + ","+ OPT;
            return s;
        }
    }
}
