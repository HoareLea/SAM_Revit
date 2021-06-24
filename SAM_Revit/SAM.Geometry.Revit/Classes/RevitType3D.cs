using Newtonsoft.Json.Linq;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public class RevitType3D : RevitType
    {
        public RevitType3D(RevitType3D revitType3D)
            :base(revitType3D)
        {

        }

        public RevitType3D(JObject jObject)
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
