using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Create
    {
        public static Log Log(SAMObject sAMObject, Document document)
        {
            if (sAMObject == null || document == null)
                return null;

            ElementId elementId = Query.ElementId(sAMObject);
            if (elementId == null || elementId == ElementId.InvalidElementId)
                return null;

            Log result = new Log();

            Element element = document.GetElement(elementId);
            if (element == null)
            {
                result.Add("There is no mathing Revit element with Element Id {0} for {1} {2} (Guid: {3})", LogRecordType.Warning, elementId.IntegerValue, sAMObject.Name, sAMObject.GetType().Name, sAMObject.Guid);
                return result;
            }

            return result;
        }
    }
}