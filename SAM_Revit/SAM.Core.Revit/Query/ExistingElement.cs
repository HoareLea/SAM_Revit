using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Element ExistingElement(this Document document, SAMObject sAMObject)
        {
            if (document == null || sAMObject == null)
                return null;

            ParameterSet parameterSet = sAMObject.GetParameterSet(document.GetType().Assembly);
            if (parameterSet == null)
                return null;

            int id = parameterSet.ToInt("ElementId");
            if (id == -1)
                return null;

            ElementId elementId = new ElementId(id);
            if (elementId == ElementId.InvalidElementId)
                return null;

            return document.GetElement(elementId);

        }
    }
}