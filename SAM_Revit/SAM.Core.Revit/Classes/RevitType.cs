using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public class RevitType: SAMType
    {
        public RevitType(RevitType revitType)
            :base(revitType)
        {

        }

        public RevitType(string name)
            : base(name)
        {

        }

        public RevitType(JObject jObject)
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
