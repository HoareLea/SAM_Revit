using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static Vector3D Transform(this Transform transform, Vector3D vector3D)
        {
            if (transform == null || vector3D == null)
                return null;

            if (transform.IsIdentity)
                return new Vector3D(vector3D);

            XYZ xyz = vector3D.ToRevit(true);

            return transform.OfVector(xyz)?.ToSAM_Vector3D(true);
        }

        public static Point3D Transform(this Transform transform, Point3D point3D)
        {
            if (transform == null || point3D == null)
                return null;

            if (transform.IsIdentity)
                return new Point3D(point3D);

            XYZ xyz = point3D.ToRevit(true);

            return transform.OfPoint(xyz)?.ToSAM(true);
        }

        public static Spatial.Plane Transform(this Transform transform, Spatial.Plane plane)
        {
            if (transform == null || plane == null)
                return null;

            if (transform.IsIdentity)
                return new Spatial.Plane(plane);

            return new Spatial.Plane(Transform(transform, plane.Origin), Transform(transform, plane.Normal));
        }

        public static List<Point3D> Transform(this Transform transform, IEnumerable<Point3D> point3Ds)
        {
            if (transform == null || point3Ds == null)
                return null;

            List<Point3D> result = new List<Point3D>();
            foreach (Point3D point3D in point3Ds)
                result.Add(Transform(transform, point3D));

            return result;
        }

        public static Segment3D Transform(this Transform transform, Segment3D segment3D)
        {
            if (transform == null || segment3D == null)
                return null;

            if (transform.IsIdentity)
                return new Segment3D(segment3D);

            return new Segment3D(Transform(transform, segment3D[0]), Transform(transform, segment3D[1]));
        }

        public static Triangle3D Transform(this Transform transform, Triangle3D triangle3D)
        {
            if (transform == null || triangle3D == null)
                return null;

            if (transform.IsIdentity)
                return new Triangle3D(triangle3D);

            List<Point3D> point3Ds = triangle3D.GetPoints();
            return new Triangle3D(Transform(transform, point3Ds[0]), Transform(transform, point3Ds[1]), Transform(transform, point3Ds[2]));
        }

        public static Polyline3D Transform(this Transform transform, Polyline3D polyline3D)
        {
            if (transform == null || polyline3D == null)
                return null;

            if (transform.IsIdentity)
                return new Polyline3D(polyline3D);

            List<Point3D> point3Ds = polyline3D.GetPoints();
            if (point3Ds == null)
                return null;

            return new Polyline3D(Transform(transform, point3Ds));
        }

        public static Polygon3D Transform(this Transform transform, Polygon3D polygon3D)
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

            return Spatial.Create.Polygon3D(Transform(transform, normal), Transform(transform, point3Ds));
        }

        public static ICurve3D Transform(this Transform transform, ICurve3D curve3D)
        {
            if (transform == null || curve3D == null)
                return null;

            return Transform(transform, curve3D as dynamic);
        }

        public static Face3D Transform(this Transform transform, Face3D face3D)
        {
            if (transform == null || face3D == null)
                return null;

            if (transform.IsIdentity)
                return new Face3D(face3D);

            Spatial.Plane plane = Transform(transform, face3D.GetPlane());
            if (plane == null)
                return null;

            Planar.IClosed2D externalEdge2D = plane.Convert(Transform(transform, face3D.GetExternalEdge3D()));
            if (externalEdge2D == null)
                return null;

            List<Planar.IClosed2D> internalEdges2D = null;

            List<IClosedPlanar3D> internalEdges3D = face3D.GetInternalEdge3Ds();
            if(internalEdges3D != null)
            {
                internalEdges2D = new List<Planar.IClosed2D>();
                foreach (IClosedPlanar3D internalEdge3D in internalEdges3D)
                {
                    Planar.IClosed2D internalEdge2D = plane.Convert(Transform(transform, internalEdge3D));
                    if (internalEdge2D == null)
                        continue;

                    internalEdges2D.Add(internalEdge2D);
                }
            }

            return Face3D.Create(plane, externalEdge2D, internalEdges2D, false);
        }

        public static IClosedPlanar3D Transform(this Transform transform, IClosedPlanar3D closedPlanar3D)
        {
            if (transform == null || closedPlanar3D == null)
                return null;

            return Transform(transform, closedPlanar3D as dynamic);
        }
    }
}