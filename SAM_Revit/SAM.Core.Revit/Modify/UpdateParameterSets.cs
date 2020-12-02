using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static void UpdateParameterSets(this SAMObject sAMObject, Type type, IEnumerable<Parameter> parameters, TypeMap typeMap)
        {
            if (sAMObject == null || type == null || parameters == null)
                return;

            Assembly assembly = type.Assembly;
            if (assembly == null)
                return;

            ParameterSet parameterSet = sAMObject.GetParameterSet(assembly);
            if (parameterSet == null)
            {
                parameterSet = new ParameterSet(assembly);
                sAMObject.Add(parameterSet);
            }

            Type type_SAMObject = sAMObject.GetType();

            List<Enum> enums = ActiveManager.GetParameterEnums(sAMObject);

            foreach (Parameter parameter in parameters)
            {
                string parameterName_Element = parameter?.Definition?.Name;
                if (parameterName_Element == null)
                    continue;

                object value = null;
                switch (parameter.StorageType)
                {
                    case StorageType.Double:
                        value = Units.Revit.Convert.ToSI(parameter.AsDouble(), parameter.Definition.UnitType);
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

                if (typeMap != null)
                {
                    string parametrName_SAMObject = typeMap.GetName(type_SAMObject, type, parameterName_Element, 1);
                    if (!string.IsNullOrWhiteSpace(parametrName_SAMObject))
                    {
                        List<Enum> enums_Temp = enums.FindAll(x => x.Name() == parametrName_SAMObject);
                        if (enums_Temp != null && enums_Temp.Count != 0)
                        {
                            foreach (Enum @enum in enums_Temp)
                                sAMObject.SetValue(@enum, value);

                            continue;
                        }
                    }
                }

                if (value == null)
                {
                    switch (parameter.StorageType)
                    {
                        case StorageType.ElementId:
                            IntegerId integerId = null;
                            parameterSet.Add(parameterName_Element, integerId);
                            continue; ;
                        default:
                            string @string = null;
                            parameterSet.Add(parameterName_Element, @string);
                            continue;
                    }
                }

                parameterSet.Add(parameterName_Element, value as dynamic);
            }
        }
        
        public static void UpdateParameterSets(this SAMObject sAMObject, Element element, TypeMap typeMap, IEnumerable<string> parameterNames_Excluded = null)
        {
            if (sAMObject == null || element == null)
                return;

            List<Parameter> parameters = new List<Parameter>();
            foreach (Parameter parameter in element.ParametersMap)
            {
                string parameterName_Element = parameter?.Definition?.Name;
                if (parameterName_Element == null)
                    continue;

                if (parameterNames_Excluded != null && parameterNames_Excluded.Contains(parameterName_Element))
                    continue;

                parameters.Add(parameter);
            }

            UpdateParameterSets(sAMObject, element.GetType(), parameters, typeMap);

            sAMObject.SetValue(ElementParameter.ElementId, element.Id.IntegerValue);
            sAMObject.SetValue(ElementParameter.UniqueId, element.UniqueId);
            sAMObject.SetValue(ElementParameter.CategoryName, element.Category?.Name);
        }
    }
}