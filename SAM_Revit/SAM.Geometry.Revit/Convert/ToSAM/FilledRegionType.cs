using SAM.Core;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static FilledRegionType ToSAM(this Autodesk.Revit.DB.FilledRegionType filledRegionType, ConvertSettings convertSettings)
        {
            if (filledRegionType == null)
            {
                return null;
            }

            FilledRegionType result = convertSettings?.GetObject<FilledRegionType>(filledRegionType.Id);
            if (result != null)
            {
                return result;
            }

            result = new FilledRegionType(filledRegionType.Name);

            if (result != null)
            {
                result.UpdateParameterSets(filledRegionType, ActiveSetting.Setting.GetValue<TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
                convertSettings?.Add(filledRegionType.Id, result);
            }

            return result;
        }
    }
}