﻿using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static FamilySymbol ToRevit(this ApertureConstruction apertureConstruction, Document document, Core.Revit.ConvertSettings convertSettings)
        {
            if (apertureConstruction == null)
                return null;

            FamilySymbol result = convertSettings?.GetObject<FamilySymbol>(apertureConstruction.Guid);
            if (result != null)
                return result;

            string fullName = apertureConstruction.Name;

            string familyName;
            string familyTypeName;
            if (!Core.Revit.Query.TryGetFamilyNameAndTypeName(fullName, out familyName, out familyTypeName))
                return null;

            List<BuiltInCategory> builtInCategories = new List<BuiltInCategory>();
            BuiltInCategory builtInCategory = apertureConstruction.BuiltInCategory();
            if(builtInCategory == BuiltInCategory.INVALID)
            {
                builtInCategories.Add(Query.BuiltInCategory(ApertureType.Door));
                builtInCategories.Add(Query.BuiltInCategory(ApertureType.Window));
            }
            else
            {
                builtInCategories.Add(builtInCategory);
            }

            List<FamilySymbol> familySymbols = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).WherePasses(new LogicalOrFilter(builtInCategories.ConvertAll(x => new ElementCategoryFilter(x) as ElementFilter))).Cast<FamilySymbol>().ToList();
            if (familySymbols == null || familySymbols.Count == 0)
                return null;

            familySymbols.RemoveAll(x => string.IsNullOrWhiteSpace(x.Name) || !x.Name.Equals(familyTypeName));
            if (!string.IsNullOrWhiteSpace(familyName))
                familySymbols.RemoveAll(x => string.IsNullOrWhiteSpace(x.FamilyName) || !x.FamilyName.Equals(familyName));

            familySymbols.RemoveAll(x => x.Family == null && x.Family.FamilyPlacementType != FamilyPlacementType.OneLevelBasedHosted);

            if (familySymbols.Count == 0)
                return null;

            result = familySymbols.First();
            if (result == null)
                return null;

            if (!result.IsActive)
                result.Activate();

            convertSettings?.Add(apertureConstruction.Guid, result);

            return result;
        }
    }
}