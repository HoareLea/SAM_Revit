using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public class RevitFilePreviewParameter : IJSAMObject
    {
        public string id;
        public string name;
        public string displayName;
        public string type;
        private string typeOfParameter;
        private string units;
        private string value;

        public RevitFilePreviewParameter(string id, string name, string displayName, string type, string typeOfParameter, string units, string value)
        {
            this.id = id;
            this.name = name;
            this.displayName = displayName;
            this.type = type;
            this.typeOfParameter = typeOfParameter;
            this.units = units;
            this.value = value;
        }
        
        public RevitFilePreviewParameter(JObject jObject)
        {
            FromJObject(jObject);
        }

        public string Id
        {
            get
            {
                return id;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string DisplayName
        {
            get
            {
                return displayName;
            }
        }

        public string Type
        {
            get
            {
                return type;
            }
        }

        public string TypeOfParameter
        {
            get
            {
                return typeOfParameter;
            }
        }

        public string NameOfParameter
        {
            get
            {
                string result = displayName;
                if (string.IsNullOrEmpty(result))
                    result = name;

                return result;
            }
        }

        public string Units
        {
            get
            {
                return units;
            }
        }

        public string Value
        {
            get
            {
                return value;
            }
        }

        public T GetValue<T>()
        {
            T result;
            if (!Core.Query.TryConvert(value, out result))
                result = default;

            return result;
        }

        public bool TryGetValue<T>(out T value)
        {
            value = default;
            if (!Core.Query.TryConvert(this.value, out value))
                return false;

            return true;
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            if (jObject.ContainsKey("Id"))
                id = jObject.Value<string>("Id");

            if (jObject.ContainsKey("Name"))
                id = jObject.Value<string>("Name");

            if (jObject.ContainsKey("DisplayName"))
                id = jObject.Value<string>("DisplayName");

            if (jObject.ContainsKey("Type"))
                id = jObject.Value<string>("Type");

            if (jObject.ContainsKey("TypeOfParameter"))
                id = jObject.Value<string>("TypeOfParameter");

            if (jObject.ContainsKey("Units"))
                id = jObject.Value<string>("Units");

            if (jObject.ContainsKey("Value"))
                id = jObject.Value<string>("Value");

            return true;

        }

        public JObject ToJObject()
        {
            JObject result = new JObject();
            result.Add("_type", Core.Query.FullTypeName(this));

            if (id != null)
                result.Add("Id", id);

            if (name != null)
                result.Add("Name", name);

            if (displayName != null)
                result.Add("DisplayName", displayName);

            if (type != null)
                result.Add("Type", type);

            if (typeOfParameter != null)
                result.Add("TypeOfParameter", typeOfParameter);

            if (units != null)
                result.Add("Units", units);

            if (value != null)
                result.Add("Value", value);

            return result;
        }
    }
}
