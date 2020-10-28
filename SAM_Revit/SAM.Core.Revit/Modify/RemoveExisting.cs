using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool RemoveExisting(this ConvertSettings convertSettings, Document document, SAMObject sAMObject)
        {
            if (convertSettings == null || document == null || sAMObject == null)
                return false;

            if (!convertSettings.RemoveExisting)
                return false;

            ElementId elementId = sAMObject.ElementId();
            if (elementId == null || elementId == ElementId.InvalidElementId)
                return false;

            Element element = document.GetElement(elementId);
            if (element == null)
                return false;

            IEnumerable<ElementId> elementIds = document.Delete(elementId);

            return elementIds != null && elementIds.Count() != 0;
        }
    }
}