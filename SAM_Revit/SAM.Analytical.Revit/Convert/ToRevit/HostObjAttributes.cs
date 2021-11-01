using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObjAttributes ToRevit(this Construction construction, Document document, PanelType panelType, Geometry.Spatial.Vector3D normal, Core.Revit.ConvertSettings convertSettings)
        {
            if (construction == null && panelType == PanelType.Air && normal != null)
                construction = Query.DefaultAirConstruction(normal.PanelType().PanelGroup());
            
            if (construction == null)
                return null;

            HostObjAttributes result = null;
            if(panelType != PanelType.Shade)
            {
                result = convertSettings?.GetObject<HostObjAttributes>(construction.Guid);
            }
            else
            {
                List<HostObjAttributes> hostObjAttributes = convertSettings?.GetObjects<HostObjAttributes>(construction.Guid);
                if(hostObjAttributes != null && hostObjAttributes.Count != 0)
                {
                    if (normal.PanelType().PanelGroup() == PanelGroup.Wall)
                        result = hostObjAttributes.Find(x => x is Autodesk.Revit.DB.WallType);
                    else
                        result = hostObjAttributes.Find(x => !(x is Autodesk.Revit.DB.WallType));
                }
            }

            if (result != null)
                return result;

            FilteredElementCollector filteredElementCollector = Query.FilteredElementCollector(document, panelType)?.OfClass(typeof(HostObjAttributes));
            if (filteredElementCollector == null)
                return null;

            if(normal != null)
            {
                if(panelType == PanelType.Air || panelType == PanelType.Shade)
                {
                    BuiltInCategory builtInCategory = normal.BuiltInCategory();
                    if (builtInCategory == BuiltInCategory.OST_Walls)
                        filteredElementCollector.OfCategory(builtInCategory);
                }
            }

            //FilteredElementCollector filteredElementCollector = new FilteredElementCollector(document).OfClass(typeof(HostObjAttributes));

            //BuiltInCategory builtInCategory = panelType.BuiltInCategory();
            //if (normal != null && (builtInCategory == BuiltInCategory.INVALID || panelType == PanelType.Shade))
            //    builtInCategory = normal.BuiltInCategory();

            //if (builtInCategory != BuiltInCategory.INVALID)
            //    filteredElementCollector.OfCategory(builtInCategory);


            string familyName_Source = null;
            string typeName_Source = null;
            if (!Core.Revit.Query.TryGetFamilyNameAndTypeName(construction.Name, out familyName_Source, out typeName_Source))
                return null;

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
                    result = hostObjAttributes;
            }

            convertSettings?.Add(construction.Guid, result);

            return result;
        }
    
        public static HostObjAttributes ToRevit_HostObjAttributes(this Panel panel, Document document, Core.Revit.ConvertSettings convertSettings)
        {
            if (panel == null || document == null)
                return null;

            return ToRevit(panel.Construction, document, panel.PanelType, panel.Normal, convertSettings);
        }
    }
}