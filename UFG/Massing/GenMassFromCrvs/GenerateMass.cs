using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Rhino;
using Rhino.Geometry;

namespace GenMassFromCrvs
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

        public Curve crvA { get; set; }
        public Curve crvB { get; set; }
        public List<Point3d> crvAPts { get; set; }
        public List<Point3d> crvBPts{ get; set; }

        public GenerateMass() { }

        public GenerateMass(Curve site, double site_ar_, double min, double max, 
            int base_flrs, int mid, int bridge, int tower, List<Curve> inpcrv, double flr_ht_)
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
            crvA = inpcrv[0];
            crvB = inpcrv[1];
        }

        public void GenBaseCrvFloors()
        {
            BaseFlrCrvs = new List<Curve>();
            BaseCrvPts = new List<List<Point3d>>();
            foreach(Curve crv in InpCrvs)
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

        public List<Line> GenBridge()
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
            
            return lineLi;
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
