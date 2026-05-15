using System.Text.Json.Nodes;
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

        public RevitType(JsonObject jObject)
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
