﻿using Autodesk.Revit.DB;
using SAM.Geometry.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObject ToRevit(this Panel panel, Document document, Core.Revit.ConvertSettings convertSettings)
        {
            Geometry.Spatial.Face3D face3D = panel?.GetFace3D();
            if (face3D == null)
                return null;

            HostObject result = convertSettings?.GetObject<HostObject>(panel.Guid);
            if (result != null)
                return result;

            PanelType panelType = panel.PanelType;
            Geometry.Spatial.Vector3D normal = panel.Normal;

            HostObjAttributes hostObjAttributes = panel.Construction.ToRevit(document, panelType, normal, convertSettings);
            if (hostObjAttributes == null)
                hostObjAttributes = Analytical.Query.DefaultConstruction(panelType)?.ToRevit(document, panelType, normal, convertSettings); //Default Construction

            BuiltInParameter[] builtInParameters = null;
            if (hostObjAttributes is Autodesk.Revit.DB.WallType)
            {
                double lowElevation = panel.LowElevation();

                Level level = document.LowLevel(lowElevation);

                Autodesk.Revit.DB.Wall wall = ToRevit_Wall(face3D, document, (Autodesk.Revit.DB.WallType)hostObjAttributes, level);
                if (wall == null)
                    return result;

                //List<Curve> curveList = new List<Curve>();
                //foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                //{
                //    if (Geometry.Spatial.Query.Clockwise(closedPlanar3D))
                //        closedPlanar3D.Reverse();

                //    List<Line> lines = closedPlanar3D.ToRevit();
                //    if (lines == null)
                //        continue;

                //    curveList.AddRange(lines);
                //}

                //if (curveList == null || curveList.Count == 0)
                //    return null;

                //double lowElevation = panel.LowElevation();

                //Level level = document.LowLevel(lowElevation);
                //if (level == null)
                //    return null;

                //Wall wall = Wall.Create(document, curveList, hostObjAttributes.Id, level.Id, false, panel.Normal.ToRevit(false));

                Parameter parameter = null;

                parameter = wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                parameter = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                if (parameter != null)
                {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                    double height = UnitUtils.ConvertToInternalUnits((panel.HighElevation() - lowElevation), DisplayUnitType.DUT_METERS);
#else
                    double height = UnitUtils.ConvertToInternalUnits((panel.HighElevation() - lowElevation), UnitTypeId.Meters);
#endif


                    parameter.Set(height);
                }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                double levelElevation = UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);
#else
                double levelElevation = UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters);
#endif

                if (Math.Abs(lowElevation - levelElevation) > Core.Tolerance.MacroDistance)
                {
                    parameter = wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                    if (parameter != null)
                    {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                        parameter.Set(UnitUtils.ConvertToInternalUnits(lowElevation - levelElevation, DisplayUnitType.DUT_METERS));
#else
                        parameter.Set(UnitUtils.ConvertToInternalUnits(lowElevation - levelElevation, UnitTypeId.Meters));
#endif
                    }

                }

                builtInParameters = new BuiltInParameter[] { BuiltInParameter.WALL_BASE_CONSTRAINT, BuiltInParameter.WALL_BASE_OFFSET, BuiltInParameter.WALL_HEIGHT_TYPE, BuiltInParameter.WALL_USER_HEIGHT_PARAM, BuiltInParameter.WALL_KEY_REF_PARAM };
                result = wall;
            }
            else if (hostObjAttributes is Autodesk.Revit.DB.FloorType)
            {
                Geometry.Spatial.IClosedPlanar3D closedPlanar3D_External = face3D.GetExternalEdge3D();
                if (!(closedPlanar3D_External is Geometry.Spatial.ICurvable3D))
                    return null;

                double elevation = panel.LowElevation();
                Level level = document.HighLevel(elevation);

                Geometry.Spatial.Plane plane = new Geometry.Spatial.Plane(new Geometry.Spatial.Point3D(0, 0, elevation), Geometry.Spatial.Vector3D.WorldZ);

                CurveArray curveArray_Sloped = new CurveArray();
                CurveArray curveArray_Plane = new CurveArray();

                Geometry.Spatial.IClosedPlanar3D closedPlanar3D = face3D.GetExternalEdge3D();
                if (!(closedPlanar3D is Geometry.Spatial.ICurvable3D))
                    return null;

                List<Geometry.Spatial.Segment3D> segment3Ds = Geometry.Revit.Query.Segment3Ds(closedPlanar3D);
                if (segment3Ds == null || segment3Ds.Count == 0)
                    return null;

                foreach (Geometry.Spatial.Segment3D segment3D in segment3Ds)
                {
                    curveArray_Sloped.Append(segment3D.ToRevit_Line());

                    Geometry.Spatial.Segment3D segment3D_Temp = Geometry.Spatial.Query.Project(plane, segment3D);
                    if (segment3D_Temp == null)
                        continue;

                    curveArray_Plane.Append(segment3D_Temp.ToRevit_Line());
                }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021
                Autodesk.Revit.DB.Floor floor = document.Create.NewFloor(curveArray_Plane, hostObjAttributes as Autodesk.Revit.DB.FloorType, level, false);
#else
                CurveLoop curveLoop = new CurveLoop();
                foreach (Curve curve in curveArray_Plane)
                {
                    curveLoop.Append(curve);
                }

                Autodesk.Revit.DB.Floor floor = Autodesk.Revit.DB.Floor.Create(document, new CurveLoop[] { curveLoop }, hostObjAttributes.Id, level.Id);
#endif

                if (floor != null)
                {
                    floor.ChangeTypeId(hostObjAttributes.Id);

                    List<Geometry.Spatial.IClosedPlanar3D> closedPlanar3Ds_Internal = face3D.GetInternalEdge3Ds();
                    if (closedPlanar3Ds_Internal != null && closedPlanar3Ds_Internal.Count > 0)
                    {
                        //Requires to be regenerated before inserting openings
                        //https://thebuildingcoder.typepad.com/blog/2013/07/create-a-floor-with-an-opening-or-complex-boundary.html
                        document.Regenerate();

                        foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D_Internal in face3D.GetInternalEdge3Ds())
                        {
                            List<Geometry.Spatial.Segment3D> segment3Ds_Internal = Geometry.Revit.Query.Segment3Ds(closedPlanar3D_Internal);
                            if (segment3Ds_Internal == null || segment3Ds_Internal.Count == 0)
                                continue;

                            curveArray_Plane = new CurveArray();
                            //foreach (Geometry.Spatial.Segment3D segment3D in segment3Ds)
                            foreach (Geometry.Spatial.Segment3D segment3D in segment3Ds_Internal)
                            {
                                curveArray_Sloped.Append(segment3D.ToRevit_Line());

                                Geometry.Spatial.Segment3D segment3D_Temp = Geometry.Spatial.Query.Project(plane, segment3D);
                                if (segment3D_Temp == null)
                                    continue;

                                curveArray_Plane.Append(segment3D_Temp.ToRevit_Line());
                            }

                            Opening opening = document.Create.NewOpening(floor, curveArray_Plane, true);
                        }
                    }
                }

                if (floor != null)
                {
                    document.Regenerate();

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                    SlabShapeEditor slabShapeEditor = floor.SlabShapeEditor;
#else
                    SlabShapeEditor slabShapeEditor = floor.GetSlabShapeEditor();
#endif


                    if (slabShapeEditor != null)
                    {
                        slabShapeEditor.ResetSlabShape();

                        if (!slabShapeEditor.IsEnabled)
                        {
                            slabShapeEditor.Enable();
                        }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                        foreach (Curve curve in curveArray_Sloped)
                        {
                            XYZ xYZ = curve.GetEndPoint(0);
                            slabShapeEditor.DrawPoint(xYZ);
                        }

#else
                        document.Regenerate();

                        List<XYZ> xYZs = new List<XYZ>();
                        foreach (Curve curve in curveArray_Sloped)
                        {
                            xYZs.Add(curve.GetEndPoint(0));
                        }
                        slabShapeEditor.AddPoints(xYZs);
#endif
                    }
                }

                builtInParameters = new BuiltInParameter[] { BuiltInParameter.LEVEL_PARAM };
                result = floor;
            }
            else if (hostObjAttributes is Autodesk.Revit.DB.RoofType)
            {
                CurveArray curveArray = new CurveArray();
                foreach (Geometry.Spatial.IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
                {
                    List<Geometry.Spatial.Segment3D> segment3Ds = Geometry.Revit.Query.Segment3Ds(closedPlanar3D);
                    if (segment3Ds == null || segment3Ds.Count == 0)
                        return null;

                    segment3Ds.ForEach(x => curveArray.Append(x.ToRevit_Line()));
                }

                Level level = document.HighLevel(panel.LowElevation());
                double levelElevation = level.Elevation;

                ModelCurveArray modelCurveArray = new ModelCurveArray();
                RoofBase roofBase = document.Create.NewFootPrintRoof(curveArray, level, hostObjAttributes as Autodesk.Revit.DB.RoofType, out modelCurveArray);

                Parameter parameter = roofBase.get_Parameter(BuiltInParameter.ROOF_UPTO_LEVEL_PARAM);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                SlabShapeEditor slabShapeEditor = roofBase.SlabShapeEditor;
#else
                SlabShapeEditor slabShapeEditor = roofBase.GetSlabShapeEditor();
#endif


                if (slabShapeEditor != null)
                {
                    slabShapeEditor.ResetSlabShape();

                    if (!slabShapeEditor.IsEnabled)
                    {
                        slabShapeEditor.Enable();
                    }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                        foreach (Curve curve in curveArray)
                        {
                            XYZ xYZ = curve.GetEndPoint(0);
                            slabShapeEditor.DrawPoint(xYZ);
                        }

#else
                    document.Regenerate();

                    List<XYZ> xYZs = new List<XYZ>();
                    foreach (Curve curve in curveArray)
                    {
                        xYZs.Add(curve.GetEndPoint(0));
                    }
                    slabShapeEditor.AddPoints(xYZs);
#endif
                }

                builtInParameters = new BuiltInParameter[] { BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM, BuiltInParameter.ROOF_BASE_LEVEL_PARAM, BuiltInParameter.ROOF_UPTO_LEVEL_PARAM };
                result = roofBase;
            }

            if (result == null)
                return null;

            List<Aperture> apertures = panel.Apertures;
            if (apertures != null)
            {
                if (result is Autodesk.Revit.DB.Wall && ((Autodesk.Revit.DB.Wall)result).WallType.Kind == WallKind.Curtain)
                {

                }
                else
                {
                    foreach (Aperture aperture in apertures)
                    {
                        Geometry.Spatial.Plane plane_Aperture = aperture?.PlanarBoundary3D?.Plane;
                        if (plane_Aperture == null)
                            continue;

                        //bool flipHand = !plane_Panel.AxisX.SameHalf(plane_Aperture.AxisX);
                        //bool flipFacing = !plane_Panel.Normal.SameHalf(plane_Aperture.Normal);

                        FamilyInstance failyInstance_Aperture = aperture.ToRevit(document, result, convertSettings);
                    }
                }
            }

            if (convertSettings.ConvertParameters)
            {
                Core.Revit.Modify.SetValues(result, panel, builtInParameters);
                Core.Revit.Modify.SetValues(result, panel, ActiveSetting.Setting);

                Core.Revit.Modify.SetJson(result, panel.ToJObject()?.ToString());
            }
            //TODO: Implement proper log
            //System.IO.File.AppendAllText(@"C:\Users\DengusiakM\Desktop\SAM\2020-04-16 floorbug\LOG.txt", string.Format("{0}\t{1}\t{2}\n", DateTime.Now.ToString(), panel.Guid, panel.Name));

            convertSettings?.Add(panel.Guid, result);

            return result;
        }
    }
}