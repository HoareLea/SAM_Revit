using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static string Name(this SpatialElement spatialElement)
        {
            if (spatialElement == null)
                return null;

            string name = null;
            string number = spatialElement.Number;

            Parameter parameter = spatialElement.get_Parameter(BuiltInParameter.ROOM_NAME);
            if (parameter != null)
                name = parameter.AsString();

            string result = null;
            if (!string.IsNullOrEmpty(name))
                result = name;

            if (!string.IsNullOrEmpty(number))
            {
                if (string.IsNullOrEmpty(result))
                    result = number;
                else
                    result = string.Format("{0} {1}", number, result);
            }

            return result;
        }

        public static string Name(Setting setting, System.Type type_1, System.Type type_2, System.Enum @enum)
        {
            string name = @enum.Name();
            if (name == null)
                return null;

            MapCluster mapCluster;
            if (!setting.TryGetValue(ActiveSetting.Name.ParameterMap, out mapCluster) || mapCluster == null)
                return null;

            return mapCluster.GetName(type_1, type_2, @enum);
        }

        public static string Name(Setting setting, SAMObject sAMObject, Element element, System.Enum @enum)
        {
            return Name(setting, sAMObject?.GetType(), element?.GetType(), @enum);
        }

        public static string Name(Setting setting, System.Type type_1, System.Type type_2, string name)
        {
            if (setting == null || type_1 == null || type_2 == null || name == null)
                return null;
            
            MapCluster mapCluster;
            if (!ActiveSetting.Setting.TryGetValue(ActiveSetting.Name.ParameterMap, out mapCluster) || mapCluster == null)
                return null;

            return mapCluster.GetName(type_1, type_2, name);
        }
    }
}