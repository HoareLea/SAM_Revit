using Autodesk.Revit.DB;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Create
    {
        public static Log Log(SAMObject sAMObject, Document document)
        {
            if (sAMObject == null || document == null)
                return null;

            ElementId elementId = Core.Revit.Query.ElementId(sAMObject);
            if (elementId == null || elementId == ElementId.InvalidElementId)
                return null;

            Log result = new Log();

            Element element = document.GetElement(elementId);
            if (element == null)
            {
                result.Add("There is no mathing Revit element for {0} {1} (Guid: {2})", LogRecordType.Warning, sAMObject.Name, sAMObject.GetType().Name, sAMObject.Guid);
                return result;
            }

            return result;
        }
    }
}