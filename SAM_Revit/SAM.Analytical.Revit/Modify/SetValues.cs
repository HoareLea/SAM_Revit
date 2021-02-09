using Autodesk.Revit.DB;
using SAM.Core;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static bool SetValues(this Element element, Result result, Setting setting, LoadType loadType)
        {
            if (element == null || result == null)
                return false;

            string name = null;
            switch(loadType)
            {
                case LoadType.Cooling:
                    name = ActiveSetting.Name.ParameterMap_Cooling;
                    break;

                case LoadType.Heating:
                    name = ActiveSetting.Name.ParameterMap_Heating;
                    break;
            }

            if (string.IsNullOrWhiteSpace(name))
                return false;

            TypeMap typeMap;
            if (!setting.TryGetValue(name, out typeMap) || typeMap == null)
                return false;

            return Core.Revit.Modify.SetValues(element, result, typeMap);
        }

        public static bool SetValues(this Element element, Zone zone, Setting setting, ZoneType zoneType)
        {
            if (element == null || zone == null)
                return false;

            string name = null;
            switch (zoneType)
            {
                case ZoneType.Cooling:
                    name = ActiveSetting.Name.ParameterMap_Cooling;
                    break;

                case ZoneType.Heating:
                    name = ActiveSetting.Name.ParameterMap_Heating;
                    break;

                case ZoneType.Ventilation:
                    name = ActiveSetting.Name.ParameterMap_Ventilation;
                    break;
            }

            if (string.IsNullOrWhiteSpace(name))
                return false;

            TypeMap typeMap;
            if (!setting.TryGetValue(name, out typeMap) || typeMap == null)
                return false;

            return Core.Revit.Modify.SetValues(element, zone, typeMap);
        }
    }
}