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

            result.AddRange(Log(adjacencyCluster.GetPanels(), document));

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

            Log result = new Log();

            return result;
        }

        public static Log Log(Construction construction, Document document)
        {
            if (construction == null || document == null)
                return null;

            Log result = new Log();

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