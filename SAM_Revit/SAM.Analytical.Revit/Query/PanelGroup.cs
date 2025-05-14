using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static PanelGroup PanelGroup(this HostObjAttributes hostObjAttributes)
        {
            if (hostObjAttributes == null)
                return Analytical.PanelGroup.Undefined;
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            return PanelType((BuiltInCategory)hostObjAttributes.Category.Id.IntegerValue).PanelGroup();
#else
            return PanelType((BuiltInCategory)hostObjAttributes.Category.Id.Value).PanelGroup();
#endif
        }
    }
}