using Autodesk.Revit.DB;
using SAM.Core.Revit;
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

            List<Panel> result = convertSettings?.GetObjects<Panel>(hostObject.Id);
            if (result != null)
                return result;

            ElementId elementId_Type = hostObject.GetTypeId();
            if (elementId_Type == null || elementId_Type == ElementId.InvalidElementId)
                return null;

            result = new List<Panel>();

            PanelType panelType = Query.PanelType(hostObject);
            Construction construction = ((HostObjAttributes)hostObject.Document.GetElement(elementId_Type)).ToSAM(convertSettings);
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

            if (hostObject is Wall)
            {
                List<Autodesk.Revit.DB.Panel> panels = Create.Panels((Wall)hostObject, convertSettings);
                if (panels != null && panels.Count > 0)
                {
                    List<ElementId> elementIds_Temp = panels.ConvertAll(x => x.Id);
                    if (elementIds != null && elementIds.Count() > 0)
                        elementIds_Temp.AddRange(elementIds);

                    elementIds = elementIds_Temp;
                }
            }

            foreach (Face3D face3D in face3Ds)
            {
                if (face3D == null)
                    continue;

                Panel panel = new Panel(construction, panelType, face3D);
                panel.UpdateParameterSets(hostObject, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

                if (elementIds != null && elementIds.Count() > 0)
                {
                    foreach (ElementId elementId in elementIds)
                    {
                        Element element = hostObject.Document.GetElement(elementId);
                        if (element == null)
                            continue;

                        if (!(element is FamilyInstance))
                            continue;

                        Aperture aperture = ToSAM_Aperture((FamilyInstance)element, convertSettings);
                        panel.AddAperture(aperture);
                    }
                }

                result.Add(panel);
            }

            convertSettings?.Add(hostObject.Id, result);

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
                    continue;

                double height = UnitUtils.ConvertFromInternalUnits(level_Max.Elevation - elevation_Min, DisplayUnitType.DUT_METERS);
                Vector3D vector3D = new Vector3D(0, 0, height);

                Face3D face3D = new Face3D(new Polygon3D(new Point3D[] { segment3D[0], segment3D[1], segment3D[1].GetMoved(vector3D) as Point3D, segment3D[0].GetMoved(vector3D) as Point3D}));

                Panel panel = new Panel(construction, panelType, face3D);
                panel.UpdateParameterSets(modelCurve, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap), null, new string[] { "Length" }, true);
                panel.SetValue(RevitPanelParameter.Length, segment3D.GetLength());
                result.Add(panel);
            }

            convertSettings?.Add(modelCurve.Id, result);

            return result;
        }
    }
}