using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static ApertureConstruction ToSAM_ApertureConstruction(this FamilyInstance familyInstance, Core.Revit.ConvertSettings convertSettings)
        {
            if (familyInstance == null)
                return null;

            FamilySymbol familySymbol = familyInstance.Document.GetElement(familyInstance.GetTypeId()) as FamilySymbol;
            if (familySymbol == null)
                return null;

            return ToSAM_ApertureConstruction(familySymbol, convertSettings);
        }

        public static ApertureConstruction ToSAM_ApertureConstruction(this FamilySymbol familySymbol, Core.Revit.ConvertSettings convertSettings)
        {
            if (familySymbol == null)
                return null;

            ApertureConstruction result = convertSettings?.GetObject<ApertureConstruction>(familySymbol.Id);
            if (result != null)
                return result;

            result = new ApertureConstruction(familySymbol.FullName(), familySymbol.ApertureType());
            result.Add(Core.Revit.Query.ParameterSet(familySymbol));

            convertSettings?.Add(familySymbol.Id, result);

            return result;
        }
    }
}