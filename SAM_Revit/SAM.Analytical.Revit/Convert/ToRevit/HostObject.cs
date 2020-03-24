using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObject ToRevit(this Document document, Panel panel)
        {
            Geometry.Spatial.Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            PanelType panelType = panel.PanelType;

            HostObjAttributes hostObjAttributes = document.ToRevit(panel.Construction, panelType);
            if (hostObjAttributes == null)
                hostObjAttributes = document.ToRevit(Analytical.Query.Construction(panelType), panelType); //Default Construction

            if (hostObjAttributes is WallType)
            {
                List<Curve> curveList = new List<Curve>();
                foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D in face3D.GetEdges())
                {
                    if (!(closedPlanar3D is Geometry.Spatial.ICurvable3D))
                        continue;

                    curveList.AddRange(Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves()).ConvertAll(x => x.ToRevit_Line()));
                }

                if (curveList == null || curveList.Count == 0)
                    return null;

                double lowElevation = panel.LowElevation();

                Level level = document.LowLevel(lowElevation);
                if (level == null)
                    return null;

                //Flipping recognition

                //Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.Base;

                ////Get Normal from Panel
                //Geometry.Spatial.Vector3D vector3D_1 = plane.Project(panel.PlanarBoundary3D.Normal);
                //vector3D_1 = vector3D_1.Unit;

                //XYZ vectorRevit = vector3D_1.ToRevit().Normalize();

                Geometry.Spatial.Vector3D normal = panel.Normal;

                Wall wall = Wall.Create(document, curveList, hostObjAttributes.Id, level.Id, false, normal.ToRevit(false));
                document.Regenerate();
                if (!normal.AlmostEqual(wall.Orientation.ToSAM_Vector3D(false), Core.Tolerance.MicroDistance))
                    wall.Flip();

                Parameter parameter = null;

                parameter = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                parameter = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                if (parameter != null)
                {
                    double height = UnitUtils.ConvertToInternalUnits((panel.HightElevation() - lowElevation), DisplayUnitType.DUT_METERS);
                    parameter.Set(height);
                }

                double levelElevation = UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);
                if (System.Math.Abs(lowElevation - levelElevation) > Core.Tolerance.MacroDistance)
                {
                    parameter = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                    if (parameter != null)
                        parameter.Set(UnitUtils.ConvertToInternalUnits(lowElevation - levelElevation, DisplayUnitType.DUT_METERS));
                }

                List<Aperture> apertures = panel.Apertures;
                if (apertures != null)
                    apertures.ForEach(x => Convert.ToRevit(document, x, wall));

                return wall;
            }
            else if (hostObjAttributes is FloorType)
            {
                Geometry.Spatial.IClosedPlanar3D closedPlanar3D_External = face3D.GetExternalEdge();
                if (!(closedPlanar3D_External is Geometry.Spatial.ICurvable3D))
                    return null;

                double elevation = panel.LowElevation();
                Level level = document.HighLevel(elevation);

                Geometry.Spatial.Plane plane = new Geometry.Spatial.Plane(new Geometry.Spatial.Point3D(0, 0, elevation), Geometry.Spatial.Vector3D.BaseZ);
                
                CurveArray curveArray_Sloped = new CurveArray();
                CurveArray curveArray_Plane = new CurveArray();
                foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D in face3D.GetEdges())
                {
                    if (!(closedPlanar3D is Geometry.Spatial.ICurvable3D))
                        continue;

                    List<Geometry.Spatial.ICurve3D> curve3Ds = Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves());
                    if (curve3Ds == null || curve3Ds.Count == 0)
                        continue;

                    foreach (Geometry.Spatial.ICurve3D curve3D in curve3Ds)
                    {
                        curveArray_Sloped.Append(curve3D.ToRevit_Line());

                        Geometry.Spatial.ICurve3D curve3D_Temp = plane.Project(curve3D);
                        if (curve3D_Temp == null)
                            continue;

                        curveArray_Plane.Append(curve3D_Temp.ToRevit_Line());
                    }
                }
                //foreach (Line line in Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D_External).GetCurves()).ConvertAll(x => x.ToRevit_Line()))
                //    curveArray_Sloped.Append(line);

                


                Floor floor = document.Create.NewFloor(curveArray_Plane, hostObjAttributes as FloorType, level, false);

                if (floor != null)
                {
                    floor.ChangeTypeId(hostObjAttributes.Id);

                    List<Geometry.Spatial.IClosedPlanar3D> closedPlanar3Ds_Internal = face3D.GetInternalEdges();
                    if (closedPlanar3Ds_Internal != null && closedPlanar3Ds_Internal.Count > 0)
                    {
                        //Requires to be regenerated before inserting openings
                        //https://thebuildingcoder.typepad.com/blog/2013/07/create-a-floor-with-an-opening-or-complex-boundary.html
                        document.Regenerate();

                        foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D_Internal in face3D.GetInternalEdges())
                        {
                            curveArray_Sloped = new CurveArray();
                            foreach (Line line in Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D_Internal).GetCurves()).ConvertAll(x => x.ToRevit_Line()))
                                curveArray_Sloped.Append(line);

                            Opening opening = document.Create.NewOpening(floor, curveArray_Sloped, true);
                        }
                    }

                    List<Aperture> apertures = panel.Apertures;
                    if (apertures != null)
                        apertures.ForEach(x => Convert.ToRevit(document, x, floor));
                }

                if(floor != null)
                {
                    document.Regenerate();

                    SlabShapeEditor slabShapeEditor = floor.SlabShapeEditor;
                    slabShapeEditor.ResetSlabShape();

                    foreach (Curve curve in curveArray_Sloped)
                    {
                        XYZ xYZ = curve.GetEndPoint(0);
                        slabShapeEditor.DrawPoint(xYZ);
                    }
                }

                return floor;

            }
            else if (hostObjAttributes is RoofType)
            {
                CurveArray curveArray = new CurveArray();
                foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D in face3D.GetEdges())
                {
                    if (!(closedPlanar3D is Geometry.Spatial.ICurvable3D))
                        continue;

                    List<Geometry.Spatial.ICurve3D> curve3Ds = Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves());
                    if (curve3Ds == null || curve3Ds.Count == 0)
                        continue;

                    foreach (Geometry.Spatial.ICurve3D curve3D in curve3Ds)
                        curveArray.Append(curve3D.ToRevit_Line());

                }

                Level level = document.HighLevel(panel.LowElevation());
                double levelElevation = level.Elevation;

                ModelCurveArray modelCurveArray = new ModelCurveArray();
                RoofBase roofBase = document.Create.NewFootPrintRoof(curveArray, level, hostObjAttributes as RoofType, out modelCurveArray);

                Parameter parameter = roofBase.get_Parameter(BuiltInParameter.ROOF_UPTO_LEVEL_PARAM);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                SlabShapeEditor slabShapeEditor = roofBase.SlabShapeEditor;
                slabShapeEditor.ResetSlabShape();

                foreach (Curve curve in curveArray)
                {
                    XYZ xYZ = curve.GetEndPoint(0);
                    //if (Math.Abs(xYZ.Z - levelElevation) > Core.Tolerance.MicroDistance)
                        slabShapeEditor.DrawPoint(xYZ);
                }

                List<Aperture> apertures = panel.Apertures;
                if (apertures != null)
                    apertures.ForEach(x => Convert.ToRevit(document, x, roofBase));

                return roofBase;
            }

            return null;
        }
    }
}
