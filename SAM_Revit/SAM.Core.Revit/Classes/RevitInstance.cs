using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public class RevitInstance<T>: SAMInstance<T> where T: RevitType
    {
        public RevitInstance(RevitInstance<T> revitInstance)
            :base(revitInstance)
        {

        }

        public RevitInstance(T revitType)
            : base(revitType)
        {

        }

        public RevitInstance(JObject jObject)
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
