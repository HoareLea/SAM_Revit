using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Text.Json.Nodes;
namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool SetJsonObject(this SAMSchema sAMSchema, Element element, JsonObject jObject)
        {
            if (sAMSchema == null || element == null || jObject == null)
                return false;

            string fieldName = sAMSchema.FieldName;
            if (string.IsNullOrWhiteSpace(fieldName))
                return false;

            return SetJsonObject(sAMSchema.GetSchema(), element, jObject, fieldName);
        }

        public static bool SetJsonObject(this Schema schema, Element element, JsonObject jObject, string fieldName)
        {
            if (schema == null || element == null || jObject == null)
                return false;

            Entity entity = new Entity(schema);
            if (entity == null)
                return false;

            Field field = schema.GetField(fieldName);
            if (field == null)
                return false;

            entity.Set(field, jObject.ToString());
            element.SetEntity(entity);
            return true;
        }
    }
}
