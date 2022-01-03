using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public class FilledRegionType : RevitType
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
