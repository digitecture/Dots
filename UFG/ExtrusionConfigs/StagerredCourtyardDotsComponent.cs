using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace StagerredCourtyardDots
{
    public class StagerredCourtyardDotsComponent : GH_Component
    {
        public StagerredCourtyardDotsComponent()
          : base("StagerredCourtyardDotsComponent", "Nickname",
            "StagerredCourtyardDotsComponent description",
            "Category", "Subcategory")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("site", "site", "site", GH_ParamAccess.item);
            pManager.AddNumberParameter("offset", "offset", "offset", GH_ParamAccess.item);
            pManager.AddIntegerParameter("number of divisions", "div", "number of divisions of the site curve", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("offsetcrv", "offsetCurve", "", GH_ParamAccess.item);
            pManager.AddPointParameter("points on crv", "ptsOnCrv", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("line normal", "nor", "line normal at point of division", GH_ParamAccess.list);
            pManager.AddCurveParameter("poly from sub-div of crvs", "poly", "polygons from the sub-divions of curves", GH_ParamAccess.list);
            pManager.AddTextParameter("debug", "debug", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        { 
            Curve site = null;
            double offset = double.NaN;
            int numDiv = 10;

            if (!DA.GetData(0, ref site)) return;
            if (!DA.GetData(1, ref offset)) return;
            if (!DA.GetData(2, ref numDiv)) return;

            Curve c2 = site.DuplicateCurve();
            Curve[] c2Offs;
            Point3d cen = AreaMassProperties.Compute(c2).Centroid;
            Rhino.Geometry.PointContainment cont = site.Contains(cen);
            if (cont.ToString() == "Inside")
            {
                c2Offs = c2.Offset(cen, Vector3d.ZAxis, offset, 0.01, CurveOffsetCornerStyle.Sharp);
            }
            else
            {
                c2Offs = c2.Offset(cen, Vector3d.ZAxis, -offset, 0.01, CurveOffsetCornerStyle.Sharp);
            }
            DA.SetData(4, cont);
            Curve c2Off = c2Offs[0];
            DA.SetData(0, c2Off);

            double[] p= c2Off.DivideByCount(numDiv, true);
            List<Point3d> ptLi=new List<Point3d>();
            for(int i=0; i<p.Length; i++)
            {
                Point3d pts = c2Off.PointAt(p[i]);
                ptLi.Add(pts);
            }
            DA.SetDataList(1, ptLi);

            List<LineCurve> lineLi = new List<LineCurve>();
            for (int i = 0; i < ptLi.Count; i++)
            {
                Point3d P = Point3d.Unset;
                Point3d Q = Point3d.Unset;
                if (i == 0)
                {
                    P = ptLi[ptLi.Count - 1];
                    Q = ptLi[0];
                }
                else
                {
                    P = ptLi[i - 1];
                    Q = ptLi[i];
                }
                double dx = P.X - (Q.Y - P.Y);
                double dy = P.Y + (Q.X - P.X);
                double ex = P.X + (Q.Y - P.Y) * 100;
                double ey = P.Y - (Q.X - P.X) * 100;
                Point3d u = new Point3d(dx, dy, 0);
                Point3d v = new Point3d(ex, ey, 0);
                LineCurve linePu = new LineCurve(P, u);
                LineCurve linePv = new LineCurve(P, v);
                var intxPv = Rhino.Geometry.Intersect.Intersection.CurveCurve(site, linePv, 0.01, 0.01);
                var intxPu = Rhino.Geometry.Intersect.Intersection.CurveCurve(site, linePu, 0.01, 0.01);
                try
                {
                    Point3d Av = intxPv[0].PointA;
                    Point3d Au = intxPu[0].PointA2;
                    if (P.DistanceTo(Av) > P.DistanceTo(Au))
                    {
                        LineCurve linePw = new LineCurve(P, Au);
                        lineLi.Add(linePw);
                    }
                    else
                    {
                        LineCurve linePw = new LineCurve(P, Av);
                        lineLi.Add(linePw);
                    }
                }catch (Exception) { }
            }
            
            DA.SetDataList(2, lineLi);

            List<PolylineCurve> polyCrvLi = new List<PolylineCurve>();
            for(int i=0; i<lineLi.Count; i++)
            {
                if (i==0)
                {
                    LineCurve A = lineLi[lineLi.Count-1];
                    LineCurve B = lineLi[0];
                    Point3d a0 = A.PointAtStart;
                    Point3d a1 = A.PointAtEnd;
                    Point3d b0 = B.PointAtStart;
                    Point3d b1 = B.PointAtEnd;
                    Point3d[] pts = { a0, b0, b1, a1, a0 };
                    PolylineCurve poly = new PolylineCurve(pts);
                    polyCrvLi.Add(poly);
                }
                else
                {
                    LineCurve A = lineLi[i-1];
                    LineCurve B = lineLi[i];
                    Point3d a0 = A.PointAtStart;
                    Point3d a1 = A.PointAtEnd;
                    Point3d b0 = B.PointAtStart;
                    Point3d b1 = B.PointAtEnd;
                    Point3d[] pts = { a0, b0, b1, a1, a0 };
                    PolylineCurve poly = new PolylineCurve(pts);
                    polyCrvLi.Add(poly);
                }
            }
            DA.SetDataList(3, polyCrvLi);
        }

        protected override System.Drawing.Bitmap Icon { get { return null; } }

        public override Guid ComponentGuid {  get { return new Guid("48a70716-4b69-4b4c-8440-24be62b00616"); }  }
    }
}
