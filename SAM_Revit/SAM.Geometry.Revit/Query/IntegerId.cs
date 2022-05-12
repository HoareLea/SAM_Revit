using Autodesk.Revit.DB;
using SAM.Core;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static IntegerId IntegerId(this Element element)
        {
            if (element == null)
                return null;

            IntegerId result = Core.Revit.Query.IntegerId(element);
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