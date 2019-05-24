using System;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

namespace UFG
{

    public class ProcessIntx
    {
        protected List<SiteObj> SITEOBJLI;
        protected List<ProcObj> PROCOBJLI;
        protected List<Curve> SITECRVLI;
        protected List<LineCurve> FULLSTREETLI;
        protected List<double> SETBACKDISTLI;
        protected List<string> LAYERNAMES;
        protected double FSR;
        protected double MINHT;
        protected double MAXHT;
        protected int NUMRAYS;
        protected double MAGRAYS;
        protected List<Line> RAYLI;

        public ProcessIntx() { }

        public ProcessIntx(
            List<ProcObj>procobjli, 
            List<Curve>sitecrvs,
            double fsr,
            double minht,
            double maxht,
            int numrays,
            double magrays
           )
        {
            RAYLI = new List<Line>();
            SITEOBJLI = new List<SiteObj>();
            SITECRVLI = new List<Curve>();
            SITECRVLI = sitecrvs;
            PROCOBJLI = new List<ProcObj>();
            PROCOBJLI = procobjli;
            FSR = fsr;
            MAXHT= maxht;
            MINHT= minht;
            NUMRAYS = numrays;
            MAGRAYS = magrays;
            SETBACKDISTLI = new List<double>();
            FULLSTREETLI = new List<LineCurve>();
            for (int i=0;i<PROCOBJLI.Count; i++)
            {
                List<LineCurve> li = PROCOBJLI[i].GetStreetLineCurves();
                for(int j=0; j<li.Count; j++) { FULLSTREETLI.Add(li[j]); }
                SETBACKDISTLI.Add(PROCOBJLI[i].GetSetbackDist());
            }
        }

        public List<Point3d> GetPtsFromCrv(Curve crv)
        {
            List<Point3d> pts = new List<Point3d>();
            try
            {
                var t = crv.TryGetPolyline(out Polyline sitepoly);
                IEnumerator<Point3d> sitePts = sitepoly.GetEnumerator();
                while (sitePts.MoveNext())
                {
                    pts.Add(sitePts.Current);
                }
            }catch(Exception) { }
            return pts;
        }

        public List<Line> GetRaysFromPt(Point3d pt, List<Point3d>pts)
        {
            List<Line> rays = new List<Line>();
            double maxD = -10.0;
            Point3d A = new Point3d();
            Point3d B = new Point3d();
            for(int i=0; i<pts.Count-1; i++)
            {
                Point3d a = pts[i];
                Point3d b = pts[i + 1];
                double e = a.DistanceTo(b);
                if (e > maxD)
                {
                    maxD = e;
                    A = a;
                    B = b;
                }
            }
            double mag = A.DistanceTo(B);
            double sc = mag * MAGRAYS; //input: magrays file:ufg main input file
            Point3d u = new Point3d((B.X - A.X) / mag, (B.Y - A.Y) / mag, 0);
            Point3d v = new Point3d(-u.Y * sc, u.X * sc, 0.0);
            Point3d w = new Point3d(u.Y * sc, -u.X * sc, 0.0);
            Point3d p0 = new Point3d(pt.X + v.X, pt.Y + v.Y, 0);
            Point3d p1 = new Point3d(pt.X + w.X, pt.Y + w.Y, 0);
            Point3d p2 = new Point3d(pt.X + u.X*sc, pt.Y + u.Y*sc, 0);
            Point3d p3 = new Point3d(pt.X - u.X*sc, pt.Y - u.Y*sc, 0);
            rays.Add(new Line(pt, p0));
            rays.Add(new Line(pt, p1));
            rays.Add(new Line(pt, p2));
            rays.Add(new Line(pt, p3));
            for(int i=0; i<rays.Count; i++)
            {
                RAYLI.Add(rays[i]);
            }
            return rays;
        }

        public void GenRays()
        {
            SITEOBJLI = new List<SiteObj>();
            for (int i = 0; i < SITECRVLI.Count; i++)
            {
                Curve sitecrv = SITECRVLI[i];
                Point3d p = AreaMassProperties.Compute(sitecrv).Centroid;
                List<Point3d> sitePts = GetPtsFromCrv(sitecrv);
                List<Line> rays = GetRaysFromPt(p, sitePts);

                Point3d fIntxPt = Point3d.Unset;
                double fsetbackdist = double.NaN;
                Line Ray = new Line();
                double minD = 1000000000000.00;
                for (int j = 0; j < rays.Count; j++)
                {
                    Line ray = rays[j];
                    for(int k=0; k< PROCOBJLI.Count; k++)
                    {
                        List<LineCurve> streets = PROCOBJLI[k].GetStreetLineCurves();
                        double setbackdist = PROCOBJLI[k].GetSetbackDist();
                        Point3d intxPt = GetIntx(ray, streets, sitecrv, setbackdist);
                        double d = p.DistanceTo(intxPt);
                        if (d < minD)
                        {
                            minD = d;
                            fIntxPt = intxPt;
                            fsetbackdist = setbackdist;
                            Ray = ray;
                        }
                    }
                }
                SiteObj obj = new SiteObj(Ray, sitecrv, fIntxPt, fsetbackdist, p, FSR);
                SITEOBJLI.Add(obj);
            }
        }

        public Point3d GetIntx(Line lineA, List<LineCurve> streets, Curve sitecrv, double setbackdist)
        {
            Point3d intxPt = new Point3d();
            double minD = 10000000000.00;

            Point3d p = lineA.PointAt(0.0);
            Point3d q = lineA.PointAt(1.0);

            for (int i=0; i<streets.Count; i++)
            {
                Line lineB = streets[i].Line;
                Point3d r = lineB.PointAt(0.0);
                Point3d s = lineB.PointAt(1.0);
                double a = 0.0;
                double b = 0.0;
                var t = Rhino.Geometry.Intersect.Intersection.LineLine(lineA, lineB, out a, out b);
                if (t == true)
                {
                    Point3d p2 = lineA.PointAt(a);
                    Point3d q2 = lineB.PointAt(b);
                    double pp2 = p.DistanceTo(p2);
                    double qp2 = q.DistanceTo(p2);
                    double pq = p.DistanceTo(q);
                    double rq2 = r.DistanceTo(q2);
                    double sq2 = s.DistanceTo(q2);
                    double rs = r.DistanceTo(s);
                    if ((Math.Abs(pp2 + qp2 - pq) < 1.1) && (Math.Abs(rq2 + sq2 - rs) < 1.1))
                    {
                        if (pp2 < minD)
                        {
                            minD = pp2;
                            intxPt = p2;
                        }
                    }
                }
            }
            return intxPt;
        }

        public List<SiteObj> GetSiteObjList() { return SITEOBJLI; }
        public List<Curve> GetSites() { return SITECRVLI; }
        public List<LineCurve> GetStreets() { return FULLSTREETLI; }
        public List<Line> GetRays() { return RAYLI; }

    } // end class
} // end namespace 


