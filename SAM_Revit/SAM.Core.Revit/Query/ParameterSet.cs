using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;


namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static ParameterSet ParameterSet(this Element element, IEnumerable<string> parameterNames_Excluded = null)
        {
            if (element == null)
                return null;
            
            ParameterSet parameterSet = new ParameterSet(element.GetType()?.Assembly);
            parameterSet.Add("ElementId", element.Id.IntegerValue);
            foreach(Parameter parameter in element.Parameters)
            {
                if (parameterNames_Excluded != null && parameterNames_Excluded.Contains(parameter.Definition.Name))
                    continue;

                switch (parameter.StorageType)
                {
                    case StorageType.Double:
                        parameterSet.Add(parameter.Definition.Name, Units.Revit.Convert.ToSI(parameter.AsDouble(), parameter.Definition.ParameterType));
                        break;
                    case StorageType.Integer:
                        parameterSet.Add(parameter.Definition.Name, parameter.AsInteger());
                        break;
                    case StorageType.String:
                        parameterSet.Add(parameter.Definition.Name, parameter.AsString());
                        break;
                    case StorageType.ElementId:
                        parameterSet.Add(parameter.Definition.Name, Convert.ToSAM(parameter.AsElementId()));
                        break;
                }
            }
            return parameterSet;
        }


    }
}
