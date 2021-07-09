using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            List<Face3D> result;

            result = Profiles_FromSketch(wall, !wall.Flipped);
            if (result != null && result.Count > 0)
                return result;

            result = Profiles_FromLocation(wall);
            if (result != null && result.Count > 0)
                return result;

            result = Profiles_FromElevationProfile(wall);
            if (result != null && result.Count > 0)
                return result;

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
            Document document = curtainSystem?.Document;
            if(document == null)
            {
                return null;
            }

            CurtainGridSet curtainGridSet = curtainSystem?.CurtainGrids;
            if(curtainGridSet == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();
            foreach (CurtainGrid curtainGrid in curtainGridSet)
            {
                IEnumerable<CurtainCell> curtainCells = curtainGrid.GetCurtainCells();
                if (curtainCells == null || curtainCells.Count() == 0)
                {
                    continue;
                }

                List<CurveArrArray> curveArrArrays = new List<CurveArrArray>();
                foreach (CurtainCell curtainCell in curtainCells)
                {
                    CurveArrArray curveArrArray = curtainCell?.PlanarizedCurveLoops;
                    if (curveArrArray == null || curveArrArray.Size == 0)
                    {
                        continue;
                    }

                    curveArrArrays.Add(curveArrArray);
                }

                List<Segment3D> segment3Ds = new List<Segment3D>();
                List<ISegmentable3D> segmentable3Ds_U = new List<ISegmentable3D>();
                List<ISegmentable3D> segmentable3Ds_V = new List<ISegmentable3D>();

                ICollection<ElementId> elementIds = null;

                elementIds = curtainGrid.GetUGridLineIds();
                if (elementIds != null)
                {
                    foreach (ElementId elementId in elementIds)
                    {
                        CurtainGridLine curtainGridLine = document.GetElement(elementId) as CurtainGridLine;
                        if (curtainGridLine == null)
                        {
                            continue;
                        }

                        ISegmentable3D segmentable3D = curtainGridLine.FullCurve.ToSAM() as ISegmentable3D;
                        if (segmentable3D == null)
                        {
                            continue;
                        }

                        segmentable3Ds_U.Add(segmentable3D);
                        segment3Ds.AddRange(segmentable3D.GetSegments());
                    }
                }

                elementIds = curtainGrid.GetVGridLineIds();
                if (elementIds != null)
                {
                    foreach (ElementId elementId in elementIds)
                    {
                        CurtainGridLine curtainGridLine = document.GetElement(elementId) as CurtainGridLine;
                        if (curtainGridLine == null)
                        {
                            continue;
                        }

                        ISegmentable3D segmentable3D = curtainGridLine.FullCurve.ToSAM() as ISegmentable3D;
                        if (segmentable3D == null)
                        {
                            continue;
                        }

                        segmentable3Ds_V.Add(segmentable3D);
                        segment3Ds.AddRange(segmentable3D.GetSegments());
                    }
                }

                List<ISegmentable3D> segmentable3Ds = null;

                segmentable3Ds = ExtremeSegmentable3Ds(segmentable3Ds_U, segmentable3Ds_V);
                if(segmentable3Ds != null && segmentable3Ds.Count > 0)
                {
                    foreach(ISegmentable3D segmentable3D in segmentable3Ds)
                    {
                        List<Segment3D> segment3Ds_Temp = segmentable3D?.GetSegments();
                        if(segment3Ds_Temp != null && segment3Ds_Temp.Count != 0)
                        {
                            segment3Ds.AddRange(segment3Ds_Temp);
                        }
                    }
                }

                segmentable3Ds = ExtremeSegmentable3Ds(segmentable3Ds_V, segmentable3Ds_U);
                if (segmentable3Ds != null && segmentable3Ds.Count > 0)
                {
                    foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                    {
                        List<Segment3D> segment3Ds_Temp = segmentable3D?.GetSegments();
                        if (segment3Ds_Temp != null && segment3Ds_Temp.Count != 0)
                        {
                            segment3Ds.AddRange(segment3Ds_Temp);
                        }
                    }
                }


                List<List<Face3D>> face3Ds = Enumerable.Repeat<List<Face3D>>(null, curveArrArrays.Count).ToList();
                Parallel.For(0, curveArrArrays.Count, (int i) =>
                {
                    CurveArrArray curveArrArray = curveArrArrays[i];
                    if(curveArrArray == null || curveArrArray.Size == 0)
                    {
                        return;
                    }

                    face3Ds[i] = new List<Face3D>();
                    foreach (CurveArray curveArray in curveArrArray)
                    {
                        Polygon3D polygon3D = curveArray?.ToSAM_Polygon3D();
                        if (polygon3D == null && !polygon3D.IsValid())
                        {
                            continue;
                        }

                        Spatial.Plane plane = polygon3D.GetPlane();
                        if (plane == null)
                        {
                            continue;
                        }

                        Polygon2D polygon2D = plane.Convert(polygon3D);
                        if (polygon2D != null)
                        {
                            List<Segment2D> segment2Ds = segment3Ds.ConvertAll(x => plane.Convert(plane.Project(x)));
                            segment2Ds.RemoveAll(x => x == null || x.GetLength() < Core.Tolerance.MacroDistance);
                            segment2Ds = segment2Ds.Split();

                            List<Polygon2D> polygon2Ds = Planar.Create.Polygon2Ds(segment2Ds);
                            if (polygon2Ds != null)
                            {
                                polygon2Ds = polygon2Ds.FindAll(x => x.Inside(polygon2D));
                                if (polygon2Ds != null && polygon2Ds.Count > 0)
                                {
                                    polygon2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
                                    polygon3D = plane.Convert(polygon2Ds[0]);
                                }
                            }
                        }

                        face3Ds[i].Add(new Face3D(polygon3D));
                    }
                });

                foreach(List<Face3D> face3Ds_Temp in face3Ds)
                {
                    if(face3Ds_Temp != null && face3Ds_Temp.Count > 0)
                    {
                        result.AddRange(face3Ds_Temp);
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

            return result;
#endif
        }

        private static List<Face3D> Profiles_FromLocation(this Wall wall, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            BoundingBoxXYZ boundingBoxXYZ = wall?.get_BoundingBox(null);
            if (boundingBoxXYZ == null)
            {
                return null;
            }

            LocationCurve locationCurve = wall.Location as LocationCurve;
            if (locationCurve == null)
            {
                return null;
            }

            ICurve3D curve3D_Location = Convert.ToSAM(locationCurve);
            IEnumerable<ICurve3D> curves = null;
            if (curve3D_Location is ISegmentable3D)
                curves = ((ISegmentable3D)curve3D_Location).GetSegments().Cast<ICurve3D>();
            else
                curves = new List<ICurve3D>() { curve3D_Location };

            List<Shell> shells = wall.ToSAM_Geometries<Shell>();
            if(shells == null || shells.Count == 0)
            {
                return null;
            }

            Document document = wall.Document;

            Dictionary<ElementId, List<Face3D>> dictionary = wall.GeneratingElementIdDictionary();
            if(dictionary != null && dictionary.Count != 0)
            {
                foreach (ElementId elementId in dictionary.Keys)
                {
                    Wall wall_Temp = document.GetElement(elementId) as Wall;
                    if(wall_Temp == null)
                    {
                        continue;
                    }

                    List<Shell> shells_Temp = wall_Temp.ToSAM_Geometries<Shell>();
                    if(shells_Temp != null && shells_Temp.Count != 0)
                    {
                        shells.AddRange(shells_Temp);
                    }
                }
            }

            List<Face3D> result = new List<Face3D>();
            foreach (ICurve3D curve3D in curves)
            {
                Segment3D segment3D = new Segment3D(curve3D.GetStart(), curve3D.GetEnd());
                if(segment3D == null  || !segment3D.IsValid() || segment3D.GetLength() < tolerance_Distance)
                {
                    continue;
                }

                Spatial.Plane plane = Spatial.Create.Plane(segment3D[0], segment3D[0].GetMoved(Vector3D.WorldZ) as Point3D, segment3D[1]);

                List<Face3D> face3Ds = new List<Face3D>();
                foreach(Shell shell in shells)
                {
                    List<Face3D> face3Ds_Section =  shell.Section(plane, false, tolerance_Angle, tolerance_Distance, tolerance_Snap);
                    if(face3Ds_Section == null || face3Ds_Section.Count == 0)
                    {
                        continue;
                    }

                    face3Ds.AddRange(face3Ds_Section);
                }

                face3Ds = face3Ds.Union(tolerance_Distance);

                for (int i = 0; i < face3Ds.Count; i++)
                {
                    Face3D face3D = face3Ds[i];

                    BoundingBox3D boundingBox3D = face3D?.GetBoundingBox();

                    if (boundingBox3D != null)
                    {
                        continue;
                    }

                    double height = boundingBox3D.Max.Z - boundingBox3D.Min.Z;
                    List<Segment3D> segment3Ds = new List<Segment3D>();
                    Segment3D segment3D_Z = null;

                    segment3D_Z = new Segment3D(segment3D[0], segment3D[0].GetMoved(Vector3D.WorldZ * height) as Point3D);
                    if (boundingBox3D.InRange(segment3D_Z, tolerance_Distance))
                        segment3Ds.Add(segment3D_Z);

                    segment3D_Z = new Segment3D(segment3D[1], segment3D[1].GetMoved(Vector3D.WorldZ * height) as Point3D);
                    if (boundingBox3D.InRange(segment3D_Z, tolerance_Distance))
                        segment3Ds.Add(segment3D_Z);

                    if (segment3Ds.Count > 0)
                    {
                        Spatial.Plane plane_Temp = face3D.GetPlane();

                        ISegmentable2D segmentable2D = plane_Temp.Convert(face3D.GetExternalEdge3D()) as ISegmentable2D;
                        if (segmentable2D != null)
                        {
                            List<Segment2D> segment2Ds = segment3Ds.ConvertAll(x => plane_Temp.Convert(x));

                            segment2Ds.AddRange(segmentable2D.GetSegments());

                            List<Face2D> face2Ds = Planar.Create.Face2Ds(segment2Ds, tolerance_Distance);
                            if (face2Ds != null)
                            {
                                Point2D point2D = plane_Temp.Convert(segment3D.Mid());

                                Face2D face2D = face2Ds.Find(x => x.On(point2D, tolerance_Distance));
                                if (face2D != null)
                                {
                                    face3D = plane_Temp.Convert(face2D);
                                }
                            }

                        }
                    }

                    result.Add(face3D);
                }
            }

            return result;
        }

        //private static List<Face3D> Profiles_FromLocation(this Wall wall, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        //{
        //    BoundingBoxXYZ boundingBoxXYZ = wall.get_BoundingBox(null);
        //    if (boundingBoxXYZ == null)
        //    {
        //        return null;
        //    }

        //    LocationCurve locationCurve = wall.Location as LocationCurve;
        //    if(locationCurve == null)
        //    {
        //        return null;
        //    }

        //    ICurve3D curve3D_Location = Convert.ToSAM(locationCurve);

        //    IEnumerable<ICurve3D> curves = null;
        //    if (curve3D_Location is ISegmentable3D)
        //        curves = ((ISegmentable3D)curve3D_Location).GetSegments().Cast<ICurve3D>();
        //    else
        //        curves = new List<ICurve3D>() { curve3D_Location };

        //    Vector3D direction = Vector3D.WorldZ;

        //    double max = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Max.Z, DisplayUnitType.DUT_METERS);
        //    Spatial.Plane plane_max = new Spatial.Plane(new Point3D(0, 0, max), direction);

        //    double min = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Min.Z, DisplayUnitType.DUT_METERS);
        //    Spatial.Plane plane_min = new Spatial.Plane(new Point3D(0, 0, min), direction);

        //    double height = max - min;

        //    Document document = wall.Document;

        //    List<Face3D> face3Ds_Cutting = new List<Face3D>();

        //    Dictionary<ElementId, List<Face3D>> dictionary = GeneratingElementIdDictionary(wall);
        //    foreach (KeyValuePair<ElementId, List<Face3D>> keyValuePair in dictionary)
        //    {
        //        if (keyValuePair.Value == null || keyValuePair.Value.Count == 0)
        //        {
        //            continue;
        //        }

        //        HostObject hostObject_Cutting = document.GetElement(keyValuePair.Key) as HostObject;
        //        if (hostObject_Cutting == null)
        //        {
        //            continue;
        //        }

        //        if(hostObject_Cutting is Floor || hostObject_Cutting is RoofBase)
        //        {
        //            List<Face3D> face3Ds_Temp = hostObject_Cutting.Profiles();
        //            if(face3Ds_Temp != null && face3Ds_Temp.Count != 0)
        //            {
        //                face3Ds_Cutting.AddRange(face3Ds_Temp);
        //            }
        //        }
        //    }

        //    List<Face3D> result = new List<Face3D>();
        //    foreach (ICurve3D curve3D in curves)
        //    {
        //        if (curve3D == null)
        //        {
        //            continue;
        //        }

        //        ICurve3D maxCurve = Spatial.Query.Project(plane_max, curve3D);
        //        ICurve3D minCurve = Spatial.Query.Project(plane_min, curve3D);

        //        Point3D point3D_1 = minCurve.GetEnd();
        //        Point3D point3D_2 = maxCurve.GetStart();
        //        Point3D point3D_3 = maxCurve.GetEnd();
        //        Point3D point3D_4 = minCurve.GetStart();
        //        if (point3D_1.Distance(point3D_3) < point3D_1.Distance(point3D_2))
        //        {
        //            Point3D point_Temp = point3D_2;

        //            maxCurve.Reverse();
        //            point3D_2 = point3D_3;
        //            point3D_3 = point_Temp;
        //        }

        //        List<Point3D> point3Ds = new List<Point3D>() { point3D_4, point3D_3, point3D_2, point3D_1 };
        //        if (wall.Flipped)
        //        {
        //            point3Ds.Reverse();
        //        }

        //        Spatial.Plane plane = Spatial.Create.Plane(point3Ds, tolerance_Distance);
        //        if (plane == null)
        //        {
        //            continue;
        //        }

        //        Vector3D vector3D = new Vector3D(point3D_4, point3D_3);

        //        Segment2D segment2D = plane.Convert(new Segment3D(point3D_4, point3D_1));

        //        List<Segment2D> segment2Ds_Intersection = new List<Segment2D>();
        //        foreach (Face3D face3D_Cutting in face3Ds_Cutting)
        //        {
        //            PlanarIntersectionResult planarIntersectionResult = Spatial.Create.PlanarIntersectionResult(plane, face3D_Cutting, tolerance_Angle, tolerance_Distance);
        //            if(planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
        //            {
        //                continue;
        //            }

        //            List<Segment2D> segment2Ds_Intersection_Temp = planarIntersectionResult.GetGeometry2Ds<Segment2D>();
        //            if(segment2Ds_Intersection_Temp != null && segment2Ds_Intersection_Temp.Count > 0)
        //            {
        //                segment2Ds_Intersection.AddRange(segment2Ds_Intersection_Temp);
        //            }
        //        }

        //        List<Face2D> face2Ds = Profiles_From2D(segment2D, plane.Convert(vector3D), segment2Ds_Intersection, tolerance_Distance);
        //        if(face2Ds != null && face2Ds.Count > 0)
        //        {
        //            result.AddRange(face2Ds.ConvertAll(x => plane.Convert(x)));
        //        }
        //    }

        //    if (result != null && result.Count > 0)
        //    {
        //        return result;
        //    }

        //    return result;
        //}

        private static List<Face3D> Profiles_FromElevationProfile(this Wall wall)
        {
            if(wall == null)
            {
                return null;
            }

            if (!ExporterIFCUtils.HasElevationProfile(wall))
            {
                return null;
            }

            IList<CurveLoop> curveLoops = ExporterIFCUtils.GetElevationProfile(wall);
            if (curveLoops == null)
            {
                return null;
            }

            List<Face3D> result = new List<Face3D>();
            foreach (CurveLoop curveLoop in curveLoops)
            {
                Polygon3D polygon3D = curveLoop.ToSAM_Polygon3D();
                if (polygon3D == null)
                {
                    continue;
                }

                result.Add(new Face3D(polygon3D));
            }

            return result;
        }

        //private static List<Face2D> Profiles_From2D(this Segment2D segment2D, Vector2D vector2D, IEnumerable<Segment2D> segment2Ds, double tolerance = Core.Tolerance.Distance)
        //{
        //    if (segment2D == null || vector2D == null|| vector2D.Length < tolerance)
        //    {
        //        return null;
        //    }

        //    Polygon2D polygon2D = new Polygon2D(new Point2D[] { segment2D[0], segment2D[0].GetMoved(vector2D), segment2D[1].GetMoved(vector2D), segment2D[1] });
            
            
        //    Face2D face2D = new Face2D(polygon2D);

        //    if (segment2Ds == null || segment2Ds.Count() == 0)
        //    {
        //        return new List<Face2D>() { face2D };
        //    }

        //    List<Segment2D> segment2Ds_Temp = new List<Segment2D>(segment2Ds);
        //    segment2Ds_Temp.Add(segment2D);
        //    segment2Ds_Temp.Add(segment2D.GetMoved(vector2D));

        //    BoundingBox2D boundingBox2D_Max = new BoundingBox2D(segment2Ds_Temp.ConvertAll(x => x.GetBoundingBox()));
        //    Vector2D vector2D_Max =  vector2D.Unit * boundingBox2D_Max.Perimeter();

        //    BoundingBox2D boundingBox2D = polygon2D.GetBoundingBox();

        //    List <Face2D> result = new List<Face2D>() { face2D };
        //    foreach (Segment2D segment2D_Temp in segment2Ds)
        //    {
        //        if(segment2D_Temp == null || !segment2D_Temp.IsValid() || segment2D_Temp.GetLength() < tolerance)
        //        {
        //            continue;
        //        }

        //        Point2D point2D_1 = segment2D_Temp[0];
        //        point2D_1 = Planar.Query.Snap(polygon2D, point2D_1, tolerance);

        //        Point2D point2D_2 = point2D_1.GetMoved(vector2D_Max);

        //        Point2D point2D_4 = segment2D_Temp[1];
        //        point2D_4 = Planar.Query.Snap(polygon2D, point2D_4, tolerance);

        //        Point2D point2D_3 = point2D_4.GetMoved(vector2D_Max);
                
        //        if (point2D_1.Distance(point2D_2) < tolerance && point2D_3.Distance(point2D_4) < tolerance)
        //        {
        //            continue;
        //        }

        //        Polygon2D polygon2D_Temp = new Polygon2D(new Point2D[] { point2D_1, point2D_2, point2D_3, point2D_4 });
        //        if (!polygon2D_Temp.IsValid() || polygon2D_Temp.GetArea() < tolerance)
        //        {
        //            continue;
        //        }

        //        BoundingBox2D boundingBox2D_Temp = polygon2D_Temp.GetBoundingBox();
        //        if(!boundingBox2D.InRange(boundingBox2D_Temp, tolerance))
        //        {
        //            continue;
        //        }

        //        Face2D face2D_Temp = new Face2D(polygon2D_Temp);

        //        List<Face2D> face2Ds_Difference = new List<Face2D>();
        //        foreach (Face2D face2D_Result in result)
        //        {
        //            List<Face2D> face2Ds_Difference_Temp = face2D_Result.Difference(face2D_Temp, tolerance);
        //            if (face2Ds_Difference_Temp == null || face2Ds_Difference_Temp.Count == 0)
        //            {
        //                continue;
        //            }
        //            face2Ds_Difference.AddRange(face2Ds_Difference_Temp);
        //        }

        //        result = face2Ds_Difference;
        //    }

        //    return result;
        //}
    }
}