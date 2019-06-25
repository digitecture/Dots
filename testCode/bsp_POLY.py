import rhinoscriptsyntax as rs
import random

def genHalfPlanes(site_crv, int_crv):
    B=rs.BoundingBox(site_crv)
    polyBB=rs.AddPolyline([B[0],B[1],B[2],B[3],B[0]])
    diagB=2*rs.Distance(B[0],B[2])
    rays=[]
    int_pts=rs.CurvePoints(int_crv)
    bsp_tree=[]
    bsp_tree.append(site_crv)
    for i in range(len(int_pts)-1):
        a=int_pts[i]
        b=int_pts[i+1]
        m=[(a[0]+b[0])/2,(a[1]+b[1])/2,0]
        p=[m[0]+(a[0]-m[0])*diagB/(rs.Distance(a,m)), m[1]+(a[1]-m[1])*diagB/(rs.Distance(a,m)), 0]
        q=[m[0]+(b[0]-m[0])*diagB/(rs.Distance(b,m)), m[1]+(b[1]-m[1])*diagB/(rs.Distance(b,m)), 0]
        u=[q[0]-p[0],q[1]-p[1],0]
        v=[-u[1],u[0],0]
        r=[p[0]+v[0],p[1]+v[1],0]
        s=[q[0]+v[0],q[1]+v[1],0]
        w=[u[1],-u[0],0]
        R=[p[0]+w[0],p[1]+w[1],0]
        S=[q[0]+w[0],q[1]+w[1],0]
        polyA=rs.AddPolyline([p,q,s,r,p])
        polyB=rs.AddPolyline([p,q,S,R,p])
        
        used_crv=[]
        new_crv=[]
        for bsp_crv in bsp_tree:
            sum=0
            try:
                crvA=rs.CurveBooleanIntersection(bsp_crv,polyA)
                new_crv.append(crvA)
                sum+=1
            except:pass
            try:
                crvB=rs.CurveBooleanIntersection(bsp_crv,polyB)
                new_crv.append(crvB)
                sum+=1
            except: pass
            if(sum>0):
                used_crv.append(bsp_crv)
        
        for crv in new_crv:
            bsp_tree.append(crv)
        for crv in used_crv:
            bsp_tree.remove(crv)
            rs.DeleteObject(crv)
            
        rs.DeleteObject(polyA)
        rs.DeleteObject(polyB)
        
    rs.DeleteObject(polyBB)
    return bsp_tree

def removePoly(bsp_tree, int_crv):
    del_crv=[]
    used_crv=[]
    for crv in bsp_tree:
        try:
            cen=rs.CurveAreaCentroid(crv)[0]
            if(rs.PointInPlanarClosedCurve(cen,int_crv)==1):
                del_crv.append(crv)
        except:
            print("error")
    for crv in del_crv:
        bsp_tree.remove(crv)
        rs.DeleteObject(crv)
    return bsp_tree


SITE_CRV=rs.GetObject("Site")
INT_CRV=rs.GetObject("Int poly")

rs.EnableRedraw(False)
Bsp_Tree=genHalfPlanes(SITE_CRV, INT_CRV)
BSP_TREE=removePoly(Bsp_Tree, INT_CRV)
rs.EnableRedraw(True)

# c# code 
"""
# driver code
List<Curve> BSP_TREE = GenHalfPlanes(SITE_CRV, INT_CRV);
A=RemovePoly(BSP_TREE, INT_CRV);
"""

"""
  public List<Curve> GenHalfPlanes(Curve site_crv, Curve int_crv){
    //main bsp-tree
    List<Curve> bsp_tree = new List<Curve>();
    List<Curve> ray_bbox = new List<Curve>();

    //bounding box of site-crv
    var t = site_crv.GetBoundingBox(true);
    Point3d A = t.Min;
    Point3d C = t.Max;
    Point3d B = new Point3d(C.X, A.Y, 0);
    Point3d D = new Point3d(A.X, C.Y, 0);
    Point3d[] pts = {A,B,C,D,A};

    PolylineCurve polyBB = new PolylineCurve(pts);// polylinecurve of site
    double diagB = 2 * A.DistanceTo(C);
    List<Line> rays = new List<Line>();

    //internal-polyline points
    Polyline intPolyPts = new Polyline();
    var t1 = int_crv.TryGetPolyline(out intPolyPts);
    IEnumerator<Point3d> ptRator = intPolyPts.GetEnumerator();
    List<Point3d> int_pts = new List<Point3d>();
    while(ptRator.MoveNext()){
      int_pts.Add(ptRator.Current);
    }

    bsp_tree.Add(site_crv);
    for(int i = 0; i < int_pts.Count - 1; i++){
      Point3d a = int_pts[i];
      Point3d b = int_pts[i + 1];
      Point3d m = new Point3d((a.X + b.X) / 2, (a.Y + b.Y) / 2, 0);
      double normMA = m.DistanceTo(a);
      double normMB = m.DistanceTo(b);

      Point3d p = new Point3d(m.X + (a.X - m.X) * diagB / normMA, m.Y + (a.Y - m.Y) * diagB / normMA, 0);
      Point3d q = new Point3d(m.X + (b.X - m.X) * diagB / normMB, m.Y + (b.Y - m.Y) * diagB / normMB, 0);
      Point3d u = new Point3d(q.X - p.X, q.Y - p.Y, 0);
      Point3d v = new Point3d(-u.Y, u.X, 0);
      Point3d r = new Point3d(p.X + v.X, p.Y + v.Y, 0);
      Point3d s = new Point3d(q.X + v.X, q.Y + v.Y, 0);
      Point3d w = new Point3d(u.Y, -u.X, 0);
      Point3d R = new Point3d(p.X + w.X, p.Y + w.Y, 0);
      Point3d S = new Point3d(q.X + w.X, q.Y + w.Y, 0);

      Point3d[] ptsA = {p,q,s,r,p};
      PolylineCurve polyA = new PolylineCurve(ptsA);
      Point3d[] ptsB = {p,q,S,R,p};
      PolylineCurve polyB = new PolylineCurve(ptsB);

      List<Curve> used_crv = new List<Curve>();
      List<Curve> new_crv = new List<Curve>();
      for(int j = 0; j < bsp_tree.Count; j++){
        int sum = 0;
        try
        {
          Curve[] crvA = Curve.CreateBooleanIntersection(bsp_tree[j], polyA);
          for(int k = 0; k < crvA.Length; k++){
            new_crv.Add(crvA[k]);
            sum++;
          }
        }
        catch(Exception){}
        try
        {
          Curve[] crvB = Curve.CreateBooleanIntersection(bsp_tree[j], polyB);
          for(int k = 0; k < crvB.Length; k++){
            new_crv.Add(crvB[k]);
            sum++;
          }
        }
        catch(Exception){}
        if(sum > 0){ used_crv.Add(bsp_tree[j]); }
      }
      for(int j = 0; j < new_crv.Count; j++){ bsp_tree.Add(new_crv[j]); }
      for(int j = 0; j < used_crv.Count; j++){ bsp_tree.Remove(used_crv[j]);}
    }
    return bsp_tree;
  }

  public List<Curve> RemovePoly(List<Curve> bsp_tree, Curve int_crv){
    List<Curve> del_crv = new List<Curve>();
    List<Curve> used_crv = new List<Curve>();
    for(int i = 0; i < bsp_tree.Count; i++){
      try{
        Point3d cen = AreaMassProperties.Compute(bsp_tree[i]).Centroid;
        var t = bsp_tree[i].Contains(cen);
        if(t.ToString().Equals("Inside")){
          del_crv.Add(bsp_tree[i]);
        }
      }catch(Exception){}

      for(int j = 0; j < del_crv.Count; j++){
        bsp_tree.Remove(del_crv[j]);
      }
    }
    return bsp_tree;
  }



"""
