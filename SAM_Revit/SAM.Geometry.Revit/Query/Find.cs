using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using SAM.Geometry.Spatial;
using System.Linq;
using SAM.Core;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static T Find<T>(Document document, IntegerId integerId, double min = Tolerance.Distance, double max = Tolerance.MacroDistance) where T : Element
        {
            if(document == null || integerId == null)
            {
                return null;
            }

            T result = Core.Revit.Query.Element<T>(document, integerId);
            if (result != null)
            {
                return result;
            }

            if (!integerId.TryGetValue(RevitIdParameter.Location, out ISAMGeometry3D sAMGeometry))
            {
                return null;
            }

            List<Element> elements = null;

            if (integerId.TryGetValue(Core.Revit.RevitIdParameter.CategoryId, out int categoryId) && categoryId != -1)
            {
                BuiltInCategory builtInCategory = (BuiltInCategory)categoryId;

                elements = new FilteredElementCollector(document).OfCategory(builtInCategory).ToList();
            }

            return Find<T>(elements, sAMGeometry, min, max);
        }
        
        public static T Find<T>(this IEnumerable<Element> elements, ISAMGeometry3D sAMGeometry3D, double min = Tolerance.Distance, double max = Tolerance.MacroDistance) where T : Element
        {
            if(elements == null || sAMGeometry3D == null)
            {
                return null;
            }

            if(sAMGeometry3D is Point3D)
            {
                return Find<T>(elements, (Point3D)sAMGeometry3D, min, max);
            }

            if(sAMGeometry3D is ISegmentable3D)
            {
                return Find<T>(elements, (ISegmentable3D)sAMGeometry3D, min, max);
            }

            return null;
        }

        public static T Find<T>(this IEnumerable<Element> elements, Point3D point3D, double min = Tolerance.Distance, double max = Tolerance.MacroDistance) where T : Element
        {
            if(elements == null || point3D == null)
            {
                return null;
            }

            List<Tuple<T, ISAMGeometry3D, double>> tuples = new List<Tuple<T, ISAMGeometry3D, double>>();
            foreach(Element element in elements)
            {
                T t = element as T;
                if (t == null)
                {
                    continue;
                }

                ISAMGeometry3D sAMGeometry3D = element?.Location();
                if(sAMGeometry3D == null)
                {
                    continue;
                }

                if(sAMGeometry3D is Point3D)
                {
                    Point3D point3D_Temp = (Point3D)sAMGeometry3D;
                    double distance = point3D.Distance(point3D_Temp);
                    if (distance <= min)
                    {
                        return t;
                    }

                    if(distance > max)
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<T, ISAMGeometry3D, double>(t, point3D_Temp, distance));
                }
                else if(sAMGeometry3D is ISegmentable3D)
                {
                    ISegmentable3D segmentable3D = (ISegmentable3D)sAMGeometry3D;
                    double distance = segmentable3D.Distance(point3D);
                    if (distance <= min)
                    {
                        return t;
                    }

                    if (distance > max)
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<T, ISAMGeometry3D, double>(t, segmentable3D, distance));
                }
            }

            if(tuples == null || tuples.Count == 0)
            {
                return null;
            }

            if(tuples.Count == 1)
            {
                return tuples[0].Item1;
            }

            tuples.Sort((x, y) => x.Item3.CompareTo(y.Item3));

            return tuples.First().Item1;
        }

        public static T Find<T>(this IEnumerable<Element> elements, ISegmentable3D segmentable3D, double min = Tolerance.Distance, double max = Tolerance.MacroDistance) where T: Element
        {
            if (elements == null || segmentable3D == null)
            {
                return null;
            }

            List<Tuple<T, ISAMGeometry3D, double>> tuples = new List<Tuple<T, ISAMGeometry3D, double>>();
            foreach (Element element in elements)
            {
                T t = element as T;
                if(t == null)
                {
                    continue;
                }

                ISAMGeometry3D sAMGeometry3D = element?.Location();
                if (sAMGeometry3D == null)
                {
                    continue;
                }

                if (sAMGeometry3D is Point3D)
                {
                    Point3D point3D_Temp = (Point3D)sAMGeometry3D;
                    double distance = segmentable3D.Distance(point3D_Temp);
                    if (distance <= min)
                    {
                        return t;
                    }

                    if (distance > max)
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<T, ISAMGeometry3D, double>(t, point3D_Temp, distance));
                }
                else if (sAMGeometry3D is ISegmentable3D)
                {
                    ISegmentable3D segmentable3D_Temp = (ISegmentable3D)sAMGeometry3D;
                    double distance = segmentable3D.Distance(segmentable3D_Temp, min);
                    if (distance <= min)
                    {
                        return t;
                    }

                    if (distance > max)
                    {
                        continue;
                    }

                    tuples.Add(new Tuple<T, ISAMGeometry3D, double>(t, segmentable3D, distance));
                }
            }

            if (tuples == null || tuples.Count == 0)
            {
                return null;
            }

            if (tuples.Count == 1)
            {
                return tuples[0].Item1;
            }

            tuples.Sort((x, y) => x.Item3.CompareTo(y.Item3));

            return tuples.First().Item1;
        }
    }
}