import rhinoscriptsyntax as rs
import random

def genHalfPlanes(crv, int_crv):
    B=rs.BoundingBox(crv)
    polyBB=rs.AddPolyline([B[0],B[1],B[2],B[3],B[0]])
    diagB=rs.Distance(B[0],B[2])
    rays=[]
    sitePts=rs.CurvePoints(int_crv)
    for i in range(len(sitePts)-1):
        a=sitePts[i]
        b=sitePts[i+1]
        m=[(a[0]+b[0])/2,(a[1]+b[1])/2,0]
        a_=[m[0]+(a[0]-m[0])*diagB/(rs.Distance(a,m)), m[1]+(a[1]-m[1])*diagB/(rs.Distance(a,m)), 0]
        b_=[m[0]+(b[0]-m[0])*diagB/(rs.Distance(a,m)), m[1]+(b[1]-m[1])*diagB/(rs.Distance(a,m)), 0]
        line_ma=rs.AddLine(m,a_)
        line_mb=rs.AddLine(m,b_)
        p=rs.CurveCurveIntersection(polyBB,line_ma)
        q=rs.CurveCurveIntersection(polyBB,line_mb)
        p_=p[0][1]
        q_=q[0][1]
        rs.DeleteObject(line_ma)
        rs.DeleteObject(line_mb)
        rays.append([p_,q_])
        #rs.AddLine(p_,q_)
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