﻿using Autodesk.Revit.DB;
using SAM.Core.Revit;
using SAM.Geometry.Planar;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<Panel> ToSAM(this HostObject hostObject, ConvertSettings convertSettings)
        {
            if (hostObject == null || !hostObject.IsValidObject)
                return null;

            if(hostObject is WallSweep)
            {
                return ((WallSweep)hostObject).ToSAM_Panels(convertSettings);
            }

            List<Panel> result = convertSettings?.GetObjects<Panel>(hostObject.Id);
            if (result != null)
                return result;

            ElementId elementId_Type = hostObject.GetTypeId();
            if (elementId_Type == null || elementId_Type == ElementId.InvalidElementId)
                return null;

            Document document = hostObject.Document;
            if(document == null)
            {
                return null;
            }

            result = new List<Panel>();

            PanelType panelType = Query.PanelType(hostObject);
            Construction construction = ((HostObjAttributes)document.GetElement(elementId_Type)).ToSAM(convertSettings);
            if (construction == null)
                construction = Analytical.Query.DefaultConstruction(panelType); //Default Construction

            PanelType panelType_Temp = Query.PanelType(construction);
            if (panelType_Temp != PanelType.Undefined)
                panelType = panelType_Temp;

            List<Face3D> face3Ds = hostObject.Profiles();
            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(new List<ElementFilter>() { new ElementCategoryFilter(BuiltInCategory.OST_Windows), new ElementCategoryFilter(BuiltInCategory.OST_Doors) });
            IEnumerable<ElementId> elementIds = hostObject.GetDependentElements(logicalOrFilter);

            if (hostObject is Wall || hostObject is CurtainSystem)
            {
                List<Autodesk.Revit.DB.Panel> panels = Core.Revit.Query.Panels(hostObject as dynamic);
                if (panels != null && panels.Count > 0)
                {
                    List<ElementId> elementIds_Temp = panels.ConvertAll(x => x.Id);
                    if (elementIds != null && elementIds.Count() > 0)
                        elementIds_Temp.AddRange(elementIds);

                    elementIds = elementIds_Temp;
                }
            }

            List<Aperture> apertures = new List<Aperture>();
            if (elementIds != null && elementIds.Count() > 0)
            {
                foreach (ElementId elementId in elementIds)
                {
                    FamilyInstance familyInstance = document.GetElement(elementId) as FamilyInstance;
                    if (familyInstance == null)
                    {
                        continue;
                    }
                    
                    Aperture aperture = ToSAM_Aperture(familyInstance, convertSettings);
                    if(aperture == null)
                    {
                        continue;
                    }

                    apertures.Add(aperture);
                }
            }


            foreach (Face3D face3D in face3Ds)
            {
                if (face3D == null)
                    continue;

                Panel panel = Analytical.Create.Panel(construction, panelType, face3D.Reduce(0.2));
                panel.UpdateParameterSets(hostObject, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

                foreach(Aperture aperture in apertures)
                {
                    panel.AddAperture(aperture);
                }

                result.Add(panel);
            }

            convertSettings?.Add(hostObject.Id, result);

            //Handling WallSweeps
            //if(hostObject is Wall)
            //{
            //    elementIds = hostObject.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_Cornices));
            //    if (elementIds != null && elementIds.Count() != 0)
            //    {
            //        //Fix it
            //        foreach (ElementId elementId in elementIds)
            //        {
            //            WallSweep wallSweep = document.GetElement(elementId) as WallSweep;
            //            if (wallSweep == null)
            //            {
            //                continue;
            //            }

            //            List<Panel> panels_WallSweep = wallSweep.ToSAM_Panels(convertSettings);
            //            if (panels_WallSweep == null || panels_WallSweep.Count == 0)
            //            {
            //                continue;
            //            }

            //            result.AddRange(panels_WallSweep);
            //        }
            //    }
            //}

            return result;
        }

        public static List<Panel> ToSAM_Panels(this Document document, IEnumerable<ElementId> elementIds, ConvertSettings convertSettings)
        {
            if (document == null)
                return null;
            
            LogicalOrFilter logicalOrFilter = null;

            logicalOrFilter = new LogicalOrFilter((new List<BuiltInCategory> { BuiltInCategory.OST_Walls, BuiltInCategory.OST_Floors, BuiltInCategory.OST_Roofs }).ConvertAll(x => (ElementFilter)(new ElementCategoryFilter(x))));
            FilteredElementCollector filteredElementCollector = null;
            if (elementIds != null && elementIds.Count() > 0)
                filteredElementCollector = new FilteredElementCollector(document, new List<ElementId>(elementIds));
            else
                filteredElementCollector = new FilteredElementCollector(document);

            IEnumerable<HostObject> hostObjects = filteredElementCollector.WherePasses(logicalOrFilter).OfClass(typeof(HostObject)).WhereElementIsNotElementType().Cast<HostObject>();

            if (hostObjects == null || hostObjects.Count() == 0)
                return null;

            List<Panel> result = new List<Panel>();
            foreach (HostObject hostObject in hostObjects)
            {
                List<Panel> panels = hostObject.ToSAM(convertSettings);
                if (panels != null && panels.Count > 0)
                    result.AddRange(panels);
            }

            logicalOrFilter = new LogicalOrFilter((new List<BuiltInCategory> { BuiltInCategory.OST_MEPSpaceSeparationLines, BuiltInCategory.OST_RoomSeparationLines }).ConvertAll(x => (ElementFilter)(new ElementCategoryFilter(x))));
            IEnumerable<ModelCurve> modelCurves = new FilteredElementCollector(document).WherePasses(logicalOrFilter).WhereElementIsNotElementType()?.ToList().FindAll(x => x is ModelCurve).Cast<ModelCurve>();
            if (modelCurves != null && modelCurves.Count() != 0)
            {
                foreach (ModelCurve modelCurve in modelCurves)
                {
                    List<Panel> panels = modelCurve.ToSAM_Panels(convertSettings);
                    if (panels != null && panels.Count > 0)
                        result.AddRange(panels);
                }
            }

            return result;
        }

        public static List<Panel> ToSAM_Panels(this Document document, ConvertSettings convertSettings)
        {
            if (document == null)
                return null;
            
            return ToSAM_Panels(document, null, convertSettings);
        }

        public static List<Panel> ToSAM_Panels(this RevitLinkInstance revitLinkInstance, ConvertSettings convertSettings)
        {
            if (revitLinkInstance == null)
                return null;

            Document document = null;

            try
            {
                document = revitLinkInstance.GetLinkDocument();
            }
            catch
            {
                return null;
            }

            if (document == null)
                return null;

            List<Panel> result = ToSAM_Panels(document, convertSettings);
            if (result == null)
                return result;

            Transform transform = revitLinkInstance.GetTotalTransform();
            if (transform != null && !transform.IsIdentity)
                result = result.ConvertAll(x => Query.Transform(transform, x));

            return result;
        }

        public static List<Panel> ToSAM_Panels(this ModelCurve modelCurve, ConvertSettings convertSettings)
        {
            if (modelCurve == null)
                return null;

            List<Panel> result = convertSettings?.GetObjects<Panel>(modelCurve.Id);
            if (result != null)
                return result;

            List<Segment3D> segment3Ds = Geometry.Revit.Convert.ToSAM_Segment3Ds(modelCurve.GeometryCurve);
            if (segment3Ds == null || segment3Ds.Count == 0)
                return null;

            PanelType panelType = PanelType.WallInternal;
            if(modelCurve.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MEPSpaceSeparationLines || modelCurve.Category.Id.IntegerValue == (int)BuiltInCategory.OST_RoomSeparationLines)
                panelType = PanelType.Air;

            Construction construction = null;
            if (ActiveSetting.Setting.TryGetValue(AnalyticalSettingParameter.DefaultConstructionLibrary, out ConstructionLibrary constructionLibrary))
                construction = constructionLibrary.GetConstructions(panelType).FirstOrDefault();

            Document document = modelCurve.Document;

            result = new List<Panel>();
            foreach(Segment3D segment3D in segment3Ds)
            {
                //double elevation_Min = System.Math.Max(segment3D[0].Z, segment3D[1].Z);
                double elevation_Min = (document.GetElement(modelCurve.LevelId) as Level).Elevation;

                //Level level_Max = Core.Revit.Query.HighLevel(document, UnitUtils.ConvertToInternalUnits(elevation_Min, DisplayUnitType.DUT_METERS));
                Level level_Max = Core.Revit.Query.HighLevel(document, elevation_Min);
                if (level_Max == null)
                {
                    continue;
                }

                double height = UnitUtils.ConvertFromInternalUnits(level_Max.Elevation - elevation_Min, DisplayUnitType.DUT_METERS);
                if(height == 0)
                {
                    continue;
                }

                Vector3D vector3D = new Vector3D(0, 0, height);

                Face3D face3D = new Face3D(new Polygon3D(new Point3D[] { segment3D[0], segment3D[1], segment3D[1].GetMoved(vector3D) as Point3D, segment3D[0].GetMoved(vector3D) as Point3D}));

                Panel panel = Analytical.Create.Panel(construction, panelType, face3D);
                panel.UpdateParameterSets(modelCurve, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap), null, new string[] { "Length" }, true);
                panel.SetValue(RevitPanelParameter.Length, segment3D.GetLength());
                result.Add(panel);
            }

            convertSettings?.Add(modelCurve.Id, result);

            return result;
        }

        /// <summary>
        /// Conversion of WallSweep to panels. WARNING! Method does not handle more complex WallSweep
        /// </summary>
        /// <param name="wallSweep">Revit WallSweep</param>
        /// <param name="convertSettings">SAM Revit ConvertSettings</param>
        /// <returns>SAM Analytical Panels (Panels projected on host (Wall) plane)</returns>
        public static List<Panel> ToSAM_Panels(this WallSweep wallSweep, ConvertSettings convertSettings)
        {
            if(wallSweep == null)
            {
                return null;
            }

            List<Panel> result = convertSettings?.GetObjects<Panel>(wallSweep.Id);
            if (result != null)
            {
                return result;
            }

            RevitInstance3D revitInstance3D = Geometry.Revit.Convert.ToSAM(wallSweep, convertSettings);
            if (revitInstance3D == null)
            {
                return null;
            }

            result = ToSAM_Panels(revitInstance3D);

            if(result != null)
            {
                convertSettings?.Add(wallSweep.Id, result);
            }

            return result;

            //if (wallSweep == null || !wallSweep.IsValidObject)
            //    return null;

            //List<Panel> result = convertSettings?.GetObjects<Panel>(wallSweep.Id);
            //if (result != null)
            //    return result;

            //IEnumerable<ElementId> elementIds = wallSweep.GetHostIds();
            //if (elementIds == null || elementIds.Count() == 0)
            //{
            //    return null;
            //}

            //Document document = wallSweep.Document;
            //if(document == null)
            //{
            //    return null;
            //}

            //HostObject hostObject = null;
            //foreach(ElementId elementId in elementIds)
            //{
            //    hostObject = document.GetElement(elementId) as HostObject;
            //    if(hostObject != null)
            //    {
            //        break;
            //    }
            //}

            //List<Face3D> face3Ds = hostObject?.Profiles();
            //if(face3Ds == null || face3Ds.Count == 0)
            //{
            //    return null;
            //}

            //Geometry.Spatial.Plane plane = face3Ds[0]?.GetPlane();
            //if(plane == null)
            //{
            //    return null;
            //}

            //List<Face3D> face3Ds_WallSweep = Geometry.Revit.Convert.ToSAM_Geometries<Face3D>(wallSweep);
            //if(face3Ds_WallSweep == null || face3Ds_WallSweep.Count == 0)
            //{
            //    return null;
            //}

            //List<Face2D> face2Ds = new List<Face2D>();
            //foreach(Face3D face3D_WallSweep in face3Ds_WallSweep)
            //{
            //    Geometry.Spatial.Plane plane_WallSweep = face3D_WallSweep?.GetPlane();
            //    if (plane_WallSweep == null || plane.Perpendicular(plane_WallSweep))
            //    {
            //        continue;
            //    }

            //    Face3D face3D = plane.Project(face3D_WallSweep);
            //    if(face3D == null || !face3D.IsValid())
            //    {
            //        continue;
            //    }

            //    Face2D face2D = plane.Convert(face3D);
            //    if(face2D == null || !face2D.IsValid())
            //    {
            //        continue;
            //    }

            //    face2Ds.Add(face2D);
            //}

            //face2Ds = face2Ds.Union();
            //if(face2Ds == null || face2Ds.Count == 0)
            //{
            //    return null;
            //}

            //Construction construction = ToSAM_Construction((ElementType)document.GetElement(wallSweep.GetTypeId()), convertSettings);

            //result = new List<Panel>();
            //foreach(Face2D face2D in face2Ds)
            //{
            //    Face3D face3D = plane.Convert(face2D);
            //    if(face3D == null || !face3D.IsValid())
            //    {
            //        continue;
            //    }

            //    Panel panel = Analytical.Create.Panel(construction, PanelType.Wall, face3D);
            //    if(panel == null)
            //    {
            //        continue;
            //    }

            //    result.Add(panel);
            //}

            //convertSettings?.Add(wallSweep.Id, result);

            //return result;
        }

        public static List<Panel> ToSAM_Panels(this RevitInstance3D revitInstance3D)
        {
            if(revitInstance3D == null)
            {
                return null;
            }

            RevitType3D revitType3D = revitInstance3D.SAMType as RevitType3D;
            Construction construction = revitType3D.ToSAM_Construction();

            List<Face3D> face3Ds = new List<Face3D>();
            List<ISAMGeometry3D> sAMGeometry3Ds = revitInstance3D.Geometries;
            foreach(ISAMGeometry3D sAMGeometry3D in sAMGeometry3Ds)
            {
                if(sAMGeometry3D is Shell)
                {
                    List<Face3D> face3Ds_Temp = ((Shell)sAMGeometry3D).Face3Ds;
                    if(face3Ds_Temp != null && face3Ds_Temp.Count != 0)
                    {
                        face3Ds.AddRange(face3Ds_Temp);
                    }
                }
                else if(sAMGeometry3D is Face3D)
                {
                    face3Ds.Add((Face3D)sAMGeometry3D);
                }
            }

            List<Panel> result = new List<Panel>();
            foreach(Face3D face3D in face3Ds)
            {
                Panel panel = Analytical.Create.Panel(construction, PanelType.Shade, face3D);
                if(panel == null)
                {
                    continue;
                }

                List<Core.ParameterSet> parameterSets = revitType3D.GetParamaterSets();
                if (parameterSets != null)
                {
                    foreach (Core.ParameterSet parameterSet in parameterSets)
                    {
                        panel.Add(parameterSet);
                    }
                }

                result.Add(panel);
            }

            return result;
        }

        public static List<Panel> ToSAM_Panels(this FamilyInstance familyInstance, ConvertSettings convertSettings)
        {
            if (familyInstance == null)
            {
                return null;
            }

            List<Panel> result = convertSettings?.GetObjects<Panel>(familyInstance.Id);
            if (result != null)
            {
                return result;
            }

            RevitInstance3D revitInstance3D = Geometry.Revit.Convert.ToSAM(familyInstance, convertSettings) as RevitInstance3D;
            if(revitInstance3D == null)
            {
                return null;
            }

            result = ToSAM_Panels(revitInstance3D);

            if (result != null)
            {
                convertSettings?.Add(familyInstance.Id, result);
            }

            return result;
        }
    }
}