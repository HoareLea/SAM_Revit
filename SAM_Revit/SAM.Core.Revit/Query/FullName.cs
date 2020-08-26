using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static string FullName(this Element element)
        {
            if (element == null || !element.IsValidObject)
                return null;
            
            ElementType elementType = null;
            string familyTypeName = element.Name;

            if (element is ElementType)
                elementType = (ElementType)element;
            else
                elementType = (ElementType)element.Document.GetElement(element.GetTypeId());

            string familyName = null;
            if (elementType != null)
            {
                Parameter parameter = elementType.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM);
                if (parameter != null)
                    familyName = parameter.AsValueString();

                if (string.IsNullOrWhiteSpace(familyName))
                    familyName = elementType.FamilyName;
            }

            return FullName(familyName, familyTypeName);
        }

        public static string FullName(string familyName, string familyTypeName)
        {
            if (string.IsNullOrWhiteSpace(familyTypeName))
                return null;

            if (!string.IsNullOrWhiteSpace(familyName) && !familyName.Equals("Rebuild curves and surfaces\nRebuild"))
                return string.Format("{0}: {1}", familyName, familyTypeName);

            return familyTypeName;
        }
    }
}