using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static bool SetJArray(this SAMSchema sAMSchema, Element element, JArray jArray)
        {
            if (sAMSchema == null || element == null || jArray == null)
                return false;

            string fieldName = sAMSchema.FieldName;
            if (string.IsNullOrWhiteSpace(fieldName))
                return false;

            return SetJArray(sAMSchema.GetSchema(), element, jArray, fieldName);
        }

        public static bool SetJArray(this Schema schema, Element element, JArray jArray, string fieldName)
        {
            if (schema == null || element == null || jArray == null)
                return false;

            Entity entity = new Entity(schema);
            if (entity == null)
                return false;

            Field field = schema.GetField(fieldName);
            if (field == null)
                return false;

            entity.Set(field, jArray.ToString());
            element.SetEntity(entity);
            return true;
        }
    }
}