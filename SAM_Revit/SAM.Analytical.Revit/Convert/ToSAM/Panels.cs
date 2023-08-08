using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Core.Revit;
using SAM.Geometry;
using SAM.Geometry.Planar;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<Panel> ToSAM(this Autodesk.Revit.DB.Panel panel, bool includeNonVisibleObjects, ConvertSettings convertSettings)
        {
            if (panel == null || !panel.IsValidObject)
            {
                return null;
            }

            List<Panel> result = convertSettings?.GetObjects<Panel>(panel.Id);
            if (result != null)
            {
                return result;
            }

            HostObject hostObject = panel.Host as HostObject;
            if(hostObject == null)
            {
                return null;
            }

            List<Face3D> face3Ds_HostObject = hostObject.Profiles();
            if (face3Ds_HostObject == null || face3Ds_HostObject.Count == 0)
            {
                return null;
            }

            List<Shell> shells = panel.ToSAM_Geometries<Shell>(includeNonVisibleObjects);
            if(shells == null || shells.Count == 0)
            {
                return null;
            }

            List<Face3D> face3Ds = new List<Face3D>();
            foreach(Shell shell in shells)
            {
                foreach(Face3D face3D in face3Ds_HostObject)
                {
                    PlanarIntersectionResult planarIntersectionResult = Geometry.Spatial.Create.PlanarIntersectionResult(face3D, shell);
                    if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    {
                        continue;
                    }


                    List<Face2D> face2Ds = Geometry.Planar.Create.Face2Ds(planarIntersectionResult.GetGeometry2Ds<ISegmentable2D>());
                    if(face2Ds == null || face2Ds.Count == 0)
                    {
                        continue;
                    }

                    Geometry.Spatial.Plane plane = planarIntersectionResult.Plane;
                    face3Ds.AddRange(face2Ds.ConvertAll(x => plane.Convert(x)));
                    break;

                }
            }


            if(face3Ds != null)
            {
                face3Ds.RemoveAll(x => x == null);
            }

            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            PanelType panelType = Query.PanelType(hostObject);

            Construction construction = panel.PanelType.ToSAM_Construction(convertSettings);
            if (construction == null)
            {
                construction = Analytical.Query.DefaultConstruction(panelType); //Default Construction
            }

            if(result == null)
            {
                result = new List<Panel>();
            }

            foreach(Face3D face3D_Temp in face3Ds)
            {
                Panel panel_Temp = Analytical.Create.Panel(construction, panelType, face3D_Temp);
                if(panel_Temp == null)
                {
                    continue;
                }
                
                panel_Temp.UpdateParameterSets(panel, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

                result.Add(panel_Temp);
            }

            convertSettings?.Add(panel.Id, result);

            if (convertSettings.UseProjectLocation)
            {
                Transform transform = Core.Revit.Query.ProjectTransform(panel.Document);
                if (transform != null)
                {
                    result = result.ConvertAll(x => Query.Transform(transform, x));
                }
            }

            return result;
        }

        public static List<Panel> ToSAM(this HostObject hostObject, ConvertSettings convertSettings)
        {
            if (hostObject == null || !hostObject.IsValidObject)
            {
                return null;
            }

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
#if Revit2017
            IEnumerable<ElementId> elementIds = null;
#else
            IEnumerable<ElementId> elementIds = hostObject.GetDependentElements(logicalOrFilter);
#endif

            if (hostObject is Autodesk.Revit.DB.Wall || hostObject is CurtainSystem)
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
                    
                    List<Aperture> apertures_Temp = ToSAM_Apertures(familyInstance, convertSettings);
                    if(apertures_Temp == null)
                    {
                        continue;
                    }

                    apertures.AddRange(apertures_Temp);
                }
            }


            foreach (Face3D face3D in face3Ds)
            {
                if (face3D == null)
                    continue;

                //Panel panel = Analytical.Create.Panel(construction, panelType, face3D.Reduce(0.2)); Double check why Reduce have been used turn off on 27.10.2021 when mesh roof in sam teplate gave gap
                Panel panel = Analytical.Create.Panel(construction, panelType, face3D);
                panel.UpdateParameterSets(hostObject, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

                foreach(Aperture aperture in apertures)
                {
                    panel.AddAperture(aperture);
                }

                result.Add(panel);
            }

            convertSettings?.Add(hostObject.Id, result);

            if (convertSettings.UseProjectLocation)
            {
                Transform transform = Core.Revit.Query.ProjectTransform(hostObject.Document);
                if (transform != null)
                {
                    result = result.ConvertAll(x => Query.Transform(transform, x));
                }
            }

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

            if (convertSettings.UseProjectLocation)
            {
                Transform transform = Core.Revit.Query.ProjectTransform(document);
                if (transform != null)
                {
                    result = result.ConvertAll(x => Query.Transform(transform, x));
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

            if (convertSettings.UseProjectLocation)
            {
                transform = Core.Revit.Query.ProjectTransform(revitLinkInstance.Document);
                if (transform != null)
                {
                    result = result.ConvertAll(x => Query.Transform(transform, x));
                }
            }

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
                Level level_Max = Core.Revit.Query.HighLevel(document, elevation_Min + Core.Tolerance.MacroDistance);
                if (level_Max == null)
                {
                    continue;
                }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                double height = UnitUtils.ConvertFromInternalUnits(level_Max.Elevation - elevation_Min, DisplayUnitType.DUT_METERS);
#else
                double height = UnitUtils.ConvertFromInternalUnits(level_Max.Elevation - elevation_Min, UnitTypeId.Meters);
#endif

                if (height == 0)
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

            if (convertSettings.UseProjectLocation)
            {
                Transform transform = Core.Revit.Query.ProjectTransform(modelCurve.Document);
                if (transform != null)
                {
                    result = result.ConvertAll(x => Query.Transform(transform, x));
                }
            }

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

            if (convertSettings.UseProjectLocation)
            {
                Transform transform = Core.Revit.Query.ProjectTransform(wallSweep.Document);
                if (transform != null)
                {
                    result = result.ConvertAll(x => Query.Transform(transform, x));
                }
            }

            return result;
        }

        public static List<Panel> ToSAM_Panels(this RevitInstance3D revitInstance3D)
        {
            if(revitInstance3D == null)
            {
                return null;
            }

            RevitType3D revitType3D = revitInstance3D.Type as RevitType3D;
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

                List<Core.ParameterSet> parameterSets = revitType3D.GetParameterSets();
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

            if (convertSettings.UseProjectLocation)
            {
                Transform transform = Core.Revit.Query.ProjectTransform(familyInstance.Document);
                if (transform != null)
                {
                    result = result.ConvertAll(x => Query.Transform(transform, x));
                }
            }

            return result;
        }
    }
}