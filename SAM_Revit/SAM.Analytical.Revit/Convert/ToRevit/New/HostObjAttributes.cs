using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObjAttributes ToRevit(this HostPartitionType hostPartitionType, Document document, Core.Revit.ConvertSettings convertSettings)
        {
            if (hostPartitionType == null)
            {
                return null;
            }

            HostObjAttributes result = null;

            List<HostObjAttributes> hostObjAttributesList = convertSettings?.GetObjects<HostObjAttributes>(hostPartitionType.Guid);
            if (hostObjAttributesList != null)
            {
                if (hostPartitionType is WallType)
                {
                    result = hostObjAttributesList.Find(x => x is Autodesk.Revit.DB.WallType);
                }
                else if (hostPartitionType is FloorType)
                {
                    result = hostObjAttributesList.Find(x => x is Autodesk.Revit.DB.FloorType);
                }
                else if (hostPartitionType is RoofType)
                {
                    result = hostObjAttributesList.Find(x => x is Autodesk.Revit.DB.RoofType);
                }
            }

            if (result != null)
            {
                return result;
            }

            FilteredElementCollector filteredElementCollector = Query.FilteredElementCollector_New(document, hostPartitionType.GetType())?.OfClass(typeof(HostObjAttributes));
            if (filteredElementCollector == null)
            {
                return null;
            }

            string familyName_Source = null;
            string typeName_Source = null;
            if (!Core.Revit.Query.TryGetFamilyNameAndTypeName(hostPartitionType.Name, out familyName_Source, out typeName_Source))
                return null;

            foreach (HostObjAttributes hostObjAttributes in filteredElementCollector)
            {
                string fullName = Core.Revit.Query.FullName(hostObjAttributes);

                string familyName = null;
                string typeName = null;
                if (!Core.Revit.Query.TryGetFamilyNameAndTypeName(fullName, out familyName, out typeName))
                {
                    continue;
                }

                if (fullName != null && fullName.Equals(hostPartitionType.Name))
                {
                    result = hostObjAttributes;
                    break;
                }

                if (!string.IsNullOrWhiteSpace(familyName) && !string.IsNullOrWhiteSpace(familyName_Source))
                {
                    continue;
                }


                if (typeName.Equals(typeName_Source))
                {
                    result = hostObjAttributes;
                }

            }

            if (result == null)
            {
                return result;
            }

            List<Architectural.MaterialLayer> materialLayers = hostPartitionType.MaterialLayers;
            if (materialLayers != null && materialLayers.Count != 0)
            {
                CompoundStructure compoundStructure = result.GetCompoundStructure();
                if (compoundStructure != null)
                {
                    List<Material> materials = new FilteredElementCollector(document).OfClass(typeof(Material)).Cast<Material>().ToList();

                    List<CompoundStructureLayer> compoundStructureLayers = new List<CompoundStructureLayer>();
                    foreach (Architectural.MaterialLayer materialLayer in materialLayers)
                    {
                        Material material = materials.Find(x => x.Name == materialLayer.Name);
                        if (material == null)
                        {
                            continue;
                        }

                        double width = double.NaN;

                        width = UnitUtils.ConvertToInternalUnits(materialLayer.Thickness, DisplayUnitType.DUT_METERS);

                        CompoundStructureLayer compoundStructureLayer = new CompoundStructureLayer(width, MaterialFunctionAssignment.Structure, material.Id);
                        if (compoundStructureLayer == null)
                        {
                            continue;
                        }

                        compoundStructureLayers.Add(compoundStructureLayer);
                    }

                    if (compoundStructureLayers != null && compoundStructureLayers.Count != 0)
                    {
                        compoundStructure.SetLayers(compoundStructureLayers);
                        result.SetCompoundStructure(compoundStructure);
                    }
                }
            }

            if (convertSettings.ConvertParameters)
            {
                Core.Revit.Modify.SetValues(result, hostPartitionType);
                Core.Revit.Modify.SetValues(result, hostPartitionType, ActiveSetting.Setting);
            }

            convertSettings?.Add(hostPartitionType.Guid, result);

            return result;
        }

    }
}
