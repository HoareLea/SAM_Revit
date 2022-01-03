using Newtonsoft.Json.Linq;

namespace SAM.Geometry.Revit
{
    public class FilledRegionType : Core.Revit.RevitType
    {
        public FilledRegionType(FilledRegionType filledRegionType) 
            : base(filledRegionType)
        {
        }

        public FilledRegionType(JObject jObject)
            : base(jObject)
        {
        }
    }
}
