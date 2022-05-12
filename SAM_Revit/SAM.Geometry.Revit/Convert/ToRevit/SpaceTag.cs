using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using SAM.Core.Revit;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static SpaceTag ToRevit_SpaceTag(this Tag tag, Document document, ConvertSettings convertSettings)
        {
            if(tag == null || document == null)
            {
                return null;
            }

            Core.IntegerId integerId_Reference = tag.ReferenceId;
            if (integerId_Reference == null)
            {
                return null;
            }

            if(tag.BuiltInCategory() != BuiltInCategory.OST_MEPSpaceTags)
            {
                return null;
            }

            FamilySymbol familySymbol = tag.Type?.ToRevit(document, convertSettings);
            if (familySymbol == null)
            {
                return null;
            }

            Core.IntegerId integerId_View = tag.ViewId;
            if (integerId_View == null)
            {
                return null;
            }

            View view = Core.Revit.Query.Element<View>(document, integerId_View, true);
            if(view == null)
            {
                return null;
            }

            Space space = Query.Find<Space>(document, integerId_Reference);
            if(space == null)
            {
                return null;
            }

            Planar.Point2D point2D = tag.Location;
            if(point2D == null)
            {
                return null;
            }

            UV uV = point2D.ToRevit();

            SpaceTag result = document.Create.NewSpaceTag(space, uV, view);
            result.ChangeTypeId(familySymbol.Id);

            return result;
        }
    }
}