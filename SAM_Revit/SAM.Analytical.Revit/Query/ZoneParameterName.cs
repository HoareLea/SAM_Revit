using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static string ZoneParameterName(this Zone zone)
        {
            if (zone == null)
                return null;

            if (!zone.TryGetValue(ZoneParameter.ZoneCategory, out string category) || string.IsNullOrWhiteSpace(category))
                return null;

            if (!ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.ZoneMap, out Core.TextMap textMap))
                return null;

            List<string> values = textMap?.GetValues(category);
            if (values == null || values.Count == 0)
                return null;

            return values[0];
        }
    }
}