using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core.Revit
{
    public class ConvertSettings : IJSAMObject
    {
        private bool convertGeometry;
        private bool convertParameters;

        public ConvertSettings(bool convertGeometry, bool convertParameters)
        {
            this.convertGeometry = convertGeometry;
            this.convertParameters = convertParameters;
        }

        public ConvertSettings(ConvertSettings convertSettings)
        {
            convertGeometry = convertSettings.convertGeometry;
            convertParameters = convertSettings.convertParameters;
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

        public bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            convertGeometry = jObject.Value<bool>("ConvertGeometry");
            convertParameters = jObject.Value<bool>("ConvertParameters");
            return true;
        }

        public JObject ToJObject()
        {
            JObject jObject = new JObject();
            jObject.Add("_type", Core.Query.FullTypeName(this));
            jObject.Add("ConvertGeometry", convertGeometry);
            jObject.Add("ConvertParameters", convertParameters);

            return jObject;
        }
    }
}
