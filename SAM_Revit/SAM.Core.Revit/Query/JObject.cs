using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static JToken JToken(this Element element)
        {
            if (element == null)
                return null;

            string json = element.Json();
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return Newtonsoft.Json.Linq.JToken.Parse(json);
        }
    }
}