using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Face3D> ToSAM(this Autodesk.Revit.DB.Face face)
        {
            return Spatial.Create.Face3Ds(face.ToSAM_Polygon3Ds(), false);
        }

        public static List<Face3D> ToSAM_Face3Ds(this Sketch sketch, bool flip = false)
        {
            if (sketch == null)
                return null;

            CurveArrArray profile = null;
            try
            {
                profile = sketch.Profile;
            }
            catch
            {
                profile = null;
            }

            if (profile == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            foreach (CurveArray curveArray in sketch.Profile)
                result.Add(curveArray.ToSAM_Face3D(flip));

            return result;
        }

        public static List<Face3D> ToSAM_Face3Ds(this Element element)
        {
            Transform transform = null;
            if (element is FamilyInstance)
                transform = ((FamilyInstance)element).GetTotalTransform();

            Options options = new Options();
            options.ComputeReferences = false;
            options.DetailLevel = ViewDetailLevel.Fine;

            return ToSAM_Face3Ds(element.get_Geometry(options), transform);
        }

        public static List<Face3D> ToSAM_Face3Ds(this GeometryElement geometryElement, Transform transform = null)
        {
            if (geometryElement == null)
                return null;

            List<Face3D> result = new List<Face3D>();
            foreach (GeometryObject geometryObject in geometryElement)
            {
                if (geometryObject is GeometryInstance)
                {
                    GeometryInstance geometryInstance = (GeometryInstance)geometryObject;

                    Transform geometryTransform = geometryInstance.Transform;
                    if (transform != null)
                        geometryTransform = geometryTransform.Multiply(transform.Inverse);

                    GeometryElement geometryElement_Temp = geometryInstance.GetInstanceGeometry(geometryTransform);
                    if (geometryElement_Temp == null)
                        continue;

                    List<Face3D> face3Ds = ToSAM_Face3Ds(geometryElement_Temp);
                    if (face3Ds != null && face3Ds.Count > 0)
                        result.AddRange(face3Ds);
                }
                else if (geometryObject is Solid)
                {
                    Solid solid = (Solid)geometryObject;
                    FaceArray faceArray = solid.Faces;
                    if (faceArray == null)
                        continue;

                    foreach (Autodesk.Revit.DB.Face face in faceArray)
                    {
                        List<Face3D> face3Ds = face.ToSAM();
                        if (face3Ds != null && face3Ds.Count != 0)
                            result.AddRange(face3Ds);
                    }
                }
            }
            return result;
        }
    }
}