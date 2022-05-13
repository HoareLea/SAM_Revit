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

            UV location = tag.Location?.ToRevit();
            if(location == null)
            {
                return null;
            }

            SpaceTag result = document.Create.NewSpaceTag(space, location, view);
            if(tag.TryGetValue(TagParameter.Leader, out bool leader) && leader)
            {
                result.HasLeader = leader;

                //XYZ xYZ = (result.Location as LocationPoint)?.Point;
                //if(xYZ != null)
                //{
                //    ElementTransformUtils.MoveElement(document, result.Id, new XYZ(location.U - xYZ.X, location.V - xYZ.Y, 0));
                //}

                UV elbow = tag.Elbow?.ToRevit();
                if(elbow != null)
                {
                    result.LeaderElbow = new XYZ(elbow.U, elbow.V, 0);
                }

                UV end = tag.End?.ToRevit();
                if (end != null)
                {
                    result.LeaderEnd = new XYZ(end.U, end.V, 0);
                }
            }

            if(tag.TryGetValue(TagParameter.Orientation, out string orientationText) && !string.IsNullOrWhiteSpace(orientationText))
            {
                if(System.Enum.TryParse(orientationText, out SpatialElementTagOrientation spatialElementTagOrientation) && result.TagOrientation != spatialElementTagOrientation)
                {
                    result.TagOrientation = spatialElementTagOrientation;
                }
            }

            result.ChangeTypeId(familySymbol.Id);

            return result;
        }
    }
}