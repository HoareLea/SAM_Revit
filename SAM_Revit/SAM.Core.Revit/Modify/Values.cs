using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool Values(this ParameterSet parameterSet, Element element, IEnumerable<string> parameterNames_Excluded = null)
        {
            if (parameterSet == null || element == null)
                return false;

            foreach (string name in parameterSet.Names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (parameterNames_Excluded != null && parameterNames_Excluded.Contains(name))
                    continue;

                IEnumerable<Parameter> parameters = element.GetParameters(name);
                if (parameters == null || parameters.Count() == 0)
                    continue;

                foreach (Parameter parameter in parameters)
                    Value(parameter, parameterSet.ToObject(name));
            }

            return true;
        }

        public static bool Values(this ParameterSet parameterSet, Element element, IEnumerable<BuiltInParameter> builtInParameters_Excluded)
        {
            if (parameterSet == null || element == null)
                return false;

            List<int> Ids = null;
            if (builtInParameters_Excluded != null)
                Ids = builtInParameters_Excluded.ToList().ConvertAll(x => (int)x);

            foreach (string name in parameterSet.Names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                IEnumerable<Parameter> parameters = element.GetParameters(name);
                if (parameters == null || parameters.Count() == 0)
                    continue;

                foreach (Parameter parameter in parameters)
                {
                    if (Ids != null && Ids.Contains(parameter.Id.IntegerValue))
                        continue;

                    Value(parameter, parameterSet.ToObject(name));
                }
            }

            return true;
        }

        public static bool Values(this Setting setting, SAMObject sAMObject, Element element)
        {
            if (element == null)
                return false;

            MapCluster mapCluster;
            if (setting.TryGetValue(ActiveSetting.Name.ParameterMap, out mapCluster))
            {
                if (mapCluster != null)
                {
                    List<string> names = mapCluster.GetNames(sAMObject.GetType(), element.GetType());
                    if (names != null || names.Count > 0)
                    {
                        foreach (string name in names)
                            Value(sAMObject, element, name, mapCluster.GetName(sAMObject.GetType(), element.GetType(), name));
                    }
                }
            }

            return true;
        }

        public static bool Values(this SAMObject sAMObject, Element element, IEnumerable<string> parameterNames_Excluded = null)
        {
            if (sAMObject == null || element == null)
                return false;

            ParameterSet parameterSet = sAMObject.GetParameterSet(element.GetType()?.Assembly);
            if (parameterSet != null)
            {
                if (!Values(parameterSet, element, parameterNames_Excluded))
                    return false;
            }

            Setting setting = ActiveSetting.Setting;

            if (!Values(setting, sAMObject, element))
                return false;

            return true;
        }

        public static bool Values(this SAMObject sAMObject, Element element, IEnumerable<BuiltInParameter> builtInParameters_Excluded)
        {
            if (sAMObject == null || element == null)
                return false;

            ParameterSet parameterSet = sAMObject.GetParameterSet(element.GetType()?.Assembly);
            if (parameterSet != null)
            {
                if (!Values(parameterSet, element, builtInParameters_Excluded))
                    return false;
            }

            Setting setting = ActiveSetting.Setting;

            if (!Values(setting, sAMObject, element))
                return false;

            return true;
        }
    }
}