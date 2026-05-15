using Autodesk.Revit.DB;
using System.Text.Json.Nodes;
namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static JsonNode JsonNode(this Element element)
        {
            if (element == null)
                return null;

            string json = element.Json();
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return System.Text.Json.Nodes.JsonNode.Parse(json);
        }
    }
}
