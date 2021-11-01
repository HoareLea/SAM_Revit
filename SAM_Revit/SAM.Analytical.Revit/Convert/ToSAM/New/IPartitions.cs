using Autodesk.Revit.DB;
using SAM.Core.Revit;
using SAM.Geometry.Spatial;
using SAM.Geometry.Revit;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<IPartition> ToSAM_Partitions(this HostObject hostObject, ConvertSettings convertSettings)
        {
            if (hostObject == null)
                return null;

            List<IPartition> result = convertSettings?.GetObjects<IPartition>(hostObject.Id);
            if (result != null)
            {
                return result;
            }

            ElementId elementId_Type = hostObject.GetTypeId();
            if (elementId_Type == null || elementId_Type == ElementId.InvalidElementId)
            {
                return null;
            }

            HostPartitionType hostPartitionType = ((HostObjAttributes)hostObject.Document.GetElement(elementId_Type)).ToSAM_HostPartitionType(convertSettings);
            if(hostPartitionType == null)
            {
                return null;
            }

            List<Face3D> face3Ds = hostObject.Profiles();
            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            LogicalOrFilter logicalOrFilter = new LogicalOrFilter(new List<ElementFilter>() { new ElementCategoryFilter(BuiltInCategory.OST_Windows), new ElementCategoryFilter(BuiltInCategory.OST_Doors) });

#if Revit2017
            IEnumerable<ElementId> elementIds = null;
#else
            IEnumerable<ElementId> elementIds = hostObject.GetDependentElements(logicalOrFilter);
#endif

            if (hostObject is Autodesk.Revit.DB.Wall || hostObject is CurtainSystem)
            {
                List<Autodesk.Revit.DB.Panel> panels = Core.Revit.Query.Panels(hostObject as dynamic);
                if (panels != null && panels.Count > 0)
                {
                    List<ElementId> elementIds_Temp = panels.ConvertAll(x => x.Id);
                    if (elementIds != null && elementIds.Count() > 0)
                        elementIds_Temp.AddRange(elementIds);

                    elementIds = elementIds_Temp;
                }
            }

            result = new List<IPartition>();

            foreach (Face3D face3D in face3Ds)
            {
                if (face3D == null)
                    continue;

                IHostPartition hostPartition = Analytical.Create.HostPartition(face3D, hostPartitionType);
                hostPartition.UpdateParameterSets(hostObject);

                if (elementIds != null && elementIds.Count() > 0)
                {
                    foreach (ElementId elementId in elementIds)
                    {
                        Element element = hostObject.Document.GetElement(elementId);
                        if (element == null)
                            continue;

                        if (!(element is FamilyInstance))
                            continue;

                        IOpening opening = ToSAM_Opening((FamilyInstance)element, convertSettings);
                        if(opening != null)
                        {
                            opening = Analytical.Query.Project(hostPartition, opening);
                            hostPartition.AddOpening(opening);
                        }
                    }
                }

                result.Add(hostPartition);
            }

            convertSettings?.Add(hostObject.Id, result);

            return result;
        }
    }
}