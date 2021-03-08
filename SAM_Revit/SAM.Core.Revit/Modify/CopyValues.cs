using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<string> CopyValues(this Element element_Source, Element element_Destionation, TextMap textMap)
        {
            if (element_Source == null || element_Destionation == null || textMap == null)
                return null;

            List<string> result = new List<string>();
            foreach(string key in textMap.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                Parameter parameter_Source = element_Source.LookupParameter(key);
                if (parameter_Source == null || !parameter_Source.HasValue)
                    continue;

                foreach(string value in textMap.GetValues(key))
                {
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    IEnumerable<Parameter> parameters_Destination = element_Source.GetParameters(value);
                    if(parameters_Destination != null && parameters_Destination.Count() != 0)
                    {
                        foreach(Parameter parameter_Destination in parameters_Destination)
                        {
                            if (parameter_Destination == null || parameter_Destination.IsReadOnly)
                                continue;

                            if (CopyValue(parameter_Source, parameter_Destination))
                                result.Add(parameter_Destination.Definition.Name);
                        }
                    }
                }
            }

            return result;
        }
    }
}