using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static T IJSAMObject<T>(this Element element) where T: IJSAMObject
        {
            if (element == null)
                return default;

            JObject jObject = element.JObject();
            if (jObject == null)
                return default;

            return Create.IJSAMObject<T>(jObject);
        }
    }
}
