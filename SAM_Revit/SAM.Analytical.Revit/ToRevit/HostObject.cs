using System;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObject ToRevit(this Document document, Panel panel)
        {
            HostObjAttributes aHostObjAttributes = document.ToRevit(panel.Construction, panel.PanelType);

            throw new NotImplementedException();

            //FilteredElementCollector filteredElementCollector = new FilteredElementCollector(document).OfClass(typeof(HostObjAttributes));

            //BuiltInCategory builtInCategory = panelType.BuiltInCategory();
            //if (builtInCategory != BuiltInCategory.INVALID)
            //    filteredElementCollector.OfCategory(builtInCategory);

            //foreach(HostObjAttributes hostObjAttributes in filteredElementCollector)
            //{
            //    string name = Core.Revit.Query.FullName(hostObjAttributes);
            //    if (name.Equals(construction.Name))
            //        return hostObjAttributes;
            //}


            //return null;
        }
    }
}
