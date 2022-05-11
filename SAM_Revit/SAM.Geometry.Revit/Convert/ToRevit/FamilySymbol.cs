using Autodesk.Revit.DB;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static FamilySymbol ToRevit(this TagType tagType, Document document, ConvertSettings convertSettings)
        {
            if(tagType == null || document == null)
            {
                return null;
            }

            FamilySymbol result = null;

            result = Core.Revit.Query.Element<FamilySymbol>(document, tagType, true);

            return result;
        }
    }
}