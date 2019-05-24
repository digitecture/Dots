import rhinoscriptsyntax as rs
import random

class BSP:
    def __init__(self, crv):
        rs.ClearCommandHistory()
        self.SITECRV=crv
        self.counter=0
        self.start()
    
    def start(self):
        self.crv=rs.CopyObject(self.SITECRV,[0,0,0])
        bb=rs.BoundingBox(self.crv)
        self.BBpts=[bb[0],bb[1],bb[2],bb[3],bb[0]]
        self.FPOLY=[]
    
    def getIntxCrv(self, poly):
        intxCrv=rs.CurveBooleanIntersection(self.SITECRV,poly)
        if(len(intxCrv)>0):
            maxCrv=intxCrv[0]
            maxAr=0.0
            for i in intxCrv: 
                ar=rs.CurveArea(i)[0]
                if(ar>maxAr):
                    maxAr=ar
                    maxCrv=i
            crvpts=rs.CurvePoints(maxCrv)
            for i in intxCrv: 
                try:
                    rs.DeleteObject(i)
                except:
                    print("#1. curve not deleted")
            return crvpts
        else:
            for i in intxCrv:
                try:
                    rs.DeleteObject(i)
                except:
                    print("#2. curve not deleted")
            return
    
    def recSplit(self, iniPts, counter=0):
        if(iniPts):
            p=iniPts
            vertical=False
            if(rs.Distance(p[0],p[1])>rs.Distance(p[0],p[3])):
                vertical=True
            if(vertical==True):
                p0,p1=self.verSplit(iniPts)
            else:
                p0,p1=self.horSplit(iniPts)
            poly0=rs.AddPolyline(p0)
            poly1=rs.AddPolyline(p1)
            counter+=1
            if(counter<3):
                pts0=self.getIntxCrv(poly0)
                pts1=self.getIntxCrv(poly1)
                rs.DeleteObjects([poly0,poly1])
                self.recSplit(pts0,counter)
                self.recSplit(pts1,counter)
            else:
                intxCrv0=rs.CurveBooleanIntersection(self.SITECRV,poly0)
                intxCrv1=rs.CurveBooleanIntersection(self.SITECRV,poly1)
                for i in intxCrv0:
                    self.FPOLY.append(i)
                for i in intxCrv1:
                    self.FPOLY.append(i)            
                rs.DeleteObjects([poly0,poly1])
            rs.DeleteObject(self.crv)
    
    def genRotBBox(self, inpPts):
        b=rs.BoundingBox(inpPts)
        poly1=rs.AddPolyline([b[0],b[1],b[2],b[3],b[0]])
        pts=rs.CurvePoints(self.SITECRV)
        c=[(b[0][0]+b[2][0])/2,(b[0][1]+b[2][1])/2,0]
        maxR=0.0
        for i in pts:
            d=rs.Distance(c,i)
            if(d>maxR):
                maxR=d
        cir=rs.AddCircle(c,maxR)
        b2=rs.BoundingBox(cir)
        poly=rs.AddPolyline([b2[0],b2[1],b2[2],b2[3],b2[0]])
        rs.RotateObject(poly, c, ANGLE)
        poly2=rs.CurvePoints(poly)
        rs.DeleteObject(poly1)
        rs.DeleteObject(poly)
        rs.DeleteObject(cir)
        return poly2
    
    def verSplit(self, inpPts):
        t=random.random()*0.5 + 0.2
        p=self.genRotBBox(inpPts)
        a=p[0]
        b=p[1]
        c=p[2]
        d=p[3]
        e=[p[0][0]+(p[1][0]-p[0][0])*t, p[0][1]+(p[1][1]-p[0][1])*t, 0]
        f=[p[3][0]+(p[2][0]-p[3][0])*t, p[3][1]+(p[2][1]-p[3][1])*t, 0]
        pts0=[a,e,f,d,a]
        pts1=[e,b,c,f,e]
        return pts0,pts1
    
    def horSplit(self, inpPts):
        t=random.random()*0.5 + 0.2
        pts=self.genRotBBox(inpPts)
        a=pts[0]
        b=pts[1]
        c=pts[2]
        d=pts[3]
        e=[a[0]+(d[0]-a[0])*t, a[1]+(d[1]-a[1])*t, 0]
        f=[b[0]+(c[0]-b[0])*t, b[1]+(c[1]-b[1])*t, 0]
        pts0=[a,b,f,e,a]
        pts1=[e,f,c,d,e]
        return pts0,pts1
    
    def postProcess(self):
        REDO=False
        ar=0.0
        for i in self.FPOLY:
            try:
                ar+=rs.CurveArea(i)[0]
            except:
                pass
        mean_ar=ar/len(self.FPOLY)
        min_ar_per=0.2*mean_ar
        j=0
        for i in self.FPOLY:
            pts=rs.CurvePoints(i)
            if(rs.CurveArea(i)[0]<min_ar_per):
                REDO=True
                break
            p=rs.BoundingBox(pts)
            max_poly=rs.AddPolyline([p[0],p[1],p[2],p[3],p[0]])
            max_poly_ar=rs.CurveArea(max_poly)[0]
            actual_poly_ar=rs.CurveArea(i)[0]
            if((max_poly_ar/actual_poly_ar)>2):
                REDO=True
                rs.DeleteObject(max_poly)
                break
            else:
                rs.DeleteObject(max_poly)
                
            ab=int(rs.Distance(p[0],p[1]))
            ad=int(rs.Distance(p[0],p[3]))
                
            if((ab>ad) and (ab/ad)>5):
                REDO=True
                break
            elif((ab<ad) and (ab/ad)<0.2):
                REDO=True
                break
            j+=1
        if(REDO==True and self.counter<MAX_REC):
            self.counter+=1
            print("Redo %s" %(self.counter))
            rs.DeleteObjects(self.FPOLY)
            self.start()
            self.recSplit(bsp.BBpts, 0)
            self.postProcess()
        else:
            j=0
            for i in self.FPOLY:
                c=rs.CurveAreaCentroid(i)[0]
                rs.AddTextDot(str(j), c)
                j+=1


MIN=50
ANGLE=30
MAX_REC=50
SITE=rs.GetObject("Pick")
rs.EnableRedraw(False)
bsp =BSP(SITE)
bsp.recSplit(bsp.BBpts, 0)
bsp.postProcess()
rs.EnableRedraw(True)    