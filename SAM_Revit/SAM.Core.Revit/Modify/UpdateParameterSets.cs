using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static void UpdateParameterSets(this SAMObject sAMObject, Element element, MapCluster mapCluster, IEnumerable<string> parameterNames_Excluded = null)
        {
            if (sAMObject == null || element == null)
                return;

            Assembly assembly = element.GetType().Assembly;
            if (assembly == null)
                return;

            ParameterSet parameterSet = sAMObject.GetParameterSet(assembly);
            if(parameterSet == null)
                parameterSet = new ParameterSet(assembly);

            Type type_SAMObject = sAMObject.GetType();
            Type type_Element = element.GetType();

            List<Enum> enums = ActiveManager.GetParameterEnums(sAMObject);

            //MapCluster mapCluster;
            //ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.ParameterMap, out mapCluster);

            foreach (Parameter parameter in element.ParametersMap)
            {
                string parameterName_Element = parameter?.Definition?.Name;
                if (parameterName_Element == null)
                    continue;

                if (parameterNames_Excluded != null && parameterNames_Excluded.Contains(parameterName_Element))
                    continue;

                object value = null;
                switch (parameter.StorageType)
                {
                    case StorageType.Double:
                        value = Units.Revit.Convert.ToSI(parameter.AsDouble(), parameter.Definition.ParameterType);
                        break;

                    case StorageType.Integer:
                        value = parameter.AsInteger();
                        break;

                    case StorageType.String:
                        value = parameter.AsString();
                        break;

                    case StorageType.ElementId:
                        value = Convert.ToSAM(parameter.AsElementId());
                        break;
                }

                if (mapCluster != null)
                {
                    string parametrName_SAMObject = mapCluster.GetName(type_SAMObject, type_Element, parameterName_Element, 1);
                    if (!string.IsNullOrWhiteSpace(parametrName_SAMObject))
                    {
                        List<Enum> enums_Temp = enums.FindAll(x => x.Name() == parametrName_SAMObject);
                        if (enums_Temp != null && enums_Temp.Count != 0)
                        {
                            foreach(Enum @enum in enums_Temp)
                                sAMObject.SetValue(@enum, value);

                            continue;                            
                        }
                    }
                }

                parameterSet.Add(parameterName_Element, value as dynamic);
            }

            sAMObject.SetValue(ElementParameter.ElementId, element.Id.IntegerValue);
            sAMObject.SetValue(ElementParameter.UniqueId, element.UniqueId);
            sAMObject.SetValue(ElementParameter.CategoryName, element.Category?.Name);
        }
    }
}