using Autodesk.Revit.DB;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Create
    {
        public static Log Log(this AnalyticalModel analyticalModel, Document document)
        {
            if(analyticalModel == null || document == null)
                return null;

            Log result = new Log();
            result.AddRange(Log(analyticalModel.AdjacencyCluster, document));


            return result;
        }

        public static Log Log(this AdjacencyCluster adjacencyCluster, Document document)
        {
            if (adjacencyCluster == null || document == null)
                return null;

            Log result = new Log();

            foreach (Construction construction in adjacencyCluster.GetConstructions())
                result.AddRange(Log(construction, document));

            foreach (ApertureConstruction apertureConstruction in adjacencyCluster.ApertureConstructions())
                result.AddRange(Log(apertureConstruction, document));

            List<Panel> panels = adjacencyCluster.GetPanels();

            List<Architectural.Level> levels = Analytical.Create.Levels(panels);
            if (levels == null || levels.Count == 0)
                result.Add("Could not find proper levels in AdjacencyCluster", LogRecordType.Error);
            else
                result.AddRange(Architectural.Revit.Create.Log(levels, document));

            return result;
        }

        public static Log Log(this IEnumerable<Panel> panels, Document document)
        {
            if (panels == null || document == null)
                return null;

            Log result = new Log();

            foreach(Panel panel in panels)
                result.AddRange(Log(panel, document));

            return result;
        }

        public static Log Log(this Panel panel, Document document)
        {
            if (panel == null || document == null)
                return null;

            Construction construction = panel.Construction;

            Log result = new Log();

            if (panel.PanelType != PanelType.Air && construction == null)
            {
                result.Add("Panel {0} is missing Construction Guid: {1}", LogRecordType.Error, panel.Name, panel.Guid);
            }

            result.AddRange(Log(construction, document));

            return result;
        }

        public static Log Log(this Aperture aperture, Document document)
        {
            if (aperture == null || document == null)
                return null;

            ApertureConstruction apertureConstruction = aperture.ApertureConstruction;

            Log result = new Log();

            if (apertureConstruction == null)
            {
                result.Add("Aperture {0} is missing ApertureConstruction Guid: {1}", LogRecordType.Error, aperture.Name, aperture.Guid);
            }

            result.AddRange(Log(apertureConstruction, document));

            return result;
        }

        public static Log Log(Construction construction, Document document)
        {
            if (construction == null || document == null)
                return null;

            Log result = new Log();

            string name = construction.Name;

            if (string.IsNullOrWhiteSpace(name))
            {
                result.Add("Construction has invalid name Guid: {0}", LogRecordType.Error, construction.Guid);
                return result;
            }

            PanelType panelType = construction.PanelType();
            if(panelType == PanelType.Undefined)
            {
                result.Add("Could not get panelType from Construction Guid: {0}, Name: {1}", LogRecordType.Warning, construction.Guid, construction.Name);
                return result;
            }

            List<BuiltInCategory> builtInCategories = Query.BuiltInCategories(panelType);
            if(builtInCategories == null || builtInCategories.Count == 0)
            {
                result.Add("Could not get BuiltInCategory from Construction Guid: {0}, Name: {1}", LogRecordType.Error, construction.Guid, construction.Name);
                return result;
            }

            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(document).OfClass(typeof(HostObjAttributes));
            if (builtInCategories.Count == 1)
                filteredElementCollector.OfCategory(builtInCategories[0]);
            else
                filteredElementCollector.WherePasses(new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter)));

            List<HostObjAttributes> hostObjAttributes = filteredElementCollector.Cast<HostObjAttributes>().ToList();
            if (hostObjAttributes != null && hostObjAttributes.Count != 0)
            {
                foreach (HostObjAttributes hostObjAttributes_Temp in hostObjAttributes)
                {
                    string fullName = Core.Revit.Query.FullName(hostObjAttributes_Temp);
                    if (construction.Name.Equals(fullName) || construction.Name.Equals(hostObjAttributes_Temp.Name))
                        return result;
                }
            }

            result.Add("Could not find Revit FamilyType (Category: {2}) Name: {1} for Construction Guid: {0}", LogRecordType.Error, construction.Guid, construction.Name, document?.Settings?.Categories?.get_Item(builtInCategories[0])?.Name);
            return result;
        }

        public static Log Log(ApertureConstruction apertureConstruction, Document document)
        {
            if (apertureConstruction == null || document == null)
                return null;

            Log result = new Log();

            string name = apertureConstruction.Name;

            if(string.IsNullOrWhiteSpace(name))
            {
                result.Add("ApertureConstruction has invalid name Guid: {0}", LogRecordType.Error, apertureConstruction.Guid);
                return result;
            }

            BuiltInCategory builtInCategory = apertureConstruction.BuiltInCategory();
            if(builtInCategory == BuiltInCategory.INVALID)
            {
                result.Add("Could not get BuiltInCategory from ApertureConstruction Guid: {0}, Name: {1}", LogRecordType.Error, apertureConstruction.Guid, apertureConstruction.Name);
                return result;
            }

            List<FamilySymbol> familySymbols = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).OfCategory(builtInCategory).Cast<FamilySymbol>().ToList();
            if (familySymbols != null && familySymbols.Count != 0)
            {
                foreach (FamilySymbol familySymbol in familySymbols)
                {
                    string fullName = Core.Revit.Query.FullName(familySymbol);
                    if (apertureConstruction.Name.Equals(fullName) || apertureConstruction.Name.Equals(familySymbol.Name))
                        return result;
                }
            }

            result.Add("Could not find Revit FamilyType Name: {1} for ApertureConstruction Guid: {0}", LogRecordType.Error, apertureConstruction.Guid, apertureConstruction.Name);
            return result;
        }
    }
}