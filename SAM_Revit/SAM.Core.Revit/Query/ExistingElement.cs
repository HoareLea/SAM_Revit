using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Element ExistingElement(this Document document, SAMObject sAMObject)
        {
            if (document == null || sAMObject == null)
                return null;

            ElementId elementId = sAMObject.ElementId();
            if (elementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            return document.GetElement(elementId);

        }
    }
}