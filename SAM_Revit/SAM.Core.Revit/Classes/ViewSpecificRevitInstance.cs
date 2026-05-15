using System.Text.Json.Nodes;
namespace SAM.Core.Revit
{
    public class ViewSpecificRevitInstance<T>: RevitInstance<T> where T: RevitType
    {
        public LongId viewId;

        public ViewSpecificRevitInstance(ViewSpecificRevitInstance<T> viewSpecificRevitInstance)
            :base(viewSpecificRevitInstance)
        {

        }

        public ViewSpecificRevitInstance(T revitType, LongId viewId)
            : base(revitType)
        {
            this.viewId = viewId == null ? null : new LongId(viewId);
        }

        public ViewSpecificRevitInstance(JsonObject jObject)
            : base(jObject)
        {

        }

        public override bool FromJsonObject(JsonObject jObject)
        {
            if (!base.FromJsonObject(jObject))
                return false;

            if (jObject.ContainsKey("ViewId"))
            {
                viewId = new LongId(jObject["ViewId"] as JsonObject);
            }

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject result = base.ToJsonObject();
            if (result == null)
                return result;

            if (viewId != null)
            {
                result.Add("ViewId", viewId.ToJsonObject());
            }

            return result;
        }

        public LongId ViewId
        {
            get
            {
                if (viewId == null)
                {
                    return null;
                }

                return new LongId(viewId);
            }
        }


    }
}
