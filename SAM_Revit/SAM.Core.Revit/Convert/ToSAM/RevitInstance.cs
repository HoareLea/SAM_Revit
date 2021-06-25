using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static RevitInstance ToSAM(this FamilyInstance familyInstance, ConvertSettings convertSettings)
        {
            if (familyInstance == null)
            {
                return null;
            }

            RevitInstance result = convertSettings?.GetObject<RevitInstance>(familyInstance.Id);
            if (result != null)
            {
                return result;
            }




            if (result != null)
            {
                convertSettings?.Add(familyInstance.Id, result);
            }

            return result;
        }
    }
}