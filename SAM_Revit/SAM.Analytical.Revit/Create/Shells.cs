using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Create
    {
        public static List<Shell> Shells(this Document document, IEnumerable<Autodesk.Revit.DB.Mechanical.Space> spaces = null, double offset = 0.1, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (document == null)
                return null;

            //Collecting Space list
            List<Autodesk.Revit.DB.Mechanical.Space> spaces_Temp = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();
            if(spaces != null)
            {
                List<Autodesk.Revit.DB.Mechanical.Space> spaces_New = new List<Autodesk.Revit.DB.Mechanical.Space>();
                foreach (Autodesk.Revit.DB.Mechanical.Space space in spaces)
                {
                    int index = spaces_Temp.FindIndex(x => x.Id.IntegerValue == space.Id.IntegerValue);
                    if (index != -1)
                        spaces_New.Add(spaces_Temp[index]);
                }
                spaces_Temp = spaces_New;
            }

            if (spaces_Temp == null || spaces_Temp.Count == 0)
                return null;

            //Dictionary of bottom elevations and tuples (top elevation, location 2D, Space) (Metric Units)
            Dictionary<double, List<Tuple<double, Geometry.Planar.Point2D, Autodesk.Revit.DB.Mechanical.Space>>> dictionary = new Dictionary<double, List<Tuple<double, Geometry.Planar.Point2D, Autodesk.Revit.DB.Mechanical.Space>>>();
            
            //Space cut elevations (Imperial Units)
            HashSet<double> cutElevations = new HashSet<double>();

            //Collecting Spaces data
            foreach (Autodesk.Revit.DB.Mechanical.Space space in spaces_Temp)
            {
                XYZ xyz = (space.Location as LocationPoint)?.Point;
                if (xyz == null)
                    continue;

                double elevation_Top = double.NaN;

                Parameter parameter = space.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL);
                if (parameter != null && parameter.HasValue)
                {
                    ElementId elementId = parameter.AsElementId();
                    if (elementId != null && elementId != ElementId.InvalidElementId)
                    {
                        Level level = document.GetElement(elementId) as Level;
                        if (level != null)
                        {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                            elevation_Top = UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);
#else
                            elevation_Top = UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters);
#endif
                        }

                    }
                }

                BoundingBoxXYZ boundingBoxXYZ = space.get_BoundingBox(null);

                if (double.IsNaN(elevation_Top) && boundingBoxXYZ != null)
                {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                    elevation_Top = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Max.Z, DisplayUnitType.DUT_METERS);
#else
                    elevation_Top = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Max.Z, UnitTypeId.Meters);
#endif
                }

                double elevation_Bottom = double.NaN;

                if (boundingBoxXYZ != null)
                {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                    elevation_Bottom = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Min.Z, DisplayUnitType.DUT_METERS);
#else
                    elevation_Bottom = UnitUtils.ConvertFromInternalUnits(boundingBoxXYZ.Min.Z, UnitTypeId.Meters);
#endif
                }


                if (double.IsNaN(elevation_Bottom))
                {
                    ElementId elementId = space.LevelId;
                    if (elementId != null && elementId != ElementId.InvalidElementId)
                    {
                        Level level = document.GetElement(elementId) as Level;
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                        elevation_Bottom = UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);
#else
                        elevation_Bottom = UnitUtils.ConvertFromInternalUnits(level.Elevation, UnitTypeId.Meters);
#endif
                    }
                }

                Point3D point3D = Geometry.Revit.Convert.ToSAM(xyz);
                if (point3D == null)
                    continue;

                Geometry.Planar.Point2D point2D = Geometry.Spatial.Plane.WorldXY.Convert(point3D);
                if (point2D == null)
                    continue;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                cutElevations.Add(UnitUtils.ConvertToInternalUnits(elevation_Bottom + offset, DisplayUnitType.DUT_METERS));
#else
                cutElevations.Add(UnitUtils.ConvertToInternalUnits(elevation_Bottom + offset, UnitTypeId.Meters));
#endif

                if (!dictionary.TryGetValue(elevation_Bottom, out List<Tuple<double, Geometry.Planar.Point2D, Autodesk.Revit.DB.Mechanical.Space>> tuples))
                {
                    tuples = new List<Tuple<double, Geometry.Planar.Point2D, Autodesk.Revit.DB.Mechanical.Space>>();
                    dictionary[elevation_Bottom] = tuples;
                }
                tuples.Add(new Tuple<double, Geometry.Planar.Point2D, Autodesk.Revit.DB.Mechanical.Space>(elevation_Top, point2D, space));
            }

            //Collecting Revit Walls
            List<Wall> walls = new FilteredElementCollector(document).OfClass(typeof(Wall)).Cast<Wall>().ToList();
            if (walls == null || walls.Count == 0)
                return null;

            //Converting Revit Walls to SAM Panels
            List<Panel> panels = new List<Panel>();
            foreach(Wall wall in walls)
            {
                BoundingBoxXYZ boundingBoxXYZ = wall?.get_BoundingBox(null);
                if (boundingBoxXYZ == null)
                {
                    continue;
                }

                bool valid = false;
                foreach(double cutElevation in cutElevations)
                {
                    if(boundingBoxXYZ.Max.Z >= cutElevation && boundingBoxXYZ.Min.Z <= cutElevation)
                    {
                        valid = true;
                        break;
                    }
                }

                if(!valid)
                {
                    continue;
                }
                
                List<Panel> panels_Wall = wall.ToSAM(new Core.Revit.ConvertSettings(true, false, false));
                if (panels_Wall == null || panels_Wall.Count == 0)
                    continue;

                panels.AddRange(panels_Wall);
            }

            if (panels == null || panels.Count == 0)
                return null;

            List<Shell> result = new List<Shell>();

            //Inerating through elevations and Spaces data
            foreach(KeyValuePair<double, List<Tuple<double, Geometry.Planar.Point2D, Autodesk.Revit.DB.Mechanical.Space>>> keyValuePair in dictionary)
            {
                double elevation_Bottom = keyValuePair.Key;
                double elevation_Cut = elevation_Bottom + offset;

                Geometry.Spatial.Plane plane_Cut = Geometry.Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Cut)) as Geometry.Spatial.Plane;

                List<Geometry.Planar.Segment2D> segment2Ds = new List<Geometry.Planar.Segment2D>();
                foreach (Panel panel in panels)
                {
                    IClosedPlanar3D closedPlanar3D = panel?.GetFace3D()?.GetExternalEdge3D();
                    if (closedPlanar3D == null)
                        continue;

                    PlanarIntersectionResult planarIntersectionResult = plane_Cut.PlanarIntersectionResult(closedPlanar3D);
                    if (planarIntersectionResult == null || !planarIntersectionResult.Intersecting)
                    {
                        continue;
                    }

                    List<Geometry.Planar.ISegmentable2D> segmentable2Ds_Temp = planarIntersectionResult.GetGeometry2Ds<Geometry.Planar.ISegmentable2D>();
                    if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    segmentable2Ds_Temp?.ForEach(x => segment2Ds.AddRange(x.GetSegments()));
                }

                if (panels == null || panels.Count == 0)
                    continue;

                segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance);

                segment2Ds = Geometry.Planar.Query.Snap(segment2Ds, true, snapTolerance);

                List<Tuple<Geometry.Planar.BoundingBox2D, Geometry.Planar.Polygon2D>> tuples_Polygon2D = Geometry.Planar.Create.Polygon2Ds(segment2Ds)?.ConvertAll(x => new Tuple<Geometry.Planar.BoundingBox2D, Geometry.Planar.Polygon2D>(x.GetBoundingBox(tolerance), x));
                if (tuples_Polygon2D == null || tuples_Polygon2D.Count == 0)
                    continue;

                Geometry.Spatial.Plane plane_Bottom = Geometry.Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation_Bottom)) as Geometry.Spatial.Plane;

                List<Tuple<double, Geometry.Planar.Point2D, Autodesk.Revit.DB.Mechanical.Space>> tuples_Space = keyValuePair.Value;
                while(tuples_Space.Count > 0)
                {
                    Tuple<double, Geometry.Planar.Point2D, Autodesk.Revit.DB.Mechanical.Space> tuple = tuples_Space[0];
                    tuples_Space.RemoveAt(0);

                    Geometry.Spatial.Plane plane_Top = Geometry.Spatial.Plane.WorldXY.GetMoved(new Vector3D(0, 0, tuple.Item1)) as Geometry.Spatial.Plane;

                    Geometry.Planar.Point2D point2D = tuple.Item2;

                    List<Tuple<Geometry.Planar.BoundingBox2D, Geometry.Planar.Polygon2D>> tuples_Polygon2D_External = tuples_Polygon2D.FindAll(x => x.Item1.Inside(point2D, tolerance)).FindAll(x => x.Item2.Inside(point2D, tolerance));
                    tuples_Polygon2D_External.Sort((x, y) => x.Item1.GetArea().CompareTo(y.Item1.GetArea()));

                    Tuple<Geometry.Planar.BoundingBox2D, Geometry.Planar.Polygon2D> tuple_Polygon2D_External = tuples_Polygon2D_External.FirstOrDefault();
                    if (tuple_Polygon2D_External == null)
                    {
                        continue;
                    }

                    List<Tuple<Geometry.Planar.BoundingBox2D, Geometry.Planar.Polygon2D>> tuples_Polygon2D_Internal = new List<Tuple<Geometry.Planar.BoundingBox2D, Geometry.Planar.Polygon2D>>();
                    foreach (Tuple<Geometry.Planar.BoundingBox2D, Geometry.Planar.Polygon2D> tuple_Polygon2D in tuples_Polygon2D)
                    {
                        if (tuple_Polygon2D == tuple_Polygon2D_External)
                        {
                            continue;
                        }
                        
                        if (tuple_Polygon2D_External.Item1.Inside(tuple_Polygon2D.Item1, tolerance) && tuple_Polygon2D_External.Item2.Inside(tuple_Polygon2D.Item2, tolerance))
                        {
                            tuples_Polygon2D_Internal.Add(tuple_Polygon2D);
                        }
                    }

                    List<Geometry.Planar.Face2D> face2Ds = Geometry.Planar.Query.Difference(new Geometry.Planar.Face2D(tuple_Polygon2D_External.Item2), tuples_Polygon2D_Internal.ConvertAll(x => new Geometry.Planar.Face2D(x.Item2)), tolerance);
                    if (face2Ds == null || face2Ds.Count == 0)
                    {
                        continue;
                    }


                    foreach (Geometry.Planar.Face2D face2D in face2Ds)
                    {
                        tuples_Space.RemoveAll(x => face2D.Inside(x.Item2, tolerance));
                    }

                    List<Face3D> face3Ds = new List<Face3D>();
                    face3Ds.AddRange(face2Ds.ConvertAll(x => new Face3D(plane_Bottom, x)));
                    face3Ds.AddRange(face2Ds.ConvertAll(x => new Face3D(plane_Top, x)));

                    List<Shell> shells_Temp = Geometry.Spatial.Create.Shells(face3Ds, tolerance);
                    if (shells_Temp == null || shells_Temp.Count == 0)
                        continue;

                    result.AddRange(shells_Temp);
                }
            }

            return result;
        }
    }
}