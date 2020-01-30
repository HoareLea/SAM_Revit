using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObjAttributes ToRevit(this Document document, Construction construction, PanelType panelType)
        {
            if (construction == null)
                return null;
            FilteredElementCollector filteredElementCollector = new FilteredElementCollector(document).OfClass(typeof(HostObjAttributes));

            BuiltInCategory builtInCategory = panelType.BuiltInCategory();
            if (builtInCategory != BuiltInCategory.INVALID)
                filteredElementCollector.OfCategory(builtInCategory);

            foreach (HostObjAttributes hostObjAttributes in filteredElementCollector)
            {
                string name = Core.Revit.Query.FullName(hostObjAttributes);
                //if (string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(hostObjAttributes.Name))
                //{
                //    string familyName;
                //    string typeName;
                //    if (!Core.Revit.Query.TryGetFamilyNameAndTypeName(construction.Name, out familyName, out typeName))
                //        continue;

                //    if (string.IsNullOrWhiteSpace(typeName))
                //        continue;

                //    if(typeName.Equals(construction.Name))
                //        return hostObjAttributes;
                //}

                if (name != null && name.Equals(construction.Name))
                    return hostObjAttributes;
            }

            return null;
        }
    }
}
