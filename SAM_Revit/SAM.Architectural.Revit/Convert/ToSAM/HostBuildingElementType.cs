using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System.Collections.Generic;

namespace SAM.Architectural.Revit
{
    public static partial class Convert
    {
        public static HostBuildingElementType ToSAM(this HostObjAttributes hostObjAttributes, ConvertSettings convertSettings)
        {
            if (hostObjAttributes == null)
            {
                return null;
            }

            HostBuildingElementType result = convertSettings?.GetObject<HostBuildingElementType>(hostObjAttributes.Id);
            if (result != null)
            {
                return result;
            }

            string name = hostObjAttributes.Name;

            List<MaterialLayer> materialLayers = Create.MaterialLayers(hostObjAttributes.Document, hostObjAttributes.GetCompoundStructure());
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
                result.UpdateParameterSets(hostObjAttributes, ActiveSetting.Setting.GetValue<Core.TypeMap>(ActiveSetting.Name.ParameterMap));
                convertSettings?.Add(hostObjAttributes.Id, result);
            }

            return result;
        }
    }
}