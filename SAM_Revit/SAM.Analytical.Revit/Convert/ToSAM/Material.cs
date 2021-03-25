using SAM.Core;
using SAM.Core.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Material ToSAM(this Autodesk.Revit.DB.Material material, ConvertSettings convertSettings)
        {
            Material result = Core.Revit.Convert.ToSAM(material, convertSettings);
            if (result == null)
                return result;

            result.UpdateParameterSets(material, ActiveSetting.Setting.GetValue<TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

            return result;
        }
    }
}