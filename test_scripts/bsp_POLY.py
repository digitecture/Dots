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
                used_crv.append(bsp_crv)
                sum+=1
            except: pass
            if(sum>0):
                new_crv.append(crvB)
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