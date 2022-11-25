using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static PanelGroup PanelGroup(this HostObjAttributes hostObjAttributes)
        {
            if (hostObjAttributes == null)
                return Analytical.PanelGroup.Undefined;

            return PanelType((BuiltInCategory)hostObjAttributes.Category.Id.IntegerValue).PanelGroup();
        }
    }
}