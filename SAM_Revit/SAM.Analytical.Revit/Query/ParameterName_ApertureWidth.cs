using SAM.Core;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static string ParameterName_BuildingElementWidth(this Setting setting)
        {
            if (setting == null)
                return null;
            
            string result;
            if (setting.TryGetValue(ActiveSetting.Name.ParameterName_ApertureWidth, out result))
                return result;

            return null;
        }

        public static string ParameterName_BuildingElementWidth()
        {
            return ParameterName_BuildingElementWidth(ActiveSetting.Setting);
        }
    }
}