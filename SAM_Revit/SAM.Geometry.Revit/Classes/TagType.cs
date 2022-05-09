using Newtonsoft.Json.Linq;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public class TagType : RevitType
    {
        public TagType(TagType tagType)
            : base(tagType)
        {
        }

        public TagType(JObject jObject)
            : base(jObject)
        {
        }

        public TagType(string name)
            : base(name)
        {
        }
    }
}
