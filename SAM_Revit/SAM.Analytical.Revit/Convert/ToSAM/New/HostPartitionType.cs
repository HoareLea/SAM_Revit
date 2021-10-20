using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System.Collections.Generic;

using SAM.Architectural.Revit;
using SAM.Architectural;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostPartitionType ToSAM_HostPartitionType(this HostObjAttributes hostObjAttributes, ConvertSettings convertSettings)
        {
            if (hostObjAttributes == null)
            {
                return null;
            }

            HostPartitionType result = convertSettings?.GetObject<HostPartitionType>(hostObjAttributes.Id);
            if (result != null)
            {
                return result;
            }

            string name = hostObjAttributes.Name;

            List<MaterialLayer> materialLayers = Architectural.Revit.Create.MaterialLayers(hostObjAttributes.Document, hostObjAttributes.GetCompoundStructure());
            if(materialLayers == null)
            {
                return null;
            }

            if(hostObjAttributes is Autodesk.Revit.DB.WallType)
            {
                result = new WallType(name);
            }
            else if(hostObjAttributes is Autodesk.Revit.DB.RoofType)
            {
                result = new RoofType(name);
            }
            else if (hostObjAttributes is Autodesk.Revit.DB.FloorType)
            {
                result = new FloorType(name);
            }

            CompoundStructure compoundStructure = hostObjAttributes.GetCompoundStructure();
            if(compoundStructure != null)
            {
                compoundStructure.GetLayers();
            }

            if(result != null)
            {
                result.UpdateParameterSets(hostObjAttributes);
                convertSettings?.Add(hostObjAttributes.Id, result);
            }

            return result;
        }
    }
}