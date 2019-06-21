using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Rhino;
using Rhino.Geometry;

namespace DotsProj
{
    class Seg
    {
        public double dist { get; set; }
        public Point3d A { get; set; }
        public Point3d B { get; set; }
        public Seg() { }
        public Seg(Point3d a, Point3d b)
        {
            this.A = a;
            this.B = b;
            this.dist=Dist();
        }
        public double Dist()
        {
            return A.DistanceTo(B);
        }
        public Point3d MP()
        {
            double x = (A.X + B.X) / 2;
            double y = (A.Y + B.Y) / 2;
            Point3d p = new Point3d(x, y, 0);
            return p;
        }
    }
}
