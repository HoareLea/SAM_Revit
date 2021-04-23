using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static Vector3D Transform(this Transform transform, Vector3D vector3D, bool convertUnits = true)
        {
            if (transform == null || vector3D == null)
                return null;

            if (transform.IsIdentity)
                return new Vector3D(vector3D);

            XYZ xyz = vector3D.ToRevit(convertUnits);

            return transform.OfVector(xyz)?.ToSAM_Vector3D(convertUnits);
        }

        public static Point3D Transform(this Transform transform, Point3D point3D, bool convertUnits = true)
        {
            if (transform == null || point3D == null)
                return null;

            if (transform.IsIdentity)
                return new Point3D(point3D);

            XYZ xyz = point3D.ToRevit(convertUnits);

            return transform.OfPoint(xyz)?.ToSAM(convertUnits);
        }

        public static Spatial.Plane Transform(this Transform transform, Spatial.Plane plane, bool convertUnits = true)
        {
            if (transform == null || plane == null)
                return null;

            if (transform.IsIdentity)
                return new Spatial.Plane(plane);

            return new Spatial.Plane(Transform(transform, plane.Origin, convertUnits), Transform(transform, plane.Normal, convertUnits));
        }

        public static List<Point3D> Transform(this Transform transform, IEnumerable<Point3D> point3Ds, bool convertUnits = true)
        {
            if (transform == null || point3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>();
            foreach (Point3D point3D in point3Ds)
                result.Add(Transform(transform, point3D, convertUnits));

            return result;
        }

        public static Segment3D Transform(this Transform transform, Segment3D segment3D, bool convertUnits = true)
        {
            if (transform == null || segment3D == null)
                return null;

            if (transform.IsIdentity)
                return new Segment3D(segment3D);

            return new Segment3D(Transform(transform, segment3D[0], convertUnits), Transform(transform, segment3D[1], convertUnits));
        }

        public static Triangle3D Transform(this Transform transform, Triangle3D triangle3D, bool convertUnits = true)
        {
            if (transform == null || triangle3D == null)
                return null;

            if (transform.IsIdentity)
                return new Triangle3D(triangle3D);

            List<Point3D> point3Ds = triangle3D.GetPoints();
            return new Triangle3D(Transform(transform, point3Ds[0], convertUnits), Transform(transform, point3Ds[1], convertUnits), Transform(transform, point3Ds[2], convertUnits));
        }

        public static Polyline3D Transform(this Transform transform, Polyline3D polyline3D, bool convertUnits = true)
        {
            if (transform == null || polyline3D == null)
                return null;

            if (transform.IsIdentity)
                return new Polyline3D(polyline3D);

            List<Point3D> point3Ds = polyline3D.GetPoints();
            if (point3Ds == null)
                return null;

            return new Polyline3D(Transform(transform, point3Ds, convertUnits));
        }

        public static Polygon3D Transform(this Transform transform, Polygon3D polygon3D, bool convertUnits = true)
        {
            if (transform == null || polygon3D == null)
                return null;

            if (transform.IsIdentity)
                return new Polygon3D(polygon3D);

            Vector3D normal = polygon3D.GetPlane()?.Normal;
            if (normal == null)
                return null;

            List<Point3D> point3Ds = polygon3D.GetPoints();
            if (point3Ds == null)
                return null;

            return Spatial.Create.Polygon3D(Transform(transform, normal, convertUnits), Transform(transform, point3Ds, convertUnits));
        }

        public static ICurve3D Transform(this Transform transform, ICurve3D curve3D)
        {
            if (transform == null || curve3D == null)
                return null;

            return Transform(transform, curve3D as dynamic);
        }

        public static Face3D Transform(this Transform transform, Face3D face3D, bool convertUnits = true)
        {
            if (transform == null || face3D == null)
                return null;

            if (transform.IsIdentity)
                return new Face3D(face3D);

            Spatial.Plane plane = Transform(transform, face3D.GetPlane());
            if (plane == null)
                return null;

            Planar.IClosed2D externalEdge2D = plane.Convert(Transform(transform, face3D.GetExternalEdge3D(), convertUnits));
            if (externalEdge2D == null)
                return null;

            List<Planar.IClosed2D> internalEdges2D = null;

            List<IClosedPlanar3D> internalEdges3D = face3D.GetInternalEdge3Ds();
            if(internalEdges3D != null)
            {
                internalEdges2D = new List<Planar.IClosed2D>();
                foreach (IClosedPlanar3D internalEdge3D in internalEdges3D)
                {
                    Planar.IClosed2D internalEdge2D = plane.Convert(Transform(transform, internalEdge3D, convertUnits));
                    if (internalEdge2D == null)
                        continue;

                    internalEdges2D.Add(internalEdge2D);
                }
            }

            return Face3D.Create(plane, externalEdge2D, internalEdges2D, false);
        }

        public static IClosedPlanar3D Transform(this Transform transform, IClosedPlanar3D closedPlanar3D, bool convertUnits = true)
        {
            if (transform == null || closedPlanar3D == null)
                return null;

            return Transform(transform, closedPlanar3D as dynamic, convertUnits);
        }
    }
}