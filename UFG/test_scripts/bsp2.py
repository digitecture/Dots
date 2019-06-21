import rhinoscriptsyntax as rs
import random



CRV=rs.GetObject("Pick crv")


pts=[]
pts=rs.CurvePoints(CRV)
b=rs.BoundingBox(pts)
poly1=rs.AddPolyline([b[0],b[1],b[2],b[3],b[0]])
c=[(b[0][0]+b[2][0])/2,(b[0][1]+b[2][1])/2,0]
maxR=0.0
for i in pts:
    d=rs.Distance(c,i)
    if(d>maxR):
        maxR=d
cir=rs.AddCircle(c,maxR)
b2=rs.BoundingBox(cir)

poly=rs.AddPolyline([b2[0],b2[1],b2[2],b2[3],b2[0]])
rs.RotateObject(poly, c, 10) 

rs.DeleteObject(poly1)
