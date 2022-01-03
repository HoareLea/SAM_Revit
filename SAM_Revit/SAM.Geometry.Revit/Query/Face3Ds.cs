using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<Face3D> Face3Ds(this HostObject hostObject, ElementId generatingElementId)
        {
            List<Autodesk.Revit.DB.Face> faces = Faces(hostObject, generatingElementId);
            if(faces == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();
            foreach(Autodesk.Revit.DB.Face face in faces)
            {
                List<Face3D> face3Ds = face?.ToSAM();
                if(face3Ds == null)
                {
                    continue;
                }

                result.AddRange(face3Ds);
            }

            return result;
        }

        public static List<Face3D> Face3Ds(this Autodesk.Revit.DB.FilledRegion filledRegion, double tolerance = Core.Tolerance.Distance)
        {
            if(filledRegion == null)
            {
                return null;
            }

            IList<CurveLoop> curveLoops = filledRegion.GetBoundaries();
            if(curveLoops == null)
            {
                return null;
            }

            View view = filledRegion.Document.GetElement(filledRegion.OwnerViewId) as View;
            if(view == null)
            {
                return null;
            }

            Vector3D normal = view.ViewDirection.ToSAM_Vector3D(false);
            if(normal == null)
            {
                return null;
            }

            Spatial.Plane plane = view.Plane();
            List<Planar.Polygon2D> polygon2Ds = new List<Planar.Polygon2D>();
            foreach(CurveLoop curveLoop in curveLoops)
            {
                Polygon3D polygon3D = curveLoop.ToSAM_Polygon3D(view.ViewDirection, tolerance);
                if(polygon3D == null)
                {
                    continue;
                }

                polygon2Ds.Add(plane.Convert(polygon3D));
            }

            List<Planar.Face2D> face2Ds = Planar.Create.Face2Ds(polygon2Ds, tolerance);
            if(face2Ds == null)
            {
                return null;
            }

            return face2Ds.ConvertAll(x => plane.Convert(x));
        }
    }
}