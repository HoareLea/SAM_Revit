using System.Text.Json.Nodes;
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

        public RevitInstance(JsonObject jObject)
            : base(jObject)
        {

        }

        public override bool FromJsonObject(JsonObject jObject)
        {
            if (!base.FromJsonObject(jObject))
                return false;

            return true;
        }

        public override JsonObject ToJsonObject()
        {
            JsonObject jObject = base.ToJsonObject();
            if (jObject == null)
                return jObject;

            return jObject;
        }

    }
}
