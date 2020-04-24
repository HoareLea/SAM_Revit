using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static FamilySymbol ToRevit(this Document document, ApertureConstruction apertureConstruction, Core.Revit.ConvertSettings convertSettings)
        {
            if (apertureConstruction == null)
                return null;
            
            string fullName = apertureConstruction.Name;

            string familyName;
            string familyTypeName;
            if (!Core.Revit.Query.TryGetFamilyNameAndTypeName(fullName, out familyName, out familyTypeName))
                return null;

            List<FamilySymbol> familySymbols = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList();
            if (familySymbols == null || familySymbols.Count == 0)
                return null;

            familySymbols.RemoveAll(x => string.IsNullOrWhiteSpace(x.Name) || !x.Name.Equals(familyTypeName));
            if (!string.IsNullOrWhiteSpace(familyName))
                familySymbols.RemoveAll(x => string.IsNullOrWhiteSpace(x.FamilyName) || !x.FamilyName.Equals(familyName));

            familySymbols.RemoveAll(x => x.Family == null && x.Family.FamilyPlacementType != FamilyPlacementType.OneLevelBasedHosted);

            if (familySymbols.Count == 0)
                return null;

            FamilySymbol familySymbol = familySymbols.First();
            if (familySymbol == null)
                return null;

            if (!familySymbol.IsActive)
                familySymbol.Activate();

            return familySymbol;
        }
    }
}
