import rhinoscriptsyntax as rs
import Rhino
import scriptcontext
import random
import math
import operator


def genGeo(site_crv,setback,flr_depth,courtyard_depth,fsr):
    """
    crv_pts=rs.CurvePoints(site_crv)
    li=[]
    for i in range(len(crv_pts)-1):
        p=crv_pts[i]
        q=crv_pts[i+1]
        m=[(p[0]+q[0])/2,(p[1]+q[1])/2,0]
        d=rs.Distance(p,q)
        li.append([p,q,m,d])
    li.sort(key=operator.itemgetter(3))
    L=li[len(li)-1]
    L1=getUnitNormalToSegInCrv(L,site_crv,setback)
    L2=getUnitNormalToSegInCrv(L1,site_crv,flr_depth)
    seg1,seg2=[L1[0],L1[1]],[L2[0],L2[1]]
    d_L2=getMaxNormalDist(L2,site_crv)
    D1=2*(setback+flr_depth)+courtyard_depth # full bay parallel to this bay
    D2=setback+2*flr_depth # perpendicular bay
    """


    crv_pts=rs.DivideCurve(site_crv, 10)
    for i in range(len(crv_pts)):
        if(i<len(crv_pts)-1):
            p=crv_pts[i]
            q=crv_pts[i+1]
        else:
            p=crv_pts[len(crv_pts)-1]
            q=crv_pts[0]
        L=[p,q]
        L1=getUnitNormalToSegInCrv(L,site_crv,setback)
        rs.AddLine(L1[0],L1[1])
        L2=getUnitNormalToSegInCrv(L1,site_crv,flr_depth)
        rs.AddLine(L2[0],L2[1])
        m=L2[2]

def getUnitNormalToSegInCrv(seg, crv, dist):
    a,b=seg[0],seg[1]
    V=getNormalVectorToSeg(seg, crv)
    a_=[a[0]+V[0]*dist, a[1]+V[1]*dist,0]
    b_=[b[0]+V[0]*dist, b[1]+V[1]*dist,0]
    m_=[(a_[0]+b_[0])/2, (a_[1]+b_[1])/2, 0]
    # rs.AddTextDot('a_',a_)
    # rs.AddTextDot('b_',b_)
    # rs.AddPolyline([a_,b_,b,a,a_])
    return [a_, b_, m_, rs.Distance(a_,b_)]

def getNormalVectorToSeg(seg, crv):
    a,b=seg[0],seg[1]
    dist=1.0
    m=[(a[0]+b[0])/2,(a[1]+b[1])/2,0]
    u=[(b[0]-a[0])/rs.Distance(a,b),(b[1]-a[1])/rs.Distance(a,b),0]
    v=[-u[1],u[0],0]
    w=[u[1],-u[0],0]
    R=[m[0]+dist*v[0],m[1]+dist*v[1],0]
    V=v
    if(rs.PointInPlanarClosedCurve(R,crv)==0):
        R=[m[0]+dist*w[0],m[1]+dist*w[1],0]
        V=w
    return V

def getMaxNormalDist(seg,crv):
    p=getMP_seg(seg)
    v=getNormalVectorToSeg(seg,crv)
    sc=10000000
    q=[p[0]+v[0]*sc,p[1]+v[1]*sc,0]
    line=rs.AddLine(p,q)
    intx=rs.CurveCurveIntersection(line,crv)
    r=q
    if(intx and len(intx)>0):
        r=intx[0][1]
    rs.DeleteObject(line)
    if(rs.Distance(p,r)<rs.Distance(p,q)):
        return rs.Distance(p,r)
    else:
        return 0.0

def getMP_pts(a,b):
    return [(a[0]+b[0])/2,(a[1]+b[1])/2,0]

def getMP_seg(L):
    return getMP_pts(L[0],L[1])

SITE_CRV= rs.GetObject("pick crv") #rs.AddCircle([0,0,0],50)
SETBACK=5.0         # 1.
FLR_DEPTH=5.0       # 2.
COURTYARD_DEPTH=5.0 # 3.
FSR=5.0


genGeo(SITE_CRV, SETBACK, FLR_DEPTH, COURTYARD_DEPTH, FSR)
