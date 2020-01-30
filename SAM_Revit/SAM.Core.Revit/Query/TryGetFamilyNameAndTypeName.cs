using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static bool TryGetFamilyNameAndTypeName(string fullName, out string familyName, out string typeName)
        {
            familyName = null;
            typeName = null;
            
            if (string.IsNullOrWhiteSpace(fullName))
                return false;

            string[] values = fullName.Split(':');
            if(values.Length == 1)
            {
                typeName = values[0].Trim();
            }
            else if(values.Length == 2)
            {
                familyName = values[0].Trim();
                typeName = values[1].Trim();
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
