using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Element ScopeBox(this View view)
        {
            if (view == null)
                return null;

            ElementId elementId = view.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP)?.AsElementId();
            if (elementId == null || elementId == Autodesk.Revit.DB.ElementId.InvalidElementId)
                return null;

            return view.Document.GetElement(elementId);
        }
    }
}