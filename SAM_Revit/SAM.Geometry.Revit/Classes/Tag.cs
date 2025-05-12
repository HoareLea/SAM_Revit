using Newtonsoft.Json.Linq;
using SAM.Core;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public class Tag : ViewSpecificRevitInstance<TagType>
    {
        private LongId referenceId;
        private Planar.Point2D location;
        private Planar.Point2D elbow;
        private Planar.Point2D end;

        public Tag(Tag tag)
       : base(tag)
        {
        }

        public Tag(JObject jObject)
            : base(jObject)
        {

        }

        public Tag(TagType tagType, LongId viewId, Planar.Point2D location, LongId referenceId)
            : base(tagType, viewId)
        {
            this.location = location == null ? null : new Planar.Point2D(location);
            this.referenceId = referenceId == null ? null : new LongId(referenceId);
        }

        public Tag(TagType tagType, LongId viewId, Planar.Point2D location, Planar.Point2D elbow, Planar.Point2D end, LongId referenceId)
            : base(tagType, viewId)
        {
            this.location = location == null ? null : new Planar.Point2D(location);
            this.referenceId = referenceId == null ? null : new LongId(referenceId);
            this.elbow = elbow == null ? null : new Planar.Point2D(elbow);
            this.end = end == null ? null : new Planar.Point2D(end);
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

            if (jObject.ContainsKey("Elbow"))
            {
                elbow = new Planar.Point2D(jObject.Value<JObject>("Elbow"));
            }

            if (jObject.ContainsKey("End"))
            {
                end = new Planar.Point2D(jObject.Value<JObject>("End"));
            }

            if (jObject.ContainsKey("ReferenceId"))
            {
                referenceId = new LongId(jObject.Value<JObject>("ReferenceId"));
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

            if (elbow != null)
            {
                result.Add("Elbow", elbow.ToJObject());
            }

            if (end != null)
            {
                result.Add("End", end.ToJObject());
            }

            if (referenceId != null)
            {
                result.Add("ReferenceId", referenceId.ToJObject());
            }

            return result;
        }

        public LongId ReferenceId
        {
            get
            {
                return referenceId == null ? null : new LongId(referenceId);
            }
        }

        public Planar.Point2D Location
        {
            get
            {
                return location == null ? null : new Planar.Point2D(location);
            }
        }

        public Planar.Point2D Elbow
        {
            get
            {
                return elbow == null ? null : new Planar.Point2D(elbow);
            }
        }

        public Planar.Point2D End
        {
            get
            {
                return end == null ? null : new Planar.Point2D(end);
            }
        }
    }
}
