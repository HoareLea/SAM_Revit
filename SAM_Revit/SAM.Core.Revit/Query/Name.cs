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
    }
}