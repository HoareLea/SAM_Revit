using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

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

            if(hostObject is CurtainSystem)
                return Profiles_CurtainSystem((CurtainSystem)hostObject);

            return null;
        }

        private static List<Face3D> Profiles_FaceWall(this FaceWall faceWall)
        {
            GeometryElement geometryElement = faceWall.get_Geometry(new Options());
            if (geometryElement == null)
                return null;

            List<Spatial.Face3D> result = new List<Spatial.Face3D>();
            foreach (GeometryObject geometryObject in geometryElement)
            {
                Type aType = geometryObject.GetType();

                if (geometryObject is Autodesk.Revit.DB.Face)
                {
                }
                else if (geometryObject is Solid)
                {
                }
                else if (geometryObject is Curve)
                {
                }
            }

            return null;
        }

        private static List<Face3D> Profiles_Wall(this Wall wall)
        {
            if (wall == null)
                return null;

            List<Face3D> result = Profiles_FromSketch(wall, !wall.Flipped);
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

                    result = new List<Face3D>();
                    foreach (ICurve3D curve3D in curves)
                    {
                        if (curve3D == null)
                            continue;

                        ICurve3D maxCurve = Spatial.Query.Project(plane_max, curve3D);
                        ICurve3D minCurve = Spatial.Query.Project(plane_min, curve3D);

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
                    {
                        //TODO: Implement Cutting by GeneratingElements
                        return result;
                    }
                        
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
#if Revit2017
            return null;
#else
            List<Face3D> face3Ds = TopProfiles(roofBase);

            IEnumerable<ElementId> elementIds = roofBase.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_Windows));
            if (elementIds == null || elementIds.Count() == 0)
                return face3Ds;

            foreach (ElementId elementId in elementIds)
            {
                Element element = roofBase.Document.GetElement(elementId);
                if (element == null)
                    continue;

                BoundingBoxXYZ boundingBoxXYZ = element.get_BoundingBox(null);
                Point3D point3D = ((boundingBoxXYZ.Max + boundingBoxXYZ.Min) / 2).ToSAM();
                foreach (Face3D face3D in face3Ds)
                {
                    List<Planar.IClosed2D> internalEdges = face3D.InternalEdge2Ds;
                    if (internalEdges == null || internalEdges.Count == 0)
                        continue;

                    Spatial.Plane plane = face3D.GetPlane();

                    Point3D point3D_Projected = plane.Project(point3D);
                    Planar.Point2D point2D = plane.Convert(point3D_Projected);

                    for (int i = 0; i < internalEdges.Count; i++)
                    {
                        Planar.IClosed2D internalEdge = internalEdges[i];
                        if (internalEdge.Inside(point2D))
                        {
                            face3D.RemoveInternalEdge(i);
                            break;
                        }
                    }
                }
            }

            return face3Ds;
#endif
        }

        private static List<Face3D> Profiles_Ceiling(this Ceiling ceiling)
        {
            return BottomProfiles(ceiling);
        }

        private static List<Face3D> Profiles_CurtainSystem(this CurtainSystem curtainSystem)
        {
            CurtainGridSet curtainGridSet = curtainSystem?.CurtainGrids;
            if(curtainGridSet == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();
            foreach (CurtainGrid curtainGrid in curtainGridSet)
            {
                IEnumerable<CurtainCell> curtainCells = curtainGrid.GetCurtainCells();
                if(curtainCells == null || curtainCells.Count() == 0)
                {
                    continue;
                }

                List<Polygon3D> polygon3Ds = new List<Polygon3D>(); 
                foreach(CurtainCell curtainCell in curtainCells)
                {
                    foreach (CurveArray curveArray in curtainCell.PlanarizedCurveLoops)
                    {
                        Polygon3D polygon3D = curveArray?.ToSAM_Polygon3D();
                        if (polygon3D == null && !polygon3D.IsValid())
                            continue;

                        polygon3Ds.Add(polygon3D);
                        result.Add(new Face3D(polygon3D));
                    }
                }
            }

            return result;
        }

        private static List<Face3D> Profiles_FromSketch(this HostObject hostObject, bool flip = false)
        {
#if Revit2017
            return null;
#else
            IEnumerable<ElementId> elementIds = hostObject.GetDependentElements(new ElementClassFilter(typeof(Sketch)));
            if (elementIds == null || elementIds.Count() == 0)
                return null;

            Document document = hostObject.Document;

            //Remove FaceSplitter Sketches
            IEnumerable<ElementId> elementIds_FaceSplitter = hostObject.GetDependentElements(new ElementClassFilter(typeof(FaceSplitter)));
            if (elementIds_FaceSplitter != null && elementIds_FaceSplitter.Count() != 0)
            {
                List<ElementId> elementIds_Temp = new List<ElementId>(elementIds); 
                foreach (ElementId elementId in elementIds_FaceSplitter)
                {
                    FaceSplitter faceSplitter = document.GetElement(elementId) as FaceSplitter;
                    if (faceSplitter == null)
                        continue;

                    IEnumerable<ElementId> elementIds_Sketch = faceSplitter.GetDependentElements(new ElementClassFilter(typeof(Sketch)));
                    if (elementIds_Sketch != null && elementIds_Sketch.Count() != 0)
                        elementIds_Temp.RemoveAll(x => elementIds_Sketch.Contains(x));
                }
                elementIds = elementIds_Temp;
            }

            List<Face3D> result = new List<Face3D>();
            foreach (ElementId elementId in elementIds)
            {
                Sketch sketch = document.GetElement(elementId) as Sketch;
                if (sketch == null)
                    continue;

                List<Face3D> face3Ds = Convert.ToSAM_Face3Ds(sketch, flip);
                if (face3Ds == null)
                    continue;

                foreach (Face3D face in face3Ds)
                    if (face != null)
                        result.Add(face);
            }

            //List<Shell> shells = hostObject.ToSAM_Shells();
            //if(shells != null && result != null && result.Count != 0)
            //{
            //    List<Face3D> face3Ds_Internal = new List<Face3D>();

            //    foreach (Face3D face3D in result)
            //    {
            //        List<Face3D> face3Ds_Temp = face3D.InternalFace3Ds(shells);
            //        if(face3Ds_Temp != null && face3Ds_Temp.Count != 0)
            //        {
            //            face3Ds_Internal.AddRange(face3Ds_Temp);
            //        }
            //    }

            //    result.AddRange(face3Ds_Internal);
            //}

            return result;
#endif
        }
    }
}