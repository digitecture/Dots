using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;


namespace DotsProj
{
    class PolyCurveSolver
    {

        private List<Point3d> outerPtLi;
        private List<Point3d> innerPtLi;
        private int numDiv;
        private int numTowers;
        private double flrHt;
        private double offset_inp;
        private double baseFsr;
        private double towerFsr;
        private double SITE_AR;
        private double BaseMassHt;
        private double minTowerAr=double.NaN;

        public List<Point3d> globalPtCrvLi {get; set;}
        public List<Curve> globalTowerCrvLi { get; set; }
        public List<Curve> globalBaseCrvLi { get; set; }
        public List<Brep> globalBrepLi { get; set; }

        Random rnd = new Random();

        public PolyCurveSolver() { }

        public PolyCurveSolver(List<Point3d> outerPtLi_, List<Point3d> innerPtLi_,
            int numDiv_, int numTowers_, double flrHt_, double offset_inp_, double baseFsr_, double towerFsr_,
            double site_ar_, double min_tower_ar_)
        {

            this.outerPtLi = outerPtLi_;
            this.innerPtLi = innerPtLi_;
            this.numDiv = numDiv_;
            this.numTowers = numTowers_;
            this.flrHt = flrHt_;
            this.offset_inp = offset_inp_;
            this.baseFsr = baseFsr_;
            this.towerFsr = towerFsr_;
            this.SITE_AR = site_ar_;
            this.minTowerAr = min_tower_ar_;
            globalBrepLi = new List<Brep>();

            this.BaseMassHt = 0.0;
        }

        public void genBaseMass()
        {
            PolylineCurve outerCrv = new PolylineCurve(outerPtLi);
            double outerAr = AreaMassProperties.Compute(outerCrv).Area;
            PolylineCurve innerCrv = new PolylineCurve(innerPtLi);
            double innerAr = AreaMassProperties.Compute(innerCrv).Area;
            double diffAr = outerAr - innerAr;
            int numBaseFlrs = (int)(SITE_AR * baseFsr / diffAr) + 1;
            double baseHt = numBaseFlrs * flrHt;
            Extrusion outerExtr = Extrusion.Create(outerCrv, baseHt, true);
            Extrusion innerExtr = Extrusion.Create(innerCrv, baseHt, true);

            Brep[] outerBrep = { outerExtr.ToBrep() };
            Brep[] innerBrep = { innerExtr.ToBrep() };
            Brep[] diffBrep = Brep.CreateBooleanDifference(outerBrep, innerBrep, Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            for(int i=0; i<diffBrep.Length; i++)
            {
                globalBrepLi.Add(diffBrep[i]);
            }

            globalBaseCrvLi = new List<Curve>();
            double spineHt = 0.0;
            for(int i=0; i<numBaseFlrs; i++)
            {
                Curve outerCrvDup = outerCrv.DuplicateCurve();
                Rhino.Geometry.Transform xform = Rhino.Geometry.Transform.Translation(0,0,spineHt);
                outerCrvDup.Transform(xform);
                Curve innerCrvDup = innerCrv.DuplicateCurve();
                innerCrvDup.Transform(xform);
                globalBaseCrvLi.Add(innerCrvDup);
                globalBaseCrvLi.Add(outerCrvDup);
                spineHt += flrHt;
            }
            BaseMassHt = baseHt;
        }

        public List<Brep> GetFinalBreps()
        {
            return globalBrepLi;
        }

        public void Compute()
        {
            genBaseMass();
            globalPtCrvLi = new List<Point3d>();
            List<PolylineCurve> polyLi = new List<PolylineCurve>(); // base of towers: poly
            for (int i = 0; i < outerPtLi.Count - 1; i++)
            {
                Point3d p = outerPtLi[i];
                Point3d q = outerPtLi[i + 1];
                Point3d a = innerPtLi[i];
                Point3d b = innerPtLi[i + 1];
                double t = (double)(1.00 / numDiv);
                List<Point3d> inner_subLi = new List<Point3d>();
                List<Point3d> outer_subLi = new List<Point3d>();
                for (double j = 0.0; j < 1.0; j += t)
                {
                    double x = a.X + (b.X - a.X) * j;
                    double y = a.Y + (b.Y - a.Y) * j;
                    Point3d A = new Point3d(x, y, 0); //a+j*(b-a)
                    // globalPtCrvLi.Add(A);
                    // inner_subLi.Add(A);
                    Point3d R=ProjPtLine(p, q, A);//normal from interp _ab to pq
                    if(PtInSeg(p,q,R) == true)
                    {
                        globalPtCrvLi.Add(A);
                        inner_subLi.Add(A);
                        globalPtCrvLi.Add(R);
                        outer_subLi.Add(R);
                    }
                    else
                    {
                        break;
                    }
                }
                for (int j = 0; j < outer_subLi.Count - 1; j++)
                {
                    try
                    {
                        Point3d A = inner_subLi[j];
                        Point3d B = inner_subLi[j + 1];
                        Point3d P = outer_subLi[j];
                        Point3d Q = outer_subLi[j + 1];
                        List<Point3d> pts = new List<Point3d> { A, B, Q, P, A };
                        PolylineCurve poly = new PolylineCurve(pts);
                        double ar2 = AreaMassProperties.Compute(poly).Area;
                        if (ar2 > minTowerAr)
                        {
                            polyLi.Add(poly);
                        }
                    }
                    catch (Exception)
                    {

                    }
                    
                }
            }
            globalTowerCrvLi = new List<Curve>();
            int numSel = numTowers;
            List<PolylineCurve> fPolyLi = new List<PolylineCurve>();
            double cumuArPoly = 0.0;
            for (int i=0; i<numSel; i++)
            {
                try
                {
                    int idx = rnd.Next(polyLi.Count);
                    fPolyLi.Add(polyLi[idx]);
                    cumuArPoly += AreaMassProperties.Compute(polyLi[idx]).Area;
                }
                catch (Exception) { }
                
            }

            int numFlrs = (int)(SITE_AR * towerFsr / cumuArPoly) + 1;
            double towerHt = numFlrs * flrHt;
            for (int i = 0; i < fPolyLi.Count; i++)
            {
                try
                {
                    PolylineCurve poly = fPolyLi[i];
                    Extrusion extr = Rhino.Geometry.Extrusion.Create(poly, towerHt, true);
                    var B = extr.GetBoundingBox(true);
                    if (B.Max.Z <= 0)
                    {
                        extr = Rhino.Geometry.Extrusion.Create(poly, -towerHt, true);
                    }
                    Brep brep = extr.ToBrep();
                    Rhino.Geometry.Transform xform2 = Rhino.Geometry.Transform.Translation(0, 0, BaseMassHt);
                    brep.Transform(xform2);
                    globalBrepLi.Add(brep);
                }
                catch (Exception) { }
                
            }

            // Tower POLY FLOOR COPIES
            // spineHt initialized above & updated from base curve copies
            
            double spineHt = BaseMassHt;
            if (numFlrs > 0)
            {
                for (int i = 0; i < numFlrs; i++)
                {
                    for (int j = 0; j < fPolyLi.Count; j++)
                    {
                        Curve crv = ((Curve)fPolyLi[j]).DuplicateCurve();
                        Rhino.Geometry.Transform xform3 = Rhino.Geometry.Transform.Translation(0, 0, spineHt);
                        crv.Transform(xform3);
                        globalTowerCrvLi.Add(crv);
                    }
                    spineHt += flrHt;
                }
            }            
        }

        public Point3d ProjPtLine(Point3d p, Point3d q, Point3d a)
        {
            double ux = p.X - q.X;
            double uy = p.Y - q.Y;
            double norm = ux * ux + uy * uy;
            double vx = a.X - q.X;
            double vy = a.Y - q.Y;
            double dp = (ux * vx + uy * vy) / norm;
            double fx = q.X + ux * dp;
            double fy = q.Y + uy * dp;
            Point3d P = new Point3d(fx, fy, 0);
            return P;
        }

        public bool PtInSeg(Point3d p, Point3d q, Point3d a)
        {
            double pq = p.DistanceTo(q);
            double ap = a.DistanceTo(p);
            double aq = a.DistanceTo(q);
            if(ap + aq > pq)
            {
                return false;
            }
            return true;
        }
    }
}
 