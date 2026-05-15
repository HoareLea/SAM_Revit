using System.Text.Json.Nodes;
namespace SAM.Geometry.Revit
{
    public class FilledRegionType : Core.Revit.RevitType
    {
        public FilledRegionType(FilledRegionType filledRegionType) 
            : base(filledRegionType)
        {
        }

        public FilledRegionType(JsonObject jObject)
            : base(jObject)
        {
        }

        public FilledRegionType(string name)
            : base(name)
        {
        }
    }
}
