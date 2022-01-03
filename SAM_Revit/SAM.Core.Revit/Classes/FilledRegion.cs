using Newtonsoft.Json.Linq;

namespace SAM.Core.Revit
{
    public class FilledRegion : RevitInstance<FilledRegionType>
    {
        public FilledRegion(FilledRegion filledRegion) 
            : base(filledRegion)
        {
        }

        public FilledRegion(JObject jObject)
            : base(jObject)
        {
        }
    }
}
