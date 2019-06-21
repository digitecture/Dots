using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Rhino;
using Rhino.Geometry;

namespace DotsProj
{
    class GenerateMass
    {
        private Curve Site;
        private double SiteArea;
        private double MinFSR;
        private double MaxFSR;
        private double FlrHt;
        private int NumBaseFlrs;
        private int NumMidFlrs;
        private int NumBridgeFlrs;
        private int NumTowerFlrs;
        private List<Curve> InpCrvs;

        public List<Curve> BaseFlrCrvs { get; set; }
        public List<List<Point3d>> BaseCrvPts { get; set; }
        public double BridgeDepth { get; set; }
        public Curve crvA { get; set; }
        public Curve crvB { get; set; }
        public List<Point3d> crvAPts { get; set; }
        public List<Point3d> crvBPts { get; set; }

        //solution curves
        public Curve BridgePoly0 { get; set; }
        public Curve BridgePoly1 { get; set; }
        public List<Point3d> BridgeCrvPts { get; set; }
        public List<Curve> BridgeCrvLi { get; set; }

        public GenerateMass() { }

        public GenerateMass(Curve site, double site_ar_, double min, double max,
            int base_flrs, int mid, int bridge, int tower, List<Curve> inpcrv,
            double flr_ht_, double bridge_depth)
        {
            this.Site = site;
            this.SiteArea = site_ar_;
            this.MinFSR = min;
            this.MaxFSR = max;
            this.NumBaseFlrs = base_flrs;
            this.NumMidFlrs = mid;
            this.NumBridgeFlrs = bridge;
            this.NumTowerFlrs = tower;
            this.InpCrvs = inpcrv;
            this.FlrHt = flr_ht_;
            this.BridgeDepth = bridge_depth;
            this.crvA = inpcrv[0];
            this.crvB = inpcrv[1];
        }

        public void GenBaseCrvFloors()
        {
            BaseFlrCrvs = new List<Curve>();
            BaseCrvPts = new List<List<Point3d>>();
            foreach (Curve crv in InpCrvs)
            {
                BaseCrvPts.Add(GetPtLiFromCrv(crv));
                for (int i = 0; i < NumBaseFlrs; i++)
                {
                    Curve dupCrv = crv.DuplicateCurve();
                    Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0, 0, FlrHt * i);
                    dupCrv.Transform(xform);
                    BaseFlrCrvs.Add(dupCrv);
                }
            }
        }

        public List<Point3d> GetPtLiFromCrv(Curve crv)
        {
            List<Point3d> ptLi = new List<Point3d>();
            Polyline T = new Polyline();
            var t = crv.TryGetPolyline(out T);
            IEnumerator<Point3d> p = T.GetEnumerator();
            while (p.MoveNext())
            {
                ptLi.Add(p.Current);
            }
            return ptLi;
        }

        public List<Curve> GenBridge()
        {           
            crvAPts = GetPtLiFromCrv(crvA);
            crvBPts = GetPtLiFromCrv(crvB);
            List<Seg> segLi = new List<Seg>();
            for(int i=0; i<crvAPts.Count; i++)
            {
                Point3d p = crvAPts[i];
                for (int j = 0; j < crvBPts.Count; j++)
                {
                    Point3d q = crvBPts[j];
                    Seg seg = new Seg(p, q);
                    segLi.Add(seg);
                }
            }
            segLi.Sort(delegate(Seg x, Seg y)
            {
                return x.dist.CompareTo(y.dist);
            });
            List<Line> lineLi = new List<Line>();

            for(int i=0; i<segLi.Count; i++)
            { 
                Line line = new Line(segLi[i].A, segLi[i].B);
                lineLi.Add(line);
            }

            BridgeCrvLi = new List<Curve>();
            BridgeCrvPts = new List<Point3d>();
            Seg s0 = segLi[0];
            Seg s1 = segLi[1];
            Curve BridgePoly0 = GenBridgeCrv(s0, s1);
            Curve BridgePoly1 = GenBridgeCrv(s1, s0);
            BridgeCrvLi.Add(BridgePoly0);
            BridgeCrvLi.Add(BridgePoly1);

            return BridgeCrvLi;
        }

        public Curve GenBridgeCrv(Seg s0, Seg s1)
        {
            Point3d a = s0.A;
            Point3d b = s0.B;
            Point3d c = s1.B;
            Point3d d = s1.A;
            double normAD = a.DistanceTo(d);
            double normBC = b.DistanceTo(c);
            Point3d d1 = new Point3d(d.X + ((a.X - d.X) * BridgeDepth / normAD), d.Y + ((a.Y - d.Y) * BridgeDepth / normAD), 0);
            Point3d c1 = new Point3d(c.X + ((b.X - c.X) * BridgeDepth / normBC), c.Y + ((b.Y - c.Y) * BridgeDepth / normBC), 0);
            List<Point3d> pts = new List<Point3d>{ d, c, c1, d1, d };
            PolylineCurve poly = new PolylineCurve(pts);
            Curve crv = poly;

            BridgeCrvPts.Add(c);
            BridgeCrvPts.Add(d);
            BridgeCrvPts.Add(c1);
            BridgeCrvPts.Add(d1);

            return crv;
        }

        public override string ToString()
        {
            String s = string.Format("number of inp crvs= {0}\nsite_area={1}\nmin_fsr={2}, " +
                "max_fsr={3}\nnum_base_flrs={4}\nnum_mid_flrs={5}\nnum_bridge_flrs={6}\nnum_tower_flrs={7}", 
                InpCrvs.Count, SiteArea, MinFSR, MaxFSR, NumBaseFlrs, NumMidFlrs, NumBridgeFlrs, NumTowerFlrs);
            return s;
        }
    }
}
