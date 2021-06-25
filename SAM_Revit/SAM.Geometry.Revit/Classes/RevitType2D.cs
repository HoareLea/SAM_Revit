using Newtonsoft.Json.Linq;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public class RevitType2D : RevitType
    {
        public RevitType2D(RevitType2D revitType2D)
            :base(revitType2D)
        {

        }

        public RevitType2D(string name)
            : base(name)
        {

        }

        public RevitType2D(JObject jObject)
            : base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return jObject;

            return jObject;
        }
    }
}
