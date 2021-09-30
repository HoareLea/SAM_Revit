using Autodesk.Revit.DB;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static List<T> IJSAMObjects<T>(this Element element) where T : IJSAMObject
        {
            if (element == null)
                return default;

            JToken jToken = element.JToken();
            if (jToken == null)
                return default;

            switch(jToken.Type)
            {
                case JTokenType.Object:
                    T t = Core.Create.IJSAMObject<T>(jToken as JObject);
                    if(t != null)
                    {
                        return new List<T>() { t };
                    }
                    break;

                case JTokenType.Array:
                    return Core.Create.IJSAMObjects<T>(jToken as JArray);
            }

            return null;
        }
    }
}