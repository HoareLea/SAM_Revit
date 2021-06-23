using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<Shell> ToSAM_Shells(this Element element)
        {
            Transform transform = null;
            if (element is FamilyInstance)
                transform = ((FamilyInstance)element).GetTotalTransform();

            Options options = new Options();
            options.ComputeReferences = false;
            options.DetailLevel = ViewDetailLevel.Fine;

            return ToSAM_Shells(element.get_Geometry(options), transform);
        }

        public static List<Shell> ToSAM_Shells(this GeometryElement geometryElement, Transform transform = null)
        {
            if (geometryElement == null)
                return null;

            List<Shell> result = new List<Shell>();
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

                    List<Shell> shells = ToSAM_Shells(geometryElement_Temp);
                    if (shells != null && shells.Count > 0)
                        result.AddRange(shells);
                }
                else if (geometryObject is Solid)
                {
                    Shell shell = ((Solid)geometryObject).ToSAM();
                    if(shell != null)
                    {
                        result.Add(shell);
                    }
                }
            }
            return result;
        }
    }
}