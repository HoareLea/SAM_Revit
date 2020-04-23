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

                    if (indexes.Count > 0)
                        tupleList.Add(new Tuple<Wall, Segment2D, List<int>, bool>(keyValuePair_Wall.Key, keyValuePair_Wall.Value, indexes, false));
                }

                //Seeking for walls to be extended/trimmed
                bool updated = true;
                while (updated)
                {
                    updated = false;
                    for (int i = 0; i < tupleList.Count; i++)
                    {
                        Tuple<Wall, Segment2D, List<int>, bool> tuple = tupleList[i];

                        Vector2D direction = tuple.Item2.Direction;
                        Vector2D direction_Negated = direction.GetNegated();

                        Segment2D segment2D = new Segment2D(tuple.Item2);

                        foreach (int index in tuple.Item3)
                        {
                            Vector2D vector2D_1 = Geometry.Planar.Query.TraceFirst(tuple.Item2[index], direction, segment2Ds);
                            if (vector2D_1 != null && vector2D_1.Length > maxDistance)
                                vector2D_1 = null;

                            Vector2D vector2D_2 = Geometry.Planar.Query.TraceFirst(tuple.Item2[index], direction_Negated, segment2Ds);
                            if (vector2D_2 != null && vector2D_2.Length > maxDistance)
                                vector2D_2 = null;

                            if (vector2D_1 == null && vector2D_2 == null)
                                continue;

                            Vector2D vector2D;
                            if (vector2D_1 == null)
                            {
                                vector2D = vector2D_2;
                            }
                            else if (vector2D_2 == null)
                            {
                                vector2D = vector2D_1;
                            }
                            else
                            {
                                if (vector2D_1.Length < vector2D_2.Length)
                                    vector2D = vector2D_1;
                                else
                                    vector2D = vector2D_2;
                            }

                            if (index == 0)
                                segment2D = new Segment2D(segment2D[0].GetMoved(vector2D), segment2D[1]);
                            else
                                segment2D = new Segment2D(segment2D[0], segment2D[1].GetMoved(vector2D));
                        }

                        if (segment2D[0] == tuple.Item2[0] && segment2D[1] == tuple.Item2[1])
                            continue;

                        if (segment2D.GetLength() < tolerance)
                            continue;

                        if (segment2D.AlmostSimilar(tuple.Item2, tolerance))
                            continue;

                        tupleList[i] = new Tuple<Wall, Segment2D, List<int>, bool>(tuple.Item1, segment2D, tuple.Item3, true);
                        updated = true;
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
