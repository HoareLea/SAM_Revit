using System.Text.Json.Nodes;
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

        public Tag(JsonObject jObject)
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

        public override bool FromJsonObject(JsonObject jObject)
        {
            if(!base.FromJsonObject(jObject))
            {
                return false;
            }

            if (jObject.ContainsKey("Location"))
            {
                location = new Planar.Point2D(jObject["Location"] as JsonObject);
            }

            if (jObject.ContainsKey("Elbow"))
            {
                elbow = new Planar.Point2D(jObject["Elbow"] as JsonObject);
            }

            if (jObject.ContainsKey("End"))
            {
                end = new Planar.Point2D(jObject["End"] as JsonObject);
            }

            if (jObject.ContainsKey("ReferenceId"))
            {
                referenceId = new LongId(jObject["ReferenceId"] as JsonObject);
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if(result == null)
            {
                return null;
            }

            if(location != null)
            {
                result.Add("Location", location.ToJsonObject());
            }

            if (elbow != null)
            {
                result.Add("Elbow", elbow.ToJsonObject());
            }

            if (end != null)
            {
                result.Add("End", end.ToJsonObject());
            }

            if (referenceId != null)
            {
                result.Add("ReferenceId", referenceId.ToJsonObject());
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
