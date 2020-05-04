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

            ApertureConstruction apertureConstruction = new ApertureConstruction(familyInstance.FullName(), familyInstance.ApertureType());
            apertureConstruction.Add(Core.Revit.Query.ParameterSet(familyInstance));

            return apertureConstruction;
        }
    }
}