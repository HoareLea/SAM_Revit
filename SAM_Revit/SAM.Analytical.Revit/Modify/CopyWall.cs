using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static Autodesk.Revit.DB.Wall CopyWall(this UIDocument uIDocument)
        {
            if(uIDocument?.Document == null)
            {
                return null;
            }

            List<RevitLinkInstance> revitLinkInstances = new FilteredElementCollector(uIDocument.Document).OfClass(typeof(RevitLinkInstance)).Cast<RevitLinkInstance>().ToList();

            if (revitLinkInstances == null || revitLinkInstances.Count < 1)
                return null;

            RevitLinkInstance revitLinkInstance = revitLinkInstances.First();

            Document document = revitLinkInstance.GetLinkDocument();
            if (document == null)
                return null;

            View view = uIDocument.ActiveView;
            if (view == null || view.GenLevel == null)
                return null;

            Autodesk.Revit.DB.Reference reference = null;
            try
            {
                reference = uIDocument.Selection.PickObject(ObjectType.LinkedElement, new RevitLinkInstanceSelectionFilter(uIDocument.Document, BuiltInCategory.OST_Walls), "Select Wall");
            }
            catch (Exception exception)
            {
                return null;
            }

            if (reference == null)
                return null;

            Level level_Bottom = view.GenLevel;

            if (level_Bottom == null)
                return null;

            Level level_Top = Core.Revit.Query.HighLevel(uIDocument.Document, level_Bottom.Elevation);
            if (level_Top == null)
                return null;

            List<FamilyInstance> familyInstances_Grid = new FilteredElementCollector(uIDocument.Document).OfClass(typeof(FamilyInstance)).OfCategory(BuiltInCategory.OST_DetailComponents).Cast<FamilyInstance>().ToList().FindAll(x => x.Name == "Snapping Region");

            return CopyWall(uIDocument.Document, new Autodesk.Revit.DB.Reference[] { reference }, level_Bottom, level_Top);
        }

        public static Autodesk.Revit.DB.Wall CopyWall(this Document document, IEnumerable<Autodesk.Revit.DB.Reference> references, Level level_Bottom, Level level_Top)
        {
            if (document == null || references == null || references.Count() < 1 || level_Bottom == null || level_Top == null)
                return null;

            Dictionary<BuiltInCategory, FamilySymbol> dictionary_FamilySymbol = new Dictionary<BuiltInCategory, FamilySymbol>();
            dictionary_FamilySymbol.Add(BuiltInCategory.OST_Windows, new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Windows).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList().Find(x => x.Name == "SIM_EXT_GLZ"));
            dictionary_FamilySymbol.Add(BuiltInCategory.OST_Doors, new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Doors).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList().Find(x => x.Name == "SIM_INT_SLD"));

            Autodesk.Revit.DB.Wall result = null;

            Dictionary<ElementId, Tuple<Document, Dictionary<ElementId, List<FamilyInstance>>>> dictionary = new Dictionary<ElementId, Tuple<Document, Dictionary<ElementId, List<FamilyInstance>>>>();
            using (Transaction transaction = new Transaction(document, "Copy Wall"))
            {
                transaction.Start();
                foreach (Autodesk.Revit.DB.Reference reference in references)
                {
                    if (reference.ElementId == null || reference.ElementId == ElementId.InvalidElementId)
                        continue;

                    if (reference.LinkedElementId == null || reference.LinkedElementId == ElementId.InvalidElementId)
                        continue;

                    RevitLinkInstance revitLinkInstance = document.GetElement(reference.ElementId) as RevitLinkInstance;

                    if (revitLinkInstance == null)
                        continue;

                    Tuple<Document, Dictionary<ElementId, List<FamilyInstance>>> tuple = null;
                    if (!dictionary.TryGetValue(revitLinkInstance.Id, out tuple))
                    {
                        Document document_RevitLinkInstance = revitLinkInstance.GetLinkDocument();

                        List<ElementFilter> elementFilters = new List<ElementFilter>();
                        elementFilters.Add(new ElementCategoryFilter(BuiltInCategory.OST_Doors));
                        elementFilters.Add(new ElementCategoryFilter(BuiltInCategory.OST_Windows));

                        List<FamilyInstance> familyInstances = new FilteredElementCollector(document_RevitLinkInstance).OfClass(typeof(FamilyInstance)).WherePasses(new LogicalOrFilter(elementFilters)).Cast<FamilyInstance>().ToList();

                        Dictionary<ElementId, List<FamilyInstance>> dictionary_FamilyInstance = new Dictionary<ElementId, List<FamilyInstance>>();
                        foreach (FamilyInstance familyInstance in familyInstances)
                        {
                            if (familyInstance.Host == null || familyInstance.Host.Id == null || familyInstance.Host.Id == ElementId.InvalidElementId)
                                continue;

                            List<FamilyInstance> familyInstances_Temp = null;
                            if (!dictionary_FamilyInstance.TryGetValue(familyInstance.Host.Id, out familyInstances_Temp))
                            {
                                familyInstances_Temp = new List<FamilyInstance>();
                                dictionary_FamilyInstance.Add(familyInstance.Host.Id, familyInstances_Temp);
                            }
                            familyInstances_Temp.Add(familyInstance);
                        }

                        tuple = new Tuple<Document, Dictionary<ElementId, List<FamilyInstance>>>(document_RevitLinkInstance, dictionary_FamilyInstance);

                        dictionary.Add(revitLinkInstance.Id, tuple);
                    }

                    if (tuple.Item1 == null)
                        continue;

                    Element element = tuple.Item1.GetElement(reference.LinkedElementId);
                    if (element == null)
                        continue;

                    if (element is Autodesk.Revit.DB.Wall)
                    {
                        Autodesk.Revit.DB.Wall wall = element as Autodesk.Revit.DB.Wall;
                        Autodesk.Revit.DB.WallType wallType = UpdateWallType(document, wall);
                        result = CopyWall(wallType, wall, level_Bottom, level_Top, tuple.Item2, dictionary_FamilySymbol);
                    }
                }
                transaction.Commit();
            }

            return result;
        }

        private static Autodesk.Revit.DB.Wall CopyWall(Autodesk.Revit.DB.WallType wallType, Autodesk.Revit.DB.Wall Wall, Level level_Bottom, Level level_Top, Dictionary<ElementId, List<FamilyInstance>> dictionary_Hosts, Dictionary<BuiltInCategory, FamilySymbol> dictionary_Symbols)
        {
            if (wallType == null || Wall == null)
                return null;

            if (!(Wall.Location is LocationCurve))
                return null;

            LocationCurve locationCurve = Wall.Location as LocationCurve;

            Document document = wallType.Document;

            Autodesk.Revit.DB.Wall result = Autodesk.Revit.DB.Wall.Create(wallType.Document, locationCurve.Curve, level_Bottom.Id, false);

            result.WallType = wallType;

            //FamilyInstance aFamilyInstance = aDocument.Create.NewFamilyInstance(aLocationCurve.Curve, ElementType, Level_Bottom, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            if (result == null)
                return null;

            if (Wall.Flipped != result.Flipped)
                result.Flip();

            Parameter parameter = null;

            if (level_Top.Elevation > level_Bottom.Elevation)
            {
                parameter = result.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                if (parameter != null)
                    parameter.Set(level_Top.Id);
            }
            else
            {
                parameter = result.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                if (parameter != null && !parameter.IsReadOnly)
                {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                    parameter.Set(UnitUtils.ConvertToInternalUnits(2000, DisplayUnitType.DUT_MILLIMETERS));
#else
                    parameter.Set(UnitUtils.ConvertToInternalUnits(2000, UnitTypeId.Millimeters));
#endif
                }

            }

            parameter = result.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            if (parameter != null)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                parameter.Set(Wall.Id.IntegerValue.ToString());
#else
                parameter.Set(Wall.Id.Value.ToString());
#endif
            }

            document.Regenerate();

            AddHosts(result, Wall.Id, dictionary_Hosts, dictionary_Symbols);

            return result;
        }

        private static void AddHosts(this Autodesk.Revit.DB.Wall wall, ElementId elementId, Dictionary<ElementId, List<FamilyInstance>> dictionary_Host, Dictionary<BuiltInCategory, FamilySymbol> dictionary_FamilySymbol)
        {
            List<FamilyInstance> familyInstances = null;

            if (!dictionary_Host.TryGetValue(elementId, out familyInstances))
                familyInstances = null;

            if (familyInstances == null || familyInstances.Count < 1)
                return;

            foreach (FamilyInstance familyInstance in familyInstances)
            {
                try
                {
                    AddHost(wall, familyInstance, dictionary_FamilySymbol);
                }
                catch (Exception exception)
                {
                    continue;
                }

            }
        }

        private static void AddHost(Autodesk.Revit.DB.Wall wall, FamilyInstance familyInstance, Dictionary<BuiltInCategory, FamilySymbol> dictionary_FamilySymbol)
        {
            if (wall == null || familyInstance == null)
                return;

            XYZ xYZ = null;
            if (familyInstance.Location is LocationPoint)
            {
                xYZ = ((LocationPoint)familyInstance.Location).Point;
            }
            else if (familyInstance.Location is LocationCurve)
            {
                xYZ = ((LocationCurve)familyInstance.Location).Curve.GetEndPoint(0);
            }

            if (xYZ == null)
                return;

            GeometryElement geometryElement = wall.get_Geometry(new Options());

            BoundingBoxXYZ boundingBoxXYZ = geometryElement.GetBoundingBox();

            if (xYZ.Z >= boundingBoxXYZ.Max.Z)
                return;

            if (xYZ.Z < boundingBoxXYZ.Min.Z)
                return;

            FamilySymbol familySymbol = null;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            if (!dictionary_FamilySymbol.TryGetValue((BuiltInCategory)familyInstance.Category.Id.IntegerValue, out familySymbol))
                return;
#else
            if (!dictionary_FamilySymbol.TryGetValue((BuiltInCategory)familyInstance.Category.Id.Value, out familySymbol))
                return;
#endif

            if (familySymbol == null)
                return;

            if (!familySymbol.IsActive)
                familySymbol.Activate();

            FamilyInstance familyInstance_Temp = wall.Document.Create.NewFamilyInstance(xYZ, familySymbol, wall, wall.Document.GetElement(wall.LevelId) as Level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            if (familyInstance_Temp.FacingFlipped != familyInstance.FacingFlipped)
                familyInstance_Temp.flipFacing();

            if (familyInstance_Temp.HandFlipped != familyInstance.HandFlipped)
                familyInstance_Temp.flipHand();

            FamilySymbol aFamilySymbol_Source = familyInstance.Symbol;

            Copy(familyInstance_Temp, familyInstance, "SAM_BuildingElementHeight", "Height");
            Copy(familyInstance_Temp, familyInstance, "SAM_BuildingElementWidth", "Width");
            Copy(familyInstance_Temp, familyInstance, "SAM_BuildingElementFrameWidth", "FrameThickness");
            Copy(familyInstance_Temp, familyInstance, "Sill Height", "Sill Height");
            Copy(familyInstance_Temp, familyInstance, "_Filter Comments 01", "Mark");
            Copy(familyInstance_Temp, familyInstance, "_Filter Comments 03", "Description");

            Parameter parameter;

            parameter = familyInstance_Temp.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
            if (parameter != null)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                parameter.Set(familyInstance.Id.IntegerValue.ToString());
#else
                parameter.Set(familyInstance.Id.Value.ToString());
#endif
            }

            parameter = familyInstance_Temp.LookupParameter("SAM_BuildingElementDescription");
            if (parameter != null)
                parameter.Set(familyInstance.Name);
        }

        private static void Copy(FamilyInstance familyInstance_Destination, FamilyInstance familyInstance_Source, string name_Destination, string name_Source)
        {
            Parameter parameter_Source = null;
            Parameter parameter_Destination = null;

            parameter_Source = familyInstance_Source.LookupParameter(name_Source);
            if (parameter_Source == null)
                parameter_Source = familyInstance_Source.Symbol.LookupParameter(name_Source);

            if (parameter_Source != null)
            {
                parameter_Destination = familyInstance_Destination.LookupParameter(name_Destination);
                if (parameter_Destination == null)
                    parameter_Destination = familyInstance_Destination.Symbol.LookupParameter(name_Destination);

                if (parameter_Destination != null)
                {
                    Core.Revit.Modify.CopyValue(parameter_Source, parameter_Destination);
                }
            }
        }
    }
}