using SAM.Core;
using System.Reflection;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static string DefaultToolsParameterMapPath()
        {
            return DefaultToolsParameterMapPath(ActiveSetting.Setting);
        }

        public static string DefaultToolsParameterMapPath(Setting setting)
        {
            if (setting == null)
                return null;

            string fileName;
            if (!setting.TryGetValue(ActiveSetting.Name.ParameterMap_Tools, out fileName) || string.IsNullOrWhiteSpace(fileName))
                return null;

            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            string resourcesDirectory = Core.Query.ResourcesDirectory(Assembly.GetExecutingAssembly());
            if (string.IsNullOrWhiteSpace(resourcesDirectory))
                return null;

            return System.IO.Path.Combine(resourcesDirectory, fileName);
        }
    }
}