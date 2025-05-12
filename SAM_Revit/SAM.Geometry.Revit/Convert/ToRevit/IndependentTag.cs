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

            Core.LongId longId_Reference = tag.ReferenceId;
            if (longId_Reference == null)
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

            Core.LongId longId_View = tag.ViewId;
            if (longId_View == null)
            {
                return null;
            }

            View view = Core.Revit.Query.Element<View>(document, longId_View, true);
            if(view == null)
            {
                return null;
            }

            Element element = Query.Find<Element>(document, longId_Reference);
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

#if Revit2017
            IndependentTag result = null;
#elif Revit2018
            IndependentTag result = IndependentTag.Create(document, view.Id, new Reference(element), leader, TagMode.TM_ADDBY_CATEGORY, tagOrientation, new XYZ(uV.U, uV.V, 0));
            result.ChangeTypeId(familySymbol.Id);
#else
            IndependentTag result = IndependentTag.Create(document, familySymbol.Id, view.Id, new Reference(element), leader, tagOrientation, new XYZ(uV.U, uV.V, 0));
#endif
            if (leader)
            {
                UV elbow = tag.Elbow?.ToRevit();
                if (elbow != null)
                {
#if Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022
                    result.LeaderElbow = new XYZ(elbow.U, elbow.V, 0);
#else
                    result.SetLeaderElbow(new Reference(familySymbol), new XYZ(elbow.U, elbow.V, 0));
#endif
                }

                UV end = tag.End?.ToRevit();
                if (end != null)
                {
#if Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022
                    result.LeaderEnd = new XYZ(end.U, end.V, 0);
#else
                    result.SetLeaderEnd(new Reference(familySymbol), new XYZ(elbow.U, elbow.V, 0));
#endif
                }
            }

            return result;
        }
    }
}