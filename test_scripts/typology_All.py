import rhinoscriptsyntax as rs

def GenExtrBlock(site_crv,setback,flr_depth,bay_gap,fsr):
    setback_crv=rs.OffsetCurve(site_crv, rs.CurveAreaCentroid(site_crv)[0],setback)
    got_ar=rs.CurveArea(setback_crv)[0]
    req_ar=rs.CurveArea(site_crv)[0]*fsr
    ht=req_ar/got_ar
    l=rs.AddLine([0,0,0],[0,0,ht])
    pl_srf=rs.AddPlanarSrf(setback_crv)
    ext_srf=rs.ExtrudeSurface(pl_srf,l) #
    rs.DeleteObject(l)
    rs.DeleteObject(pl_srf)

def GenStagerredBlock(site_crv,setback,stepbacks,bay_gap,fsr_li,flr_depths):
    bldg_srf_li=[]
    setback_crv=rs.OffsetCurve(site_crv, rs.CurveAreaCentroid(site_crv)[0],setback)
    ht=fsr_li[0]*rs.CurveArea(site_crv)[0] / rs.CurveArea(setback_crv)[0]
    pl_srf0=rs.AddPlanarSrf(setback_crv)
    l=rs.AddLine([0,0,0],[0,0,ht])
    ext_srf=rs.ExtrudeSurface(pl_srf0,l)
    rs.DeleteObjects([l,pl_srf0])
    k=1
    for depth in stepbacks:
        stepback_crv=rs.OffsetCurve(setback_crv,rs.CurveAreaCentroid(site_crv)[0],depth)
        ht2=rs.CurveArea(site_crv)[0]*fsr_li[k] / rs.CurveArea(stepback_crv)[0]
        l=rs.AddLine([0,0,0],[0,0,ht2])
        pl_srf=rs.AddPlanarSrf(stepback_crv)
        ext_srf=rs.ExtrudeSurface(pl_srf,l)
        rs.MoveObject(ext_srf, [0,0,ht])
        bldg_srf_li.append(ext_srf)
        rs.DeleteObject(l)
        rs.DeleteObject(pl_srf)
        ht+=ht2
        k+=1

def GenCourtyardBlock(site_crv,setback,flr_depth,bay_gap,fsr):
    setback_crv=rs.OffsetCurve(site_crv, rs.CurveAreaCentroid(site_crv)[0],setback)
    inner_crv=rs.OffsetCurve(setback_crv,rs.CurveAreaCentroid(site_crv)[0], flr_depth)
    got_ar=rs.CurveArea(setback_crv)[0]-rs.CurveArea(inner_crv)[0]
    req_ar=rs.CurveArea(site_crv)[0]*fsr
    ht=req_ar/got_ar
    l=rs.AddLine([0,0,0],[0,0,ht])
    pl_srf=rs.AddPlanarSrf([setback_crv,inner_crv])
    ext_srf=rs.ExtrudeSurface(pl_srf,l) #
    rs.DeleteObject(l)
    rs.DeleteObject(pl_srf)

def GenStaggeredCourtyardBlock(site_crv,setback,stepbacks,bay_gap,fsr_li,flr_depth):
    bldg_srf_li=[]
    outer_setback_crv=rs.OffsetCurve(site_crv,rs.CurveAreaCentroid(site_crv)[0],setback)
    inner_floor_crv=rs.OffsetCurve(site_crv,rs.CurveAreaCentroid(site_crv)[0],setback+flr_depth)
    req_ht= (rs.CurveArea(site_crv)[0]*fsr_li[0]) / (rs.CurveArea(outer_setback_crv)[0] - rs.CurveArea(inner_floor_crv)[0])
    l=rs.AddLine([0,0,0],[0,0,req_ht])
    srf=rs.AddPlanarSrf([outer_setback_crv,inner_floor_crv])
    srf2=rs.ExtrudeSurface(srf,l)
    rs.DeleteObject(l)
    prev_ht=req_ht
    k=1
    for depth in stepbacks:
        req_ar=rs.CurveArea(site_crv)[0]*fsr_li[k]
        itr_stepback_crv=rs.OffsetCurve(site_crv,rs.CurveAreaCentroid(site_crv)[0],setback+depth)
        got_ar=rs.CurveArea(itr_stepback_crv)[0]-rs.CurveArea(inner_floor_crv)[0]
        ht=req_ar/got_ar
        l=rs.AddLine([0,0,0],[0,0,ht])
        srf=rs.AddPlanarSrf([itr_stepback_crv,inner_floor_crv])
        srf2=rs.ExtrudeSurface(srf,l)
        rs.MoveObject(srf2,[0,0,prev_ht])
        rs.DeleteObject(l)
        rs.DeleteObject(srf)
        bldg_srf_li.append(srf2) #
        prev_ht+=ht
        k+=1



SITE_CRV=rs.GetObject("pick")
SETBACK=10.0
STEPBACKS=[2.0,5.0,7.0]
FLOOR_DEPTH=26.0
BAY_GAP=10.0
FSR=2.75
STAGERRED_FSR=[0.15,1.15,1.75,2.5]

#GenCourtyardBlock(SITE_CRV,SETBACK,FLOOR_DEPTH,BAY_GAP,FSR)
#GenExtrBlock(SITE_CRV,SETBACK,FLOOR_DEPTH,BAY_GAP,FSR)
#GenStaggeredCourtyardBlock(SITE_CRV,SETBACK,STEPBACKS,BAY_GAP,STAGERRED_FSR,FLOOR_DEPTH)
GenStagerredBlock(SITE_CRV,SETBACK,STEPBACKS,BAY_GAP,STAGERRED_FSR,FLOOR_DEPTH)

rs.Command("_SetDisplayMode _Shaded")
