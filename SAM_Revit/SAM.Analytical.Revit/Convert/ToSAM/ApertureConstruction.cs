using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static ApertureConstruction ToSAM_ApertureConstruction(this FamilyInstance familyInstance)
        {
            if (familyInstance == null)
                return null;

            FamilySymbol familySymbol = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as FamilySymbol;
            if (familySymbol == null)
                return null;

            return ToSAM_ApertureConstruction(familySymbol);
        }

        public static ApertureConstruction ToSAM_ApertureConstruction(this FamilySymbol familySymbol)
        {
            if (familySymbol == null)
                return null;

            ApertureConstruction apertureConstruction = new ApertureConstruction(familySymbol.FullName(), familySymbol.ApertureType());
            apertureConstruction.Add(Core.Revit.Query.ParameterSet(familySymbol));

            return apertureConstruction;
        }
    }
}