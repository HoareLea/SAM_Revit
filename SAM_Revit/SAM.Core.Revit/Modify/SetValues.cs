using Autodesk.Revit.DB;
using SAM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        /// <summary>
        /// Sets Revit element parameters uisng values in given ParameterSet
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <param name="parameterSet">SAM ParameterSet values will be teaken</param>
        /// <param name="parameterNames_Excluded"> Parameter Names to be skipped</param>
        /// <returns></returns>
        public static bool SetValues(this Element element, ParameterSet parameterSet,  IEnumerable<string> parameterNames_Excluded = null)
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
                    SetValue(parameter, parameterSet.ToObject(name));
            }

            return true;
        }

        public static bool SetValues(this Element element, ParameterSet parameterSet, IEnumerable<BuiltInParameter> builtInParameters_Excluded)
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

                    SetValue(parameter, parameterSet.ToObject(name));
                }
            }

            return true;
        }

        public static bool SetValues(this Element element, SAMObject sAMObject, Setting setting)
        {
            if (element == null)
                return false;

            TypeMap typeMap;
            if (!setting.TryGetValue(ActiveSetting.Name.ParameterMap, out typeMap) || typeMap == null)
                return false;

            List<string> names = typeMap.GetNames(sAMObject.GetType(), element.GetType());
            if (names != null || names.Count > 0)
            {
                foreach (string name in names)
                    SetValue(element, typeMap.GetName(sAMObject.GetType(), element.GetType(), name), sAMObject, name);
            }

            return true;
        }

        public static bool SetValues(this Element element, SAMObject sAMObject,  IEnumerable<string> parameterNames_Excluded = null)
        {
            if (sAMObject == null || element == null)
                return false;

            List<ParameterSet> parameterSets = sAMObject.GetParamaterSets();
            if(parameterSets != null && parameterSets.Count != 0)
            {
                foreach (ParameterSet parameterSet in parameterSets)
                    element.SetValues(parameterSet, parameterNames_Excluded);
            }

            Setting setting = ActiveSetting.Setting;

            if (!element.SetValues(sAMObject, setting))
                return false;

            return true;
        }

        public static bool SetValues(this Element element, SAMObject sAMObject, IEnumerable<BuiltInParameter> builtInParameters_Excluded)
        {
            if (sAMObject == null || element == null)
                return false;

            ParameterSet parameterSet = sAMObject.GetParameterSet(element.GetType()?.Assembly);
            if (parameterSet != null)
            {
                if (!element.SetValues(parameterSet, builtInParameters_Excluded))
                    return false;
            }

            Setting setting = ActiveSetting.Setting;

            if (!element.SetValues(sAMObject, setting))
                return false;

            return true;
        }
    }
}