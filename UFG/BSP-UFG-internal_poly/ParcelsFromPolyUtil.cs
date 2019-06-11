using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace DotsProj
{
    public class ParcelsFromPolyUtil
    {
      
        public ParcelsFromPolyUtil() { }

        public List<Curve> GenHalfPlanes(Curve site_crv, Curve int_crv)
        {
            // main bsp-tree
            List<Curve> bsp_tree = new List<Curve>();
            var t = site_crv.GetBoundingBox(true);
            Point3d A = t.Min;
            Point3d C = t.Max;
            double diagB = 2 * A.DistanceTo(C);

            // internal-polyline points
            Polyline intPolyPts = new Polyline();
            var t1 = int_crv.TryGetPolyline(out intPolyPts);
            IEnumerator<Point3d> ptRator = intPolyPts.GetEnumerator();
            List<Point3d> int_pts = new List<Point3d>();
            while (ptRator.MoveNext())
            {
                int_pts.Add(ptRator.Current);
            }

            bsp_tree.Add(site_crv);
            for(int i=0; i<int_pts.Count-1; i++)
            {
                Point3d a = int_pts[i];
                Point3d b = int_pts[i + 1];
                Point3d m = new Point3d((a.X + b.X) / 2, (a.Y + b.Y) / 2, 0);
                double normA = m.DistanceTo(a);
                double normB = m.DistanceTo(b);

                Point3d p = new Point3d(m.X + (a.X - m.X) * diagB / normA, m.Y + (a.Y - m.Y) * diagB / normA, 0);
                Point3d q = new Point3d(m.X + (b.X - m.X) * diagB / normB, m.Y + (b.Y - m.Y) * diagB / normB, 0);
                Point3d u = new Point3d(q.X - p.X, q.Y - p.Y, 0);
                Point3d v = new Point3d(-u.Y, u.X, 0);
                Point3d r = new Point3d(p.X + v.X, p.Y + v.Y, 0);
                Point3d s = new Point3d(q.X + v.X, q.Y + v.Y, 0);
                Point3d w = new Point3d(u.Y, -u.X, 0);
                Point3d R = new Point3d(p.X + w.X, p.Y + w.Y, 0);
                Point3d S = new Point3d(q.X + w.X, q.Y + w.Y, 0);

                Point3d[] ptsA = { p, q, s, r, p };
                PolylineCurve polyA = new PolylineCurve(ptsA);
                Point3d[] ptsB = { p, q, S, R, p };
                PolylineCurve polyB = new PolylineCurve(ptsB);

                List<Curve> used_crv = new List<Curve>();
                List<Curve> new_crv = new List<Curve>();
                for(int j=0; j<bsp_tree.Count; j++)
                {
                    int sum = 0;
                    try 
                    { 
                        Curve[] crvA = Curve.CreateBooleanIntersection(bsp_tree[j], polyA);  
                        for(int k=0; k<crvA.Length; k++)
                        {
                            new_crv.Add(crvA[k]);
                            sum++;
                        }
                    }
                    catch (Exception) { }
                    try
                    {
                        Curve[] crvB = Curve.CreateBooleanIntersection(bsp_tree[j], polyB);
                        for(int k=0; k<crvB.Length; k++)
                        {
                            new_crv.Add(crvB[k]);
                            sum++;
                        }
                    }
                    catch (Exception) { }

                    if (sum > 0) { used_crv.Add(bsp_tree[j]); }
                }
                for (int j = 0; j < new_crv.Count; j++) { bsp_tree.Add(new_crv[j]); }
                for(int j=0; j<used_crv.Count; j++) { bsp_tree.Remove(used_crv[j]); }

            }
            return bsp_tree;
        }


        public List<Curve> RemovePoly(List<Curve> bsp_tree, Curve int_crv)
        {
            List<Curve> del_crv = new List<Curve>();
            for(int i=0; i<bsp_tree.Count; i++)
            {
                try
                {
                    Point3d cen = AreaMassProperties.Compute(bsp_tree[i]).Centroid;
                    var t=bsp_tree[i].Contains(cen);
                    if (t.ToString().Equals("Inside"))
                    {
                        del_crv.Add(bsp_tree[i]);
                    }
                }
                catch (Exception) { }

                for(int j=0; j<del_crv.Count; j++)
                {
                    bsp_tree.Remove(del_crv[j]);
                }
            }
            return bsp_tree;
        }
    }
}
