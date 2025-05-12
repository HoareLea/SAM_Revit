using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using SAM.Core;
using SAM.Core.Revit;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static Tag ToSAM(this SpaceTag spaceTag, ConvertSettings convertSettings)
        {
            if (spaceTag == null)
            {
                return null;
            }

            Document document = spaceTag.Document;
            if (document == null)
            {
                return null;
            }

            Tag result = convertSettings?.GetObject<Tag>(spaceTag.Id);
            if (result != null)
            {
                return result;
            }

            TagType tagType = ToSAM(document.GetElement(spaceTag.GetTypeId()) as SpaceTagType, convertSettings);
            if (tagType == null)
            {
                return null;
            }

            ElementId elementId = spaceTag.OwnerViewId;
            if (elementId == null)
            {
                return null;
            }

            View view = document.GetElement(elementId) as View;
            if (view == null)
            {
                return null;
            }

            LongId viewId = Query.LongId(view);
            LongId referenceId = Query.LongId(spaceTag.Space);

            Spatial.Point3D location = ToSAM(spaceTag.TagHeadPosition);
            if (location == null)
            {
                return null;
            }

            Planar.Point2D elbow = null;
            Planar.Point2D end = null;
            if (spaceTag.HasLeader)
            {
#if Revit2017

#else
                if (spaceTag.HasElbow)
                {
                    Spatial.Point3D elbow3D = ToSAM(spaceTag.LeaderElbow);
                    if (elbow3D != null)
                    {
                        elbow = new Planar.Point2D(elbow3D.X, elbow3D.Y);
                    }
                }
#endif

                Spatial.Point3D end3D = ToSAM(spaceTag.LeaderEnd);
                if (end3D != null)
                {
                    end = new Planar.Point2D(end3D.X, end3D.Y);
                }
            }

            result = new Tag(tagType, viewId, new Planar.Point2D(location.X, location.Y), elbow, end, referenceId);
            if (result != null)
            {
                result.SetValue(ElementParameter.RevitId, Query.LongId(spaceTag));
                result.SetValue(TagParameter.Leader, spaceTag.HasLeader);
                result.SetValue(TagParameter.Orientation, spaceTag.TagOrientation.ToString());

                Core.Revit.Modify.SetValues(spaceTag, result);

                convertSettings?.Add(spaceTag.Id, result);
            }

            return result;
        }

        public static Tag ToSAM(this IndependentTag independentTag, ConvertSettings convertSettings)
        {
            if (independentTag == null)
            {
                return null;
            }

            Document document = independentTag.Document;
            if (document == null)
            {
                return null;
            }

            Tag result = convertSettings?.GetObject<Tag>(independentTag.Id);
            if (result != null)
            {
                return result;
            }

            TagType tagType = ToSAM_TagType(document.GetElement(independentTag.GetTypeId()) as FamilySymbol, convertSettings);
            if (tagType == null)
            {
                return null;
            }

            ElementId elementId = independentTag.OwnerViewId;
            if (elementId == null)
            {
                return null;
            }

            View view = document.GetElement(elementId) as View;
            if (view == null)
            {
                return null;
            }

            LongId viewId = Query.LongId(view);

#if Revit2017
            LongId referenceId = null;
            throw new System.NotImplementedException();
#elif Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022
            LongId referenceId = Query.LongId(document.GetElement(independentTag.GetTaggedReference()));
#else
            Autodesk.Revit.DB.Reference reference = independentTag.GetTaggedReferences().FirstOrDefault();
            if(reference == null)
            {
                return null;
            }

            LongId referenceId = Query.LongId(document.GetElement(reference));
#endif

            if (referenceId == null)
            {
                return null;
            }

            Spatial.Point3D location = ToSAM(independentTag.TagHeadPosition);
            if (location == null)
            {
                return null;
            }

            Planar.Point2D elbow = null;
            Planar.Point2D end = null;
            if (independentTag.HasLeader)
            {
#if Revit2017

#elif Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022
                if (independentTag.HasElbow)
                {
                    Spatial.Point3D elbow3D = ToSAM(independentTag.LeaderElbow);
                    if (elbow3D != null)
                    {
                        elbow = new Planar.Point2D(elbow3D.X, elbow3D.Y);
                    }
                }

#else
                if (independentTag.HasLeaderElbow(reference))
                {
                    Spatial.Point3D elbow3D = ToSAM(independentTag.GetLeaderElbow(reference));
                    if (elbow3D != null)
                    {
                        elbow = new Planar.Point2D(elbow3D.X, elbow3D.Y);
                    }
                }
#endif


#if Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022
                if (independentTag.LeaderEndCondition == LeaderEndCondition.Free)
                {
                    Spatial.Point3D end3D = ToSAM(independentTag.LeaderEnd);
                    if (end3D != null)
                    {
                        end = new Planar.Point2D(end3D.X, end3D.Y);
                    }
                }
#else
                if (independentTag.LeaderEndCondition == LeaderEndCondition.Free)
                {
                    Spatial.Point3D end3D = ToSAM(independentTag.GetLeaderEnd(reference));
                    if (end3D != null)
                    {
                        end = new Planar.Point2D(end3D.X, end3D.Y);
                    }
                }
#endif
            }

            result = new Tag(tagType, viewId, new Planar.Point2D(location.X, location.Y), elbow, end, referenceId);
            if (result != null)
            {
                result.SetValue(ElementParameter.RevitId, Query.LongId(independentTag));
                result.SetValue(TagParameter.Leader, independentTag.HasLeader);
                result.SetValue(TagParameter.Orientation, independentTag.TagOrientation.ToString());

                Core.Revit.Modify.SetValues(independentTag, result);

                convertSettings?.Add(independentTag.Id, result);
            }

            return result;
        }
    }
}