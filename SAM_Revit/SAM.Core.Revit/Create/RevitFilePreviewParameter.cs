using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SAM.Core.Revit
{
    public static partial class Create
    {
        public static RevitFilePreviewParameter RevitFilePreviewParameter(this XElement xElement)
        {
            if (xElement == null)
                return null;

            string value = xElement.Value;
            string name = xElement.Name?.LocalName;
            string id = null;
            string displayName = null;
            string type = null;
            string typeOfParameter = null;
            string units = null;

            List<XAttribute> attributes = xElement?.Attributes()?.ToList();
            if (attributes != null && attributes.Count() > 0)
            {
                XAttribute xAttribute = null;

                xAttribute = attributes.Find(x => ("displayName").Equals(x.Name?.LocalName));
                if (xAttribute != null)
                    displayName = xAttribute.Value;

                xAttribute = attributes.Find(x => ("id").Equals(x.Name?.LocalName));
                if (xAttribute != null)
                    id = xAttribute.Value;

                xAttribute = attributes.Find(x => ("type").Equals(x.Name?.LocalName));
                if (xAttribute != null)
                    type = xAttribute.Value;

                xAttribute = attributes.Find(x => ("typeOfParameter").Equals(x.Name?.LocalName));
                if (xAttribute != null)
                    typeOfParameter = xAttribute.Value;

                xAttribute = attributes.Find(x => ("units").Equals(x.Name?.LocalName));
                if (xAttribute != null)
                    units = xAttribute.Value;
            }

            return new RevitFilePreviewParameter(id, name, displayName, type,typeOfParameter, units, value);
        }
    }
}