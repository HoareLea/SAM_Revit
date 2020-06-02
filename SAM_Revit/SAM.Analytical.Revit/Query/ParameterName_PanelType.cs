using SAM.Core;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static string ParameterName_PanelType(this Setting setting)
        {
            if (setting == null)
                return null;
            
            string result;
            if (setting.TryGetValue(ActiveSetting.Name.ParameterName_PanelType, out result))
                return result;

            return null;
        }

        public static string ParameterName_PanelType()
        {
            return ParameterName_PanelType(ActiveSetting.Setting);
        }
    }
}