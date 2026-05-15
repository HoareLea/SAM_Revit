using System.Text.Json.Nodes;
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

        public DesignOption(JsonObject jObject)
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
