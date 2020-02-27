using System.Collections.Generic;

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

            return new ApertureConstruction(familyInstance.FullName(), familyInstance.ApertureType());
        }
    }
}
