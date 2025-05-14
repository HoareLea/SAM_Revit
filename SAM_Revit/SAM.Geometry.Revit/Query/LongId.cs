using Autodesk.Revit.DB;
using SAM.Core;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static LongId LongId(this Element element)
        {
            if (element == null)
                return null;

            LongId result = Core.Revit.Query.LongId(element);
            if(result == null)
            {
                return result;
            }

            ISAMGeometry sAMGeometry = Location(element);
            if(sAMGeometry != null)
            {
                result.SetValue(RevitIdParameter.Location, sAMGeometry);
            }

            return result;
        }
    }
}