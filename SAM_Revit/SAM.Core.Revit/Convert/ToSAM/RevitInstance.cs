using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static IRevitInstance ToSAM(this FamilyInstance familyInstance, ConvertSettings convertSettings)
        {
            if (familyInstance == null)
            {
                return null;
            }

            IRevitInstance result = convertSettings?.GetObject<IRevitInstance>(familyInstance.Id);
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