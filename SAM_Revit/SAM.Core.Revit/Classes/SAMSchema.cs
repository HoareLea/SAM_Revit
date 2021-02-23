using Autodesk.Revit.DB.ExtensibleStorage;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public abstract class SAMSchema : SAMObject
    {
        private string vendorId = "SAM";
        private AccessLevel readAccessLevel = AccessLevel.Vendor;
        private AccessLevel writeAccessLevel = AccessLevel.Vendor;
        private string fieldName = "Data";
        private string fieldDocumentation = "SAM Data Field";

        public SAMSchema(Guid guid)
            : base(guid, "SAM")
        {

        }

        public SAMSchema(Guid guid, 
            string name, 
            string vendorId, 
            AccessLevel readAccessLevel, 
            AccessLevel writeAccessLevel,
            string fieldName,
            string fieldDocumentation)
            :base(guid, name)
        {
            this.vendorId = vendorId;
            this.readAccessLevel = readAccessLevel;
            this.writeAccessLevel = writeAccessLevel;
            this.fieldName = fieldName;
            this.fieldDocumentation = fieldDocumentation;
        }


        public SAMSchema(JObject jObject)
            : base(jObject)
        {
            FromJObject(jObject);
        }

        public string VendorId
        {
            get
            {
                return vendorId;
            }
        }

        public AccessLevel ReadAccessLevel
        {
            get
            {
                return readAccessLevel;
            }
        }

        public AccessLevel WriteAccesLevel
        {
            get
            {
                return writeAccessLevel;
            }
        }

        public string FieldName
        {
            get
            {
                return fieldName;
            }
        }

        public string FieldDocumentation
        {
            get
            {
                return fieldDocumentation;
            }
        }

        public virtual Schema GetSchema()
        {
            Schema result = Schema.Lookup(Guid);
            if (result != null)
                return result;

            if (Guid == System.Guid.Empty || string.IsNullOrWhiteSpace(Name))
                return null;

            SchemaBuilder schemaBuilder = new SchemaBuilder(Guid);
            schemaBuilder.SetReadAccessLevel(readAccessLevel);
            schemaBuilder.SetWriteAccessLevel(writeAccessLevel);
            schemaBuilder.SetVendorId(vendorId);

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                FieldBuilder fieldBuilder = schemaBuilder.AddSimpleField(fieldName, typeof(string));
                if(!string.IsNullOrWhiteSpace(fieldDocumentation))
                    fieldBuilder.SetDocumentation(fieldDocumentation);
            }

            schemaBuilder.SetSchemaName(Name);
            return schemaBuilder.Finish();
        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("VendorId"))
                vendorId = jObject.Value<string>("VendorId");

            if (jObject.ContainsKey("ReadAccessLevel"))
                readAccessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), jObject.Value<string>("ReadAccessLevel"));

            if (jObject.ContainsKey("WriteAccessLevel"))
                writeAccessLevel = (AccessLevel)Enum.Parse(typeof(AccessLevel), jObject.Value<string>("WriteAccessLevel"));

            if (jObject.ContainsKey("FieldName"))
                fieldName = jObject.Value<string>("FieldName");

            if (jObject.ContainsKey("FieldDocumentation"))
                fieldDocumentation = jObject.Value<string>("FieldDocumentation");

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            if (vendorId != null)
                jObject.Add("VendorId", vendorId);

            jObject.Add("ReadAccessLevel", readAccessLevel.ToString());
            jObject.Add("WriteAccessLevel", writeAccessLevel.ToString());


            if (fieldName != null)
                jObject.Add("FieldName", fieldName);

            if (fieldDocumentation != null)
                jObject.Add("FieldDocumentation", fieldDocumentation);

            return jObject;
        }

        public List<T> GetIJSAMObjects<T>(Autodesk.Revit.DB.Element element) where T : IJSAMObject
        {
            if (element == null || Guid == System.Guid.Empty || string.IsNullOrEmpty(fieldName))
                return null;

            Schema schema = GetSchema();
            if (schema == null)
                return null;

            Entity entity = element.GetEntity(schema);
            if (entity.Schema == null)
                return null;

            string json = entity.Get<string>(fieldName);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            JArray jArray = Core.Query.JArray(json);
            if (jArray == null)
                return null;

            return Core.Create.IJSAMObjects<T>(jArray);
        }

        public T GetIJSAMObject<T>(Autodesk.Revit.DB.Element element) where T : IJSAMObject
        {
            List<T> jSAMObjects = GetIJSAMObjects<T>(element);
            if (jSAMObjects == null || jSAMObjects.Count == 0)
                return default(T);

            return jSAMObjects[0];
        }

        public bool SetIJSAMObject(Autodesk.Revit.DB.Element element, IJSAMObject jSAMObject)
        {
            if (element == null || jSAMObject == null || string.IsNullOrEmpty(fieldName))
                return false;

            JObject jObject = jSAMObject.ToJObject();
            if (jObject == null)
                return false;

            return Modify.SetJObject(this, element, jObject);
        }

        public List<bool> SetIJSAMObjects(Autodesk.Revit.DB.Element element, IEnumerable<IJSAMObject> jSAMObjects)
        {
            if (element == null || jSAMObjects == null)
                return null;

            List<bool> result = new List<bool>();
            JArray jArray = new JArray();
            foreach(IJSAMObject iJSAMObject in jSAMObjects)
            {
                if (iJSAMObject == null)
                {
                    result.Add(false);
                    continue;
                }

                JObject jObject = iJSAMObject.ToJObject();
                if (jObject == null)
                {
                    result.Add(false);
                    continue;
                }

                jArray.Add(jObject);
                result.Add(true);
            }

            if (!Modify.SetJArray(this, element, jArray))
                return new List<bool>();

            return result;
        }

        public List<bool> AppendIJSAMObjects(Autodesk.Revit.DB.Element element, IEnumerable<IJSAMObject> jSAMObjects)
        {
            if (element == null || jSAMObjects == null)
                return null;

            if (jSAMObjects.Count() == 0)
                return new List<bool>();
            
            List<IJSAMObject> iJSAMObjects = GetIJSAMObjects<IJSAMObject>(element);
            if (iJSAMObjects == null)
                iJSAMObjects = new List<IJSAMObject>();

            iJSAMObjects.AddRange(jSAMObjects);

            return SetIJSAMObjects(element, iJSAMObjects);
        }

        public List<T> RemoveIJSAMObjects<T>(Autodesk.Revit.DB.Element element) where T : IJSAMObject
        {
            if (element == null)
                return null;

            List<IJSAMObject> jSAMObjects = GetIJSAMObjects<IJSAMObject>(element);
            if (jSAMObjects == null)
                return null;

            List<T> result = new List<T>();

            if (jSAMObjects.Count == 0)
                return result;

            List<IJSAMObject> jSAMObjects_Temp = new List<IJSAMObject>();
            foreach(IJSAMObject jSAMObject in jSAMObjects)
            {
                if (jSAMObject == null)
                    continue;
                
                if(jSAMObject is T)
                {
                    result.Add((T)jSAMObject);
                    continue;
                }

                jSAMObjects_Temp.Add(jSAMObject);
            }

            SetIJSAMObjects(element, jSAMObjects_Temp);
            return result;
        }

        public bool Clear(Autodesk.Revit.DB.Element element)
        {
            if (element == null)
                return false;

            Schema schema = GetSchema();
            if (schema == null)
                return false;

            Entity entity = new Entity(schema);
            if (entity == null)
                return false;
            
            Field field = schema.GetField(FieldName);
            if (field == null)
                return false;

            entity.Clear(field);
            return true;
        }
    }
}