
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static ParameterSet ParameterSet(this Element element)
        {
            if (element == null)
                return null;
            
            ParameterSet parameterSet = new ParameterSet(element.GetType()?.Assembly);
            parameterSet.Add("ElementId", element.Id.IntegerValue);
            foreach(Parameter parameter in element.Parameters)
            {
                switch(parameter.StorageType)
                {
                    case StorageType.Double:
                        parameterSet.Add(parameter.Definition.Name, parameter.AsDouble());
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
