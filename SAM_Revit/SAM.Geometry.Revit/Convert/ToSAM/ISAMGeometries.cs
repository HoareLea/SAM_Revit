using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<T> ToSAM_Geometries<T>(this Element element) where T: ISAMGeometry
        {
            Transform transform = null;
            if (element is FamilyInstance)
                transform = ((FamilyInstance)element).GetTotalTransform();

            Options options = new Options();
            options.ComputeReferences = false;
            options.DetailLevel = ViewDetailLevel.Fine;

            return ToSAM<T>(element.get_Geometry(options), transform);
        }

        public static List<T> ToSAM<T>(this GeometryElement geometryElement, Transform transform = null) where T : ISAMGeometry
        {
            if (geometryElement == null)
                return null;

            List<T> result = new List<T>();
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

                    List<T> sAMGeometries = ToSAM<T>(geometryElement_Temp);
                    if (sAMGeometries != null && sAMGeometries.Count > 0)
                        result.AddRange(sAMGeometries);
                }
                else if (geometryObject is Solid)
                {
                    if(typeof(T).IsAssignableFrom(typeof(Shell)))
                    {
                        Shell shell = ((Solid)geometryObject).ToSAM();
                        if (shell != null)
                        {
                            result.Add((T)(ISAMGeometry)shell);
                        }
                    }
                    else if(typeof(T).IsAssignableFrom(typeof(Face3D)))
                    {
                        List<Face3D> face3Ds = ((Solid)geometryObject).ToSAM_Face3Ds();
                        if (face3Ds != null)
                        {
                            foreach (Face3D face3D in face3Ds)
                            {
                                result.Add((T)(ISAMGeometry)face3D);
                            }
                        }
                    }
                }

            }
            return result;
        }
    }
}