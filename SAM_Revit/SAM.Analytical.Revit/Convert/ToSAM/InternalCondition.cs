using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static InternalCondition ToSAM_InternalCondition(this SpatialElement spatialElement, ConvertSettings convertSettings)
        {
            if (spatialElement == null)
                return null;

            InternalCondition result = convertSettings?.GetObject<InternalCondition>(spatialElement.Id);
            if (result != null)
                    return result;

            string name = null;

            Core.TypeMap typeMap = ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap);
            if(typeMap != null)
            {
                string parameterName = typeMap.GetName(typeof(InternalCondition), typeof(Autodesk.Revit.DB.Mechanical.Space), "Name", 2);
                if(!string.IsNullOrWhiteSpace(parameterName))
                {
                    Parameter parameter = spatialElement.GetParameters(parameterName)?.ToList().Find(x => x.StorageType == StorageType.String);
                    if (parameter != null)
                        name = parameter.AsString();
                }

                if(string.IsNullOrWhiteSpace(name))
                    name = Core.Revit.Query.Name(spatialElement);

                if (string.IsNullOrWhiteSpace(name))
                    name = spatialElement.Name;
            }

            result = new InternalCondition(name);

            //Replace by UpdateParameterSets(this SAMObject sAMObject, Type type, IEnumerable < Parameter > parameters, TypeMap typeMap)
            result.UpdateParameterSets(spatialElement, typeMap);

            convertSettings?.Add(spatialElement.Id, result);

            return result;
        }
    }
}