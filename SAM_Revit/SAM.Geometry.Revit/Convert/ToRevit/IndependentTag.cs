using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static IndependentTag ToRevit(this Tag tag, Document document, ConvertSettings convertSettings)
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

            if(tag.BuiltInCategory() == BuiltInCategory.OST_MEPSpaceTags)
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

            Element element = Query.Find<Element>(document, integerId_Reference);
            if(element == null)
            {
                return null;
            }

            if(!tag.TryGetValue(TagParameter.Leader, out bool leader))
            {
                leader = false;
            }

            TagOrientation tagOrientation = TagOrientation.Horizontal;
            if (tag.TryGetValue(TagParameter.Orientation, out string orientation))
            {
                Enum.TryParse(orientation, out tagOrientation);
            }

            Planar.Point2D point2D = tag.Location;
            if(point2D == null)
            {
                return null;
            }

            UV uV = point2D.ToRevit();

            IndependentTag result = IndependentTag.Create(document, familySymbol.Id, view.Id, new Reference(element), leader, tagOrientation, new XYZ(uV.U, uV.V, 0));

            UV elbow = tag.Elbow?.ToRevit();
            if(elbow != null)
            {
                result.LeaderElbow = new XYZ(elbow.U, elbow.V, 0);
            }

            return result;
        }
    }
}