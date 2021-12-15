using SAM.Core;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static DelimitedFileTable DefaultToolsParameterMap()
        {
            return DefaultToolsParameterMap(ActiveSetting.Setting);
        }

        public static DelimitedFileTable DefaultToolsParameterMap(Setting setting)
        {
            if (setting == null)
                return null;

            string path = DefaultToolsParameterMapPath(setting);
            if(string.IsNullOrWhiteSpace(path) || System.IO.File.Exists(path))
            {
                return null;
            }

            return Core.Create.DelimitedFileTable(DelimitedFileType.TabDelimited, path);
        }
    }
}