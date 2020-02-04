using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static string FullName(this Element element)
        {
            if (element is ElementType)
                return FullName(((ElementType)element).FamilyName, element.Name);
            else
                return FullName(((ElementType)element.Document.GetElement(element.GetTypeId())).FamilyName, element.Name);
        }

        public static string FullName(string familyName, string familyTypeName)
        {
            if (string.IsNullOrWhiteSpace(familyTypeName))
                return null;

            if (!string.IsNullOrWhiteSpace(familyName))
                return string.Format("{0}: {1}", familyName, familyTypeName);

            return familyTypeName;
        }
    }
}
