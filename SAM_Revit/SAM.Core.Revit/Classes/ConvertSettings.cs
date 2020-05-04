using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public class ConvertSettings : IJSAMObject
    {
        private bool convertGeometry;
        private bool convertParameters;
        private bool removeExisting;

        public ConvertSettings(bool convertGeometry, bool convertParameters, bool removeExisting)
        {
            this.convertGeometry = convertGeometry;
            this.convertParameters = convertParameters;
            this.removeExisting = removeExisting;
        }

        public ConvertSettings(ConvertSettings convertSettings)
        {
            convertGeometry = convertSettings.convertGeometry;
            convertParameters = convertSettings.convertParameters;
            removeExisting = convertSettings.removeExisting;
        }

        public bool ConvertGeometry
        {
            get
            {
                return convertGeometry;
            }
        }

        public bool ConvertParameters
        {
            get
            {
                return convertParameters;
            }
        }

        public bool RemoveExisting
        {
            get
            {
                return removeExisting;
            }
        }

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            convertGeometry = jObject.Value<bool>("ConvertGeometry");
            convertParameters = jObject.Value<bool>("ConvertParameters");
            removeExisting = jObject.Value<bool>("RemoveExisting");
            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));
            jObject.Add("ConvertGeometry", convertGeometry);
            jObject.Add("ConvertParameters", convertParameters);
            jObject.Add("RemoveExisting", removeExisting);

            return jObject;
        }
    }
}