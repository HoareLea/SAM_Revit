using Autodesk.Revit.DB;
using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static bool SetValues(this Element element, Result result, Setting setting, LoadType loadType, Dictionary<string, object> parameters = null)
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

            return Core.Revit.Modify.SetValues(element, result, typeMap, parameters);
        }

        public static bool SetValues(this Element element, SAMObject sAMObject, Setting setting, ZoneType zoneType, Dictionary<string, object> parameters = null)
        {
            if (element == null || sAMObject == null)
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

            return Core.Revit.Modify.SetValues(element, sAMObject, typeMap, parameters);
        }
    }
}