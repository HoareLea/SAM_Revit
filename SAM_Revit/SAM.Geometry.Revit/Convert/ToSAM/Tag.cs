using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using SAM.Core;
using SAM.Core.Revit;

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
            if(document == null)
            {
                return null;
            }

            Tag result = convertSettings?.GetObject<Tag>(spaceTag.Id);
            if (result != null)
            {
                return result;
            }

            TagType tagType = ToSAM(document.GetElement(spaceTag.GetTypeId()) as SpaceTagType, convertSettings);
            if(tagType == null)
            {
                return null;
            }

            ElementId elementId = spaceTag.OwnerViewId;
            if(elementId == null)
            {
                return null;
            }

            View view = document.GetElement(elementId) as View;
            if(view == null)
            {
                return null;
            }

            IntegerId viewId = Query.IntegerId(view);
            IntegerId referenceId = Query.IntegerId(spaceTag.Space);

            Spatial.Point3D point3D = ToSAM((spaceTag.Location as LocationPoint)?.Point);

            result = new Tag(tagType, viewId, new Planar.Point2D(point3D.X, point3D.Y), referenceId);
            if (result != null)
            {
                result.SetValue(ElementParameter.RevitId, Query.IntegerId(spaceTag));

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

            IntegerId viewId = Query.IntegerId(view);
            IntegerId referenceId = Query.IntegerId(document.GetElement(independentTag.GetTaggedReference()));

            XYZ xYZ = (independentTag.Location as LocationPoint)?.Point;
            if(xYZ == null)
            {
                xYZ = independentTag.TagHeadPosition;
            }

            if(xYZ == null)
            {
                return null;
            }

            Spatial.Point3D point3D = ToSAM(xYZ);
            if(point3D == null)
            {
                return null;
            }

            result = new Tag(tagType, viewId, new Planar.Point2D(point3D.X, point3D.Y), referenceId);
            if (result != null)
            {
                result.SetValue(ElementParameter.RevitId, Query.IntegerId(independentTag));

                Core.Revit.Modify.SetValues(independentTag, result);

                convertSettings?.Add(independentTag.Id, result);
            }

            return result;
        }
    }
}