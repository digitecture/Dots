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
            crvA=rs.CurveBooleanIntersection(bsp_crv,polyA)
            crvB=rs.CurveBooleanIntersection(bsp_crv,polyB)
            used_crv.append(bsp_crv)
            new_crv.append(crvA)
            new_crv.append(crvB)
        for crv in new_crv:
            bsp_tree.append(crv)
        for crv in used_crv:
            bsp_tree.remove(crv)
            rs.DeleteObject(crv)
        rs.DeleteObject(polyA)
        rs.DeleteObject(polyB)
    rs.DeleteObject(polyBB)
    return rays


#find orientation of q wrt pr
def orientation(p,q,r):
    orientation=0
    t=(q[0]-p[0])
    if(t==0):
        t=0.001
    s=(r[0]-q[0])
    if(s==0):
        s=0.001
    tau=(q[1]-p[1])/t
    mu=(r[1]-q[1])/s
    if(tau<mu):
        orientation =-1
    elif(tau>mu):
        orientation=1
    else:
        orientation =0
    return orientation

def initSplitCurve(crv, rays):
    f_rays=[]
    for ray in rays:
        p_=ray[0]
        q_=ray[1]
        ray2=rs.AddLine(p_,q_)
        intx=rs.CurveCurveIntersection(crv, ray2)
        p=intx[0][1]
        q=intx[1][1]
        f_rays.append([p,q])
        rs.DeleteObject(ray2)
    return f_rays

def splitCrv(site_crv, rays):
    bsp_tree=[]
    bsp_tree.append(site_crv)
    ray=rays[0]
    p=ray[0]
    q=ray[1]
    ray=rs.AddLine(p,q)
    ptsA=[]
    ptsB=[]
    used=[]
    for crv in bsp_tree:
        try:
            intx=rs.CurveCurveIntersection(crv, ray)
            if(intx and len(intx)>0):
                crv_pts=rs.CurvePoints(crv)
                for pt in crv_pts:
                    t=orientation(p,pt,q)
                    if(t==1):
                        ptsA.append(pt)
                    else:
                        ptsB.append(pt)
                used.append(crv)
                ptsA.insert(0,p)
                ptsA.append(q)
                ptsA.append(p)
                polyA=rs.AddPolyline(ptsA)
                bsp_tree.append(polyA)

                
                rs.DeleteObject(ray)
            else:
                continue
        except:
            pass
        for i in used:
            bsp_tree.remove(i)
        
            



SITE_CRV=rs.GetObject("Site")
INT_CRV=rs.GetObject("Int poly")



rs.EnableRedraw(False)

iRAYS=genHalfPlanes(SITE_CRV, INT_CRV)
fRAYS=initSplitCurve(SITE_CRV, iRAYS)

#fRays=rs.GetObject()
#splitCrv(SITE_CRV, fRAYS)

rs.EnableRedraw(True)