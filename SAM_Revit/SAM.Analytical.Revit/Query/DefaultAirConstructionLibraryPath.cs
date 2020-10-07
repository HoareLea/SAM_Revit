using SAM.Core;
using System.Reflection;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static string DefaultAirConstructionLibraryPath()
        {
            return DefaultAirConstructionLibraryPath(ActiveSetting.Setting);
        }

        public static string DefaultAirConstructionLibraryPath(Setting setting)
        {
            if (setting == null)
                return null;

            string fileName;
            if (!setting.TryGetValue(ActiveSetting.Name.FileName_DefaultAirConstructionLibrary, out fileName) || string.IsNullOrWhiteSpace(fileName))
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