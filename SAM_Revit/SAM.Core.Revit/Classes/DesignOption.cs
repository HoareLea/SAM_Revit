using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public class DesignOption: SAMObject
    {
        private bool isPrimary;

        public DesignOption(DesignOption designOption)
            :base(designOption)
        {

        }

        public DesignOption(string name, bool isPrimary)
            : base(name)
        {
            this.isPrimary = isPrimary;
        }

        public DesignOption(JObject jObject)
            : base(jObject)
        {

        }

        public bool IsPrimary
        {
            get
            {
                return isPrimary;
            }
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
