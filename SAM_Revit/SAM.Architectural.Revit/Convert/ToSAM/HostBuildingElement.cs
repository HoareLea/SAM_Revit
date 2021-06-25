using Autodesk.Revit.DB;
using SAM.Core.Revit;
using SAM.Geometry.Spatial;
using SAM.Geometry.Revit;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural.Revit
{
    public static partial class Convert
    {
        public static List<HostBuildingElement> ToSAM(this HostObject hostObject, ConvertSettings convertSettings)
        {
            if (hostObject == null)
                return null;

            List<HostBuildingElement> result = convertSettings?.GetObjects<HostBuildingElement>(hostObject.Id);
            if (result != null)
            {
                return result;
            }

            ElementId elementId_Type = hostObject.GetTypeId();
            if (elementId_Type == null || elementId_Type == ElementId.InvalidElementId)
            {
                return null;
            }

            HostBuildingElementType hostBuildingElementType = ((HostObjAttributes)hostObject.Document.GetElement(elementId_Type)).ToSAM(convertSettings);
            if(hostBuildingElementType == null)
            {
                return null;
            }

            List<Face3D> face3Ds = hostObject.Profiles();
            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(new List<ElementFilter>() { new ElementCategoryFilter(BuiltInCategory.OST_Windows), new ElementCategoryFilter(BuiltInCategory.OST_Doors) });
            IEnumerable<ElementId> elementIds = hostObject.GetDependentElements(logicalOrFilter);

            if (hostObject is Autodesk.Revit.DB.Wall || hostObject is CurtainSystem)
            {
                List<Panel> panels = Core.Revit.Query.Panels(hostObject as dynamic);
                if (panels != null && panels.Count > 0)
                {
                    List<ElementId> elementIds_Temp = panels.ConvertAll(x => x.Id);
                    if (elementIds != null && elementIds.Count() > 0)
                        elementIds_Temp.AddRange(elementIds);

                    elementIds = elementIds_Temp;
                }
            }

            result = new List<HostBuildingElement>();

            foreach (Face3D face3D in face3Ds)
            {
                if (face3D == null)
                    continue;

                HostBuildingElement hostBuildingElement = Architectural.Create.HostBuildingElement(face3D, hostBuildingElementType);
                hostBuildingElement.UpdateParameterSets(hostObject, Core.Revit.ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

                if (elementIds != null && elementIds.Count() > 0)
                {
                    foreach (ElementId elementId in elementIds)
                    {
                        Element element = hostObject.Document.GetElement(elementId);
                        if (element == null)
                            continue;

                        if (!(element is FamilyInstance))
                            continue;

                        Opening opening = ToSAM_Opening((FamilyInstance)element, convertSettings);
                        if(opening != null)
                        {
                            hostBuildingElement.AddOpening(opening);
                        }
                    }
                }

                result.Add(hostBuildingElement);
            }

            convertSettings?.Add(hostObject.Id, result);

            return result;
        }
    }
}