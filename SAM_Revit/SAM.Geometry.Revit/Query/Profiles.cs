using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<IClosed3D> Profiles(this HostObject hostObject)
        {
            if (hostObject == null || hostObject.Document == null)
                return null;

            if (hostObject is Floor)
                return Profiles_Floor((Floor)hostObject);

            if (hostObject is RoofBase)
                return Profiles_RoofBase((RoofBase)hostObject);

            if (hostObject is Ceiling)
                return Profiles_Ceiling((Ceiling)hostObject);

            if (hostObject is Wall)
                return Profiles_Wall((Wall)hostObject);

            return null;
        }

        public static List<IClosed3D> Profiles_Wall(this Wall wall)
        {
            List<IClosed3D> result = Profiles_FromSketch(wall);
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

                    double max = Units.Query.ToSI(boundingBoxXYZ.Max.Z, Units.UnitType.Feet);
                    Spatial.Plane plane_max = new Spatial.Plane(new Point3D(0, 0, max), new Vector3D(0, 0, 1));

                    double min = Units.Query.ToSI(boundingBoxXYZ.Min.Z, Units.UnitType.Feet);
                    Spatial.Plane plane_min = new Spatial.Plane(new Point3D(0, 0, min), new Vector3D(0, 0, 1));

                    result = new List<IClosed3D>();
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

                        Segment3D segment3D_1 = new Segment3D(point3D_1, point3D_2);
                        Segment3D segment3D_2 = new Segment3D(point3D_3, minCurve.GetStart());

                        result.Add(new PolycurveLoop3D(new ICurve3D[] { minCurve, segment3D_1, maxCurve, segment3D_2 }));
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

            result = new List<IClosed3D>();
            foreach (CurveLoop curveLoop in curveLoops)
            {
                PolycurveLoop3D polycurveLoop3D = curveLoop.ToSAM();
                if (polycurveLoop3D != null)
                    result.Add(polycurveLoop3D);
            }

            return result;
        }

        private static List<IClosed3D> Profiles_Floor(this Floor floor)
        {
            List<IClosed3D> closed3Ds = Profiles_FromSketch(floor);
            if (closed3Ds == null || closed3Ds.Count() == 0)
                closed3Ds = TopProfiles(floor);

            return closed3Ds;
        }

        private static List<IClosed3D> Profiles_RoofBase(this RoofBase roofBase)
        {
            List<IClosed3D> closed3Ds = Profiles_FromSketch(roofBase);
            if(closed3Ds != null && closed3Ds.Count() > 0)
            {
                double offset = 0;

                Parameter parameter = roofBase.get_Parameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM);
                if(parameter != null)
                {
                    offset = parameter.AsDouble();
                    if (!double.IsNaN(offset))
                        offset = UnitUtils.ConvertFromInternalUnits(offset, DisplayUnitType.DUT_METERS);
                }
                
                if(offset != 0)
                {
                    Vector3D vector3D = new Vector3D(0, 0, offset);
                    closed3Ds = closed3Ds.ConvertAll(x => (IClosed3D)x.GetMoved(vector3D));
                }
            }
            else
            {
                closed3Ds = TopProfiles(roofBase);
            }                

            return closed3Ds;
        }

        private static List<IClosed3D> Profiles_Ceiling(this Ceiling ceiling)
        {
            List<IClosed3D> closed3Ds = Profiles_FromSketch(ceiling);
            if (closed3Ds == null || closed3Ds.Count() == 0)
                closed3Ds = BottomProfiles(ceiling);

            return closed3Ds;
        }

        private static List<IClosed3D> Profiles_FromSketch(this HostObject hostObject)
        {
            IEnumerable<ElementId> elementIds = hostObject.GetDependentElements(new ElementClassFilter(typeof(Sketch)));
            if (elementIds == null || elementIds.Count() == 0)
                return null;

            Document document = hostObject.Document;

            List<IClosed3D> result = new List<IClosed3D>();
            foreach (ElementId elementId in elementIds)
            {
                Sketch sketch = document.GetElement(elementId) as Sketch;
                if (sketch == null)
                    continue;

                List<IClosed3D> closed3Ds = Convert.ToSAM(sketch);
                if (closed3Ds == null)
                    continue;

                foreach (IClosed3D closed3D in closed3Ds)
                    if (closed3D != null)
                        result.Add(closed3D);
            }

            return result;
        }
    }
}
