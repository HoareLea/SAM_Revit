using Autodesk.Revit.DB;
using SAM.Core;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static bool SetValues(this Element element, Result result, Setting setting, LoadType loadType)
        {
            if (element == null)
                return false;

            string name = null;
            switch(loadType)
            {
                case LoadType.Cooling:
                    name = ActiveSetting.Name.ResultCoolingMap;
                    break;
                case LoadType.Heating:
                    name = ActiveSetting.Name.ResultHeatingMap;
                    break;
            }

            if (string.IsNullOrWhiteSpace(name))
                return false;

            TypeMap typeMap;
            if (!setting.TryGetValue(name, out typeMap) || typeMap == null)
                return false;

            return Core.Revit.Modify.SetValues(element, result, typeMap);
        }
    }
}