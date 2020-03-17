using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Autodesk.Revit.DB;


namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool Values(this ParameterSet parameterSet, Element element, IEnumerable<string> parameterNames_Excluded = null)
        {
            if (parameterSet == null || element == null)
                return false;

            foreach(string name in parameterSet.Names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (parameterNames_Excluded != null && parameterNames_Excluded.Contains(name))
                    continue;

                Value(element.LookupParameter(name), parameterSet.ToObject(name));
            }

            return true;
        }

        public static bool Values(this Setting setting, SAMObject sAMObject, Element element)
        {
            if (element == null)
                return false;

            SAMRelationCluster sAMRelationCluster;
            if (setting.TryGetValue(ActiveSetting.Name.ParameterMap, out sAMRelationCluster))
            {
                if (sAMRelationCluster != null)
                {
                    List<SAMRelation> sAMRelations = sAMRelationCluster.GetSAMRelations(sAMObject.GetType(), element.GetType());
                    if (sAMRelations != null || sAMRelations.Count > 0)
                    {
                        foreach (SAMRelation sAMRelation in sAMRelations)
                        {
                            if (sAMRelation == null)
                                continue;

                            Value(sAMRelation, sAMObject, element);
                        }
                    }
                }
            }

            return true;
        }

        public static bool Values(this SAMObject sAMObject, Element element, IEnumerable<string> parameterNames_Excluded = null)
        {
            if (sAMObject == null || element == null)
                return false;

            Assembly assembly;

            assembly = element.GetType()?.Assembly;
            if (assembly != null)
            {
                ParameterSet parameterSet = sAMObject.GetParameterSet(assembly);
                if (parameterSet != null)
                {
                    if (!Values(parameterSet, element, parameterNames_Excluded))
                        return false;
                }
            }

            assembly = Assembly.GetExecutingAssembly();
            if (assembly != null)
            {
                Setting setting = ActiveSetting.Setting;

                if (!Values(setting, sAMObject, element))
                    return false;
            }

            return true;
        }
    }
}
