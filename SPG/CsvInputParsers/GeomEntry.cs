using System;
using System.Collections.Generic;

namespace dots_dev
{
    public class GeomEntry
    {
        private string Name;
        private string Parent;
        private double Area;
        private double RatioA;
        private double RatioB;
        private double Length=0.00;
        private double Width=0.00;
        private int Number = 1;

        string OPT = ""; // send option of the constructor

        public GeomEntry() { }
        public GeomEntry(string name, String parent, double area, double a, double b, int num)
        {
            Name = name;
            Parent = parent;
            Area = area;
            RatioA = a;
            RatioB = b;
            Length = Math.Sqrt(Area * RatioA/RatioB);
            Width = Area / Length;
            Number = num;
            OPT = "opt-0";
        }
        public GeomEntry(string name, string parent, double l, double w, int num)
        {
            Name = name;
            Parent = parent;
            Length = l;
            Width = w;
            Number = num;
            OPT = "opt-1";
            Area = Length * Width;
            RatioA = Length / (Length + Width);
            RatioB = Width / (Length + Width);
        }
        public string displayString()
        {
            string s = getName() + "," + getParent() + "," + getArea() + "," + getLength() + "," + getWidth() + "," + getNumber() + "," + OPT;
            return s;
        }
        public string getName() { return Name; }
        public string getParent() { return Parent;  }
        public double getLength() { return Length;  }
        public void setLength(double t) { Length = t; }
        public double getWidth() { return Width; }
        public void setWidth(double t) { Width = t; }
        public double getArea() { return Area; }
        public void setArea(double t) { Area = t; }
        public int getNumber() { return Number; }
    }
}
