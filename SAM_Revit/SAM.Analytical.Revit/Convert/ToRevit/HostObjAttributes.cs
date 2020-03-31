using Autodesk.Revit.DB;


namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObjAttributes ToRevit(this Document document, Construction construction, PanelType panelType, Core.Revit.ConvertSettings convertSettings)
        {
            if (construction == null)
                return null;
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(document).OfClass(typeof(HostObjAttributes));

            BuiltInCategory builtInCategory = panelType.BuiltInCategory();
            if (builtInCategory != BuiltInCategory.INVALID)
                filteredElementCollector.OfCategory(builtInCategory);

            string familyName_Source = null;
            string typeName_Source = null;
            if (!Core.Revit.Query.TryGetFamilyNameAndTypeName(construction.Name, out familyName_Source, out typeName_Source))
                return null;

            HostObjAttributes hostObjAttributes_Default = null;
            foreach (HostObjAttributes hostObjAttributes in filteredElementCollector)
            {
                string fullName = Core.Revit.Query.FullName(hostObjAttributes);

                string familyName = null;
                string typeName = null;
                if (!Core.Revit.Query.TryGetFamilyNameAndTypeName(fullName, out familyName, out typeName))
                    continue;

                if (fullName != null && fullName.Equals(construction.Name))
                    return hostObjAttributes;

                if (!string.IsNullOrWhiteSpace(familyName) && !string.IsNullOrWhiteSpace(familyName_Source))
                    continue;

                if (typeName.Equals(typeName_Source))
                    hostObjAttributes_Default = hostObjAttributes;
            }

            return hostObjAttributes_Default;
        }
    }
}
