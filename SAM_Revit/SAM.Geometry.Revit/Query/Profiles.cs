using System;
using System.Collections.Generic;
using System.Linq;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<Face3D> Profiles(this HostObject hostObject)
        {
            if (hostObject == null || hostObject.Document == null)
                return null;

            if (hostObject is Wall)
                return Profiles_Wall((Wall)hostObject);

            if (hostObject is Floor)
                return Profiles_Floor((Floor)hostObject);

            if (hostObject is RoofBase)
                return Profiles_RoofBase((RoofBase)hostObject);

            if (hostObject is Ceiling)
                return Profiles_Ceiling((Ceiling)hostObject);

            if (hostObject is FaceWall)
                return Profiles_FaceWall((FaceWall)hostObject);

            return null;
        }

        public static List<Face3D> Profiles_FaceWall(this FaceWall faceWall)
        {
            GeometryElement geometryElement = faceWall.get_Geometry(new Options());
            if (geometryElement == null)
                return null;

            List<Spatial.Face3D> result = new List<Spatial.Face3D>();
            foreach (GeometryObject geometryObject in geometryElement)
            {
                Type aType = geometryObject.GetType();

                if(geometryObject is Autodesk.Revit.DB.Face)
                {

                }
                else if(geometryObject is Solid)
                {

                }
                else if(geometryObject is Curve)
                {

                }
            }

            return null;
        }

        public static List<Face3D> Profiles_Wall(this Wall wall)
        {
            List<Face3D> result = Profiles_FromSketch(wall);
            if (result != null && result.Count > 0)
                return result;

            BoundingBoxXYZ boundingBoxXYZ = wall.get_BoundingBox(null);
            if (boundingBoxXYZ != null)
            {
                LocationCurve locationCurve = wall.Location as LocationCurve;
                if (locationCurve != null)
                {
                    ICurve3D curve3D_Location = Convert.ToSAM(locationCurve);

                    IEnumerable<ICurve3D> curves = null;
                    if (curve3D_Location is ISegmentable3D)
                        curves = ((ISegmentable3D)curve3D_Location).GetSegments().Cast<ICurve3D>();
                    else
                        curves = new List<ICurve3D>() { curve3D_Location };

                    double max = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Max.Z, DisplayUnitType.DUT_METERS);
                    Spatial.Plane plane_max = new Spatial.Plane(new Point3D(0, 0, max), new Vector3D(0, 0, 1));

                    double min = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Min.Z, DisplayUnitType.DUT_METERS);
                    Spatial.Plane plane_min = new Spatial.Plane(new Point3D(0, 0, min), new Vector3D(0, 0, 1));

                    result = new List<Spatial.Face3D>();
                    foreach (ICurve3D curve3D in curves)
                    {
                        if (curve3D == null)
                            continue;

                        ICurve3D maxCurve = plane_max.Project(curve3D);
                        ICurve3D minCurve = plane_min.Project(curve3D);

                        Point3D point3D_1 = minCurve.GetEnd();
                        Point3D point3D_2 = maxCurve.GetStart();
                        Point3D point3D_3 = maxCurve.GetEnd();
                        if (point3D_1.Distance(point3D_3) < point3D_1.Distance(point3D_2))
                        {
                            Point3D point_Temp = point3D_2;

                            maxCurve.Reverse();
                            point3D_2 = point3D_3;
                            point3D_3 = point_Temp;
                        }

                        List<Point3D> point3Ds = new List<Point3D>() { minCurve.GetStart(), point3D_3, point3D_2, point3D_1 };
                        if (wall.Flipped)
                            point3Ds.Reverse();

                        result.Add(new Face3D(new Polygon3D(point3Ds)));
                    }

                    if (result != null && result.Count > 0)
                        return result;
                }
            }

            if (!ExporterIFCUtils.HasElevationProfile(wall))
                return null;

            IList<CurveLoop> curveLoops = ExporterIFCUtils.GetElevationProfile(wall);
            if (curveLoops == null)
                return null;

            result = new List<Face3D>();
            foreach (CurveLoop curveLoop in curveLoops)
            {
                Polygon3D polygon3D = curveLoop.ToSAM_Polygon3D();
                if (polygon3D != null)
                    result.Add(new Face3D(polygon3D));
            }

            return result;
        }

        private static List<Face3D> Profiles_Floor(this Floor floor)
        {
            List<Face3D> face3Ds = TopProfiles(floor);      
            return face3Ds;
        }

        private static List<Face3D> Profiles_RoofBase(this RoofBase roofBase)
        {
            List<Face3D> face3Ds = TopProfiles(roofBase);

            IEnumerable<ElementId> elementIds = roofBase.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_Windows));
            if (elementIds == null || elementIds.Count() == 0)
                return face3Ds;

            foreach(ElementId elementId in elementIds)
            {
                Element element = roofBase.Document.GetElement(elementId);
                if (element == null)
                    continue;

                BoundingBoxXYZ boundingBoxXYZ = element.get_BoundingBox(null);
                Point3D point3D = ((boundingBoxXYZ.Max + boundingBoxXYZ.Min) / 2).ToSAM();
                foreach(Face3D face3D in face3Ds)
                {
                    List<Planar.IClosed2D> internalEdges = face3D.InternalEdges;
                    if (internalEdges == null || internalEdges.Count == 0)
                        continue;

                    Spatial.Plane plane = face3D.GetPlane();

                    Point3D point3D_Projected = plane.Project(point3D);
                    Planar.Point2D point2D = plane.Convert(point3D_Projected);

                    for(int i=0; i < internalEdges.Count; i++)
                    {
                        Planar.IClosed2D internalEdge = internalEdges[i];
                        if(internalEdge.Inside(point2D))
                        {
                            face3D.RemoveInternalEdge(i);
                            break;
                        }
                    }
                }
            }

            return face3Ds;
        }

        private static List<Face3D> Profiles_Ceiling(this Ceiling ceiling)
        {
            return BottomProfiles(ceiling);
        }

        private static List<Face3D> Profiles_FromSketch(this HostObject hostObject)
        {
            IEnumerable<ElementId> elementIds = hostObject.GetDependentElements(new ElementClassFilter(typeof(Sketch)));
            if (elementIds == null || elementIds.Count() == 0)
                return null;

            Document document = hostObject.Document;

            List<Face3D> result = new List<Face3D>();
            foreach (ElementId elementId in elementIds)
            {
                Sketch sketch = document.GetElement(elementId) as Sketch;
                if (sketch == null)
                    continue;

                List<Face3D> face3Ds = Convert.ToSAM_Face3Ds(sketch);
                if (face3Ds == null)
                    continue;

                foreach (Face3D face in face3Ds)
                    if (face != null)
                        result.Add(face);
            }

            return result;
        }
    }
}
