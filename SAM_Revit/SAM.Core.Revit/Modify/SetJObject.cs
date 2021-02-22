using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool SetJObject(this SAMSchema sAMSchema, Element element, JObject jObject)
        {
            if (sAMSchema == null || element == null || jObject == null)
                return false;

            string fieldName = sAMSchema.FieldName;
            if (string.IsNullOrWhiteSpace(fieldName))
                return false;

            return SetJObject(sAMSchema.GetSchema(), element, jObject, fieldName);
        }

        public static bool SetJObject(this Schema schema, Element element, JObject jObject, string fieldName)
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