using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static List<Wall> TrimOrExtendWall(this IEnumerable<Wall> walls, double maxDistance, double tolerance = Core.Tolerance.Distance)
        {
            Dictionary<double, Dictionary<Wall, Segment2D>> dictionary = new Dictionary<double, Dictionary<Wall, Segment2D>>();
            foreach (Wall wall in walls)
            {
                Curve curve = (wall.Location as LocationCurve).Curve;
                Segment3D segment3D = Geometry.Revit.Convert.ToSAM_Segment3D(curve);

                double elevation = Math.Min(segment3D[0].Z, segment3D[1].Z);

                Dictionary<Wall, Segment2D> dictionary_Wall = null;
                if (!dictionary.TryGetValue(elevation, out dictionary_Wall))
                {
                    dictionary_Wall = new Dictionary<Wall, Segment2D>();
                    dictionary[elevation] = dictionary_Wall;
                }

                dictionary_Wall[wall] = Geometry.Spatial.Plane.Base.Convert(segment3D);
            }

            List<Wall> result = new List<Wall>();

            foreach (KeyValuePair<double, Dictionary<Wall, Segment2D>> keyValuePair in dictionary)
            {
                List<Segment2D> segment2Ds = keyValuePair.Value.Values.ToList();

                //Filtering Walls by Level
                List<Tuple<Wall, Segment2D, List<int>, bool>> tupleList = new List<Tuple<Wall, Segment2D, List<int>, bool>>();
                foreach (KeyValuePair<Wall, Segment2D> keyValuePair_Wall in keyValuePair.Value)
                {
                    LocationCurve locationCurve = keyValuePair_Wall.Key.Location as LocationCurve;

                    List<int> indexes = new List<int>();

                    ElementArray elementArray = null;

                    elementArray = locationCurve.get_ElementsAtJoin(0);
                    if (elementArray == null || elementArray.Size == 0)
                        indexes.Add(0);

                    elementArray = locationCurve.get_ElementsAtJoin(1);
                    if (elementArray == null || elementArray.Size == 0)
                        indexes.Add(1);

                    //if (indexes.Count > 0)
                        tupleList.Add(new Tuple<Wall, Segment2D, List<int>, bool>(keyValuePair_Wall.Key, keyValuePair_Wall.Value, indexes, false));
                }

                //Seeking for walls to be extended/trimmed
                bool updated = true;
                while (updated)
                {
                    updated = false;
                    List<Tuple<Wall, Segment2D, List<int>, bool>> tupleList_Unconnected = tupleList.FindAll(x => x.Item3 != null && x.Item3.Count > 0);
                    for (int i = 0; i < tupleList_Unconnected.Count; i++)
                    {
                        Tuple<Wall, Segment2D, List<int>, bool> tuple = tupleList_Unconnected[i];

                        List<Tuple<Wall, Segment2D, List<int>, bool>> tupleList_Temp = new List<Tuple<Wall, Segment2D, List<int>, bool>>(tupleList);
                        tupleList_Temp.Remove(tuple);

                        Segment2D segment2D = tuple.Item2;
                        List<Tuple<Point2D, Segment2D>> tupleList_Intersection = new List<Tuple<Point2D, Segment2D>>();
                        foreach (Segment2D segment2D_Temp in tupleList_Temp.ConvertAll(x => x.Item2))
                        {
                            Point2D point2D_Intersection = segment2D_Temp.Intersection(segment2D, false, tolerance);
                            if (point2D_Intersection == null)
                                continue;

                            double distance = segment2D.Distance(point2D_Intersection);

                            if (distance <= maxDistance)
                                tupleList_Intersection.Add(new Tuple<Point2D, Segment2D>(point2D_Intersection, segment2D_Temp));
                        }

                        if (tupleList_Intersection.Count == 0)
                            continue;

                        foreach (int index in tuple.Item3)
                        {
                            Point2D point2D = segment2D[index];

                            tupleList_Intersection.Sort((x, y) => x.Item1.Distance(point2D).CompareTo(y.Item1.Distance(point2D)));
                            Tuple<Point2D, Segment2D> tuple_Intersection = tupleList_Intersection.Find(x => x.Item1.Distance(point2D) < maxDistance);
                            if (tuple_Intersection == null)
                                continue;

                            Segment2D segment2D_Intersection = tuple_Intersection.Item2;

                            int j = tupleList.FindIndex(x => x.Item2 == segment2D_Intersection);
                            if (j == -1)
                                continue;

                            int k = tupleList.FindIndex(x => x.Item2 == segment2D);
                            if (k == -1)
                                continue;

                            if(segment2D_Intersection[0].Distance(tuple_Intersection.Item1) < maxDistance || segment2D_Intersection[1].Distance(tuple_Intersection.Item1) < maxDistance)
                            {
                                segment2D_Intersection.Adjust(tuple_Intersection.Item1);
                                tupleList[j] = new Tuple<Wall, Segment2D, List<int>, bool>(tupleList[j].Item1, segment2D_Intersection, tupleList[j].Item3.FindAll(x => x != segment2D_Intersection.GetEndIndex(tuple_Intersection.Item1)), true);
                            }
                                
                            segment2D.Adjust(tuple_Intersection.Item1);
                            tupleList[k] = new Tuple<Wall, Segment2D, List<int>, bool>(tuple.Item1, segment2D, tuple.Item3.FindAll(x => x != index), true);
                            
                            updated = true;
                            break;
                        }

                        if (updated)
                            break;
                    }
                }

                tupleList.RemoveAll(x => !x.Item4);

                //Updating Revit Walls
                foreach (Tuple<Wall, Segment2D, List<int>, bool> tuple in tupleList)
                {
                    LocationCurve locationCurve = tuple.Item1.Location as LocationCurve;

                    Segment2D segment2D = tuple.Item2;

                    Segment3D segment3D = new Segment3D(new Point3D(segment2D[0].X, segment2D[0].Y, keyValuePair.Key), new Point3D(segment2D[1].X, segment2D[1].Y, keyValuePair.Key));

                    Line line = Geometry.Revit.Convert.ToRevit(segment3D);

                    locationCurve.Curve = line;

                    result.Add(tuple.Item1);
                }
                
            }

            return result;
        }
    }
}
