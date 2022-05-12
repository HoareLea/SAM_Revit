using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public class Tag : ViewSpecificRevitInstance<TagType>
    {
        private IntegerId referenceId;
        private Planar.Point2D location;

        public Tag(Tag tag)
       : base(tag)
        {
        }

        public Tag(JObject jObject)
            : base(jObject)
        {

        }

        public Tag(TagType tagType, IntegerId viewId, Planar.Point2D location, IntegerId referenceId)
            :base(tagType, viewId)
        {
            this.location = location;
            this.referenceId = referenceId;
        }

        public override bool FromJObject(JObject jObject)
        {
            if(!base.FromJObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Location"))
            {
                location = new Planar.Point2D(jObject.Value<JObject>("Location"));
            }

            if (jObject.ContainsKey("ReferenceId"))
            {
                referenceId = new IntegerId(jObject.Value<JObject>("ReferenceId"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if(result == null)
            {
                return null;
            }

            if(location != null)
            {
                result.Add("Location", location.ToJObject());
            }

            if (referenceId != null)
            {
                result.Add("ReferenceId", referenceId.ToJObject());
            }

            return result;
        }

        public IntegerId ReferenceId
        {
            get
            {
                return referenceId == null ? null : new IntegerId(referenceId);
            }
        }

        public Planar.Point2D Location
        {
            get
            {
                return location == null ? null : new Planar.Point2D(location);
            }
        }
    }
}
