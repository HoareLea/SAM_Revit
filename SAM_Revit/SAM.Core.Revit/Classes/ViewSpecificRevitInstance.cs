using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public class ViewSpecificRevitInstance<T>: RevitInstance<T> where T: RevitType
    {
        public IntegerId viewId;

        public ViewSpecificRevitInstance(ViewSpecificRevitInstance<T> viewSpecificRevitInstance)
            :base(viewSpecificRevitInstance)
        {

        }

        public ViewSpecificRevitInstance(T revitType, IntegerId viewId)
            : base(revitType)
        {
            this.viewId = viewId == null ? null : new IntegerId(viewId);
        }

        public ViewSpecificRevitInstance(JObject jObject)
            : base(jObject)
        {

        }

        public override bool FromJObject(JObject jObject)
        {
            if (!base.FromJObject(jObject))
                return false;

            if (jObject.ContainsKey("ViewId"))
            {
                viewId = new IntegerId(jObject.Value<JObject>("ViewId"));
            }

            return true;
        }

        public override JObject ToJObject()
        {
            JObject result = base.ToJObject();
            if (result == null)
                return result;

            if (viewId != null)
            {
                result.Add("ViewId", viewId.ToJObject());
            }

            return result;
        }

        public IntegerId ViewId
        {
            get
            {
                if (viewId == null)
                {
                    return null;
                }

                return new IntegerId(viewId);
            }
        }


    }
}
