using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static JObject JObject(this Element element)
        {
            if (element == null)
                return null;

            string json = element.Json();
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JToken.Parse(json) as JObject;
        }
    }
}
