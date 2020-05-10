using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static List<Solid> Solids(this GeometryElement geometryElement, Transform transform = null)
        {
            if (geometryElement == null)
                return null;

            List<Solid> result = new List<Solid>();
            foreach (GeometryObject geomObject in geometryElement)
            {
                if (geomObject is GeometryInstance)
                {
                    GeometryInstance geomInstance = (GeometryInstance)geomObject;

                    Transform transformation = geomInstance.Transform;
                    if (transform != null)
                        transformation = transformation.Multiply(transform.Inverse);

                    GeometryElement geomElement = geomInstance.GetInstanceGeometry(transformation);
                    if (geomElement == null)
                        continue;

                    List<Solid> solids = Solids(geomElement);
                    if (solids != null && solids.Count != 0)
                        result.AddRange(solids);
                }
                else if (geomObject is Solid)
                {
                    result.Add((Solid)geomObject);
                }
            }

            return result;
        }

        public static List<Solid> Solids(this Element element, Options options)
        {
            GeometryElement geomElement = element.get_Geometry(options);

            Transform transform = null;
            if (element is FamilyInstance)
                transform = ((FamilyInstance)element).GetTotalTransform();

            return Solids(geomElement, transform);
        }
    }
}