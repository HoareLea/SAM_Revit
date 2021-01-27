using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        /// <summary>
        /// Sets Default values to parameters
        /// </summary>
        /// <param name="element">Revit Element</param>
        /// <param name="parameterNames"> Parameter Names to be included (rest to be skipped)</param>
        /// <returns>Parameter Names have been set to default</returns>
        public static List<string> SetDefault(this Element element, params string[] parameterNames)
        {
            if (element == null || parameterNames == null || parameterNames.Length == 0)
                return null;

            HashSet<string> names = new HashSet<string>();
            foreach (string parameterName in parameterNames)
            {
                IList<Parameter> parameters = element.GetParameters(parameterName);
                if (parameters == null || parameters.Count == 0)
                    continue;

                foreach(Parameter parameter in parameters)
                {
                    if (parameter == null || parameter.IsReadOnly)
                        continue;

                    switch (parameter.StorageType)
                    {
                        case StorageType.Double:
                            if (parameter.Set(default(double)))
                                names.Add(parameterName);
                            continue;
                        case StorageType.ElementId:
                            if(parameter.Set(ElementId.InvalidElementId))
                                names.Add(parameterName);
                            continue;
                        case StorageType.Integer:
                            if(parameter.Set(default(int)))
                                names.Add(parameterName);
                            continue;
                        case StorageType.String:
                            if(parameter.Set(default(string)))
                                names.Add(parameterName);
                            continue;
                    }
                }
            }

            return names.ToList();
        }
    }
}