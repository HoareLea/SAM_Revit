using Autodesk.Revit.DB;
using SAM.Geometry.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObject ToRevit(this Panel panel, Document document, Core.Revit.ConvertSettings convertSettings)
        {
            Geometry.Spatial.Face3D face3D = panel.GetFace3D();
            if (face3D == null)
                return null;

            PanelType panelType = panel.PanelType;

            HostObjAttributes hostObjAttributes = panel.Construction.ToRevit(document, panelType, convertSettings);
            if (hostObjAttributes == null)
                hostObjAttributes = Analytical.Query.Construction(panelType).ToRevit(document, panelType, convertSettings); //Default Construction

            HostObject result = null;
            BuiltInParameter[] builtInParameters = null;
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

                Wall wall = Wall.Create(document, curveList, hostObjAttributes.Id, level.Id, false, panel.Normal.ToRevit(false));

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
                if (Math.Abs(lowElevation - levelElevation) > Core.Tolerance.MacroDistance)
                {
                    parameter = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                    if (parameter != null)
                        parameter.Set(UnitUtils.ConvertToInternalUnits(lowElevation - levelElevation, DisplayUnitType.DUT_METERS));
                }

                builtInParameters = new BuiltInParameter[] { BuiltInParameter.WALL_BASE_CONSTRAINT, BuiltInParameter.WALL_BASE_OFFSET, BuiltInParameter.WALL_HEIGHT_TYPE, BuiltInParameter.WALL_USER_HEIGHT_PARAM, BuiltInParameter.WALL_KEY_REF_PARAM };
                result = wall;
            }
            else if (hostObjAttributes is FloorType)
            {
                Geometry.Spatial.IClosedPlanar3D closedPlanar3D_External = face3D.GetExternalEdge();
                if (!(closedPlanar3D_External is Geometry.Spatial.ICurvable3D))
                    return null;

                double elevation = panel.LowElevation();
                Level level = document.HighLevel(elevation);

                Geometry.Spatial.Plane plane = new Geometry.Spatial.Plane(new Geometry.Spatial.Point3D(0, 0, elevation), Geometry.Spatial.Vector3D.WorldZ);

                CurveArray curveArray_Sloped = new CurveArray();
                CurveArray curveArray_Plane = new CurveArray();

                Geometry.Spatial.IClosedPlanar3D closedPlanar3D = face3D.GetExternalEdge();
                if (!(closedPlanar3D is Geometry.Spatial.ICurvable3D))
                    return null;

                List<Geometry.Spatial.ICurve3D> curve3Ds = Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D).GetCurves());
                if (curve3Ds == null || curve3Ds.Count == 0)
                    return null;

                foreach (Geometry.Spatial.ICurve3D curve3D in curve3Ds)
                {
                    curveArray_Sloped.Append(curve3D.ToRevit_Line());

                    Geometry.Spatial.ICurve3D curve3D_Temp = plane.Project(curve3D);
                    if (curve3D_Temp == null)
                        continue;

                    curveArray_Plane.Append(curve3D_Temp.ToRevit_Line());
                }

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
                            curveArray_Plane = new CurveArray();
                            foreach (Geometry.Spatial.ICurve3D curve3D in Geometry.Spatial.Query.Explode(((Geometry.Spatial.ICurvable3D)closedPlanar3D_Internal).GetCurves()))
                            {
                                curveArray_Sloped.Append(curve3D.ToRevit_Line());

                                Geometry.Spatial.ICurve3D curve3D_Temp = plane.Project(curve3D);
                                if (curve3D_Temp == null)
                                    continue;

                                curveArray_Plane.Append(curve3D_Temp.ToRevit_Line());
                            }

                            Opening opening = document.Create.NewOpening(floor, curveArray_Plane, true);
                        }
                    }
                }

                if (floor != null)
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

                builtInParameters = new BuiltInParameter[] { BuiltInParameter.LEVEL_PARAM };
                result = floor;
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

                builtInParameters = new BuiltInParameter[] { BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM, BuiltInParameter.ROOF_BASE_LEVEL_PARAM, BuiltInParameter.ROOF_UPTO_LEVEL_PARAM };
                result = roofBase;
            }

            if (result == null)
                return null;

            List<Aperture> apertures = panel.Apertures;
            if (apertures != null)
            {
                Geometry.Spatial.Plane plane_Panel = panel.PlanarBoundary3D?.Plane;

                foreach (Aperture aperture in apertures)
                {
                    Geometry.Spatial.Plane plane_Aperture = aperture?.PlanarBoundary3D?.Plane;
                    if (plane_Aperture == null)
                        continue;

                    bool flipHand = !plane_Panel.AxisX.SameHalf(plane_Aperture.AxisX);
                    bool flipFacing = !plane_Panel.Normal.SameHalf(plane_Aperture.Normal);

                    FamilyInstance failyInstance_Aperture = aperture.ToRevit(document, result, convertSettings);
                }
            }

            if (convertSettings.ConvertParameters)
            {
                Core.Revit.Modify.Values(panel, result, builtInParameters);
                Core.Revit.Modify.Values(ActiveSetting.Setting, panel, result);

                Core.Revit.Modify.Json(result, panel.ToJObject()?.ToString());
            }
            //TODO: Implement proper log
            //System.IO.File.AppendAllText(@"C:\Users\DengusiakM\Desktop\SAM\2020-04-16 floorbug\LOG.txt", string.Format("{0}\t{1}\t{2}\n", DateTime.Now.ToString(), panel.Guid, panel.Name));

            return result;
        }
    }
}