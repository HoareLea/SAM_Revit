using SAM.Core;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static string ParameterName_ApertureHeight(this Setting setting)
        {
            if (setting == null)
                return null;
            
            string result;
            if (setting.TryGetValue(ActiveSetting.Name.ParameterName_ApertureHeight, out result))
                return result;

            return null;
        }

        public static string ParameterName_ApertureHeight()
        {
            return ParameterName_ApertureHeight(ActiveSetting.Setting);
        }
    }
}