using Autodesk.Revit.DB;
using SAM.Core.Revit;

namespace SAM.Architectural.Revit
{
    public static partial class Convert
    {
        public static HostBuildingElementType ToSAM(this HostObjAttributes hostObjAttributes, ConvertSettings convertSettings)
        {
            if (hostObjAttributes == null)
                return null;

            HostBuildingElementType result = convertSettings?.GetObject<HostBuildingElementType>(hostObjAttributes.Id);
            if (result != null)
                return result;

            string name = hostObjAttributes.Name;

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

            result.UpdateParameterSets(hostObjAttributes, ActiveSetting.Setting.GetValue<Core.TypeMap>(ActiveSetting.Name.ParameterMap));

            convertSettings?.Add(hostObjAttributes.Id, result);

            return result;
        }
    }
}