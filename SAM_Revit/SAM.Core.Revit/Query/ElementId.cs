using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static ElementId ElementId(this SAMObject sAMObject)
        {
            if (sAMObject == null)
                return null;

            if (!sAMObject.TryGetValue(ElementParameter.RevitId, out IntegerId integerId) || integerId == null)
            {
                return null;
            }

            return ElementId(integerId);
        }

        public static ElementId ElementId(this IntegerId integerId)
        {
            if (integerId == null)
            {
                return null;
            }

            return new ElementId(integerId.Id);
        }

        public static ElementId ElementId(this string originatingElementDescription)
        {
            if (string.IsNullOrEmpty(originatingElementDescription))
                return null;

            int startIndex = originatingElementDescription.LastIndexOf("[");
            if (startIndex == -1)
                return null;

            int endIndex = originatingElementDescription.IndexOf("]", startIndex);
            if (endIndex == -1)
                return null;

            string elementID = originatingElementDescription.Substring(startIndex + 1, endIndex - startIndex - 1);

            int id;
            if (!int.TryParse(elementID, out id))
                return null;

            return new ElementId(id);
        }
    }
}