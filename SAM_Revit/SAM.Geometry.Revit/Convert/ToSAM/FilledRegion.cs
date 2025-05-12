using Autodesk.Revit.DB;
using SAM.Core;
using SAM.Core.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static FilledRegion ToSAM(this Autodesk.Revit.DB.FilledRegion filledRegion, ConvertSettings convertSettings)
        {
            if (filledRegion == null || !filledRegion.IsValidObject)
            {
                return null;
            }

            FilledRegion result = convertSettings?.GetObject<FilledRegion>(filledRegion.Id);
            if (result != null)
            {
                return result;
            }

            Document document = filledRegion.Document;
            if (document == null)
            {
                return result;
            }

            Autodesk.Revit.DB.FilledRegionType filledRegionType_Revit = document.GetElement(filledRegion.GetTypeId()) as Autodesk.Revit.DB.FilledRegionType;
            if (filledRegionType_Revit == null)
            {
                return null;
            }

            View view = filledRegion.Document.GetElement(filledRegion.OwnerViewId) as View;
            if(view == null)
            {
                return null;
            }

            Spatial.Plane plane = view.Plane();
            if(plane == null)
            {
                return null;
            }

            List<Face3D> face3Ds = filledRegion.Face3Ds();
            if (face3Ds == null)
            {
                return null;
            }

            FilledRegionType filledRegionType = filledRegionType_Revit.ToSAM(convertSettings);
            List<Planar.Face2D> face2Ds = face3Ds.ConvertAll(x => Spatial.Query.Convert(plane, x));
            LongId longId = Query.LongId(view);

            result = new FilledRegion(filledRegionType, longId, face2Ds);

            if (result != null)
            {
                result.UpdateParameterSets(filledRegion, ActiveSetting.Setting.GetValue<TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
                convertSettings?.Add(filledRegion.Id, result);
            }

            return result;
        }
    }
}