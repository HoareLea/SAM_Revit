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
            List<IClosed3D> result = null;

            BoundingBoxXYZ boundingBoxXYZ = wall.get_BoundingBox(null);
            if (boundingBoxXYZ != null)
            {
                LocationCurve locationCurve = wall.Location as LocationCurve;
                if (locationCurve != null)
                {
                    ICurve3D curve = Convert.ToSAM(locationCurve);
                    if (curve != null)
                    {
                        Spatial.Plane plane = null;

                        double max = Units.Convert.ToSI(boundingBoxXYZ.Max.Z, Units.UnitType.Feet);
                        plane = new Spatial.Plane(new Point3D(0, 0, max), new Vector3D(0, 0, 1));
                        ICurve3D maxCurve = plane.Project(curve);

                        double min = Units.Convert.ToSI(boundingBoxXYZ.Min.Z, Units.UnitType.Feet);
                        plane = new Spatial.Plane(new Point3D(0, 0, min), new Vector3D(0, 0, 1));
                        ICurve3D minCurve = plane.Project(curve);

                        Point3D point3D_1;
                        Point3D point3D_2;
                        Point3D point3D_3;

                        point3D_1 = minCurve.GetEnd();
                        point3D_2 = maxCurve.GetStart();
                        point3D_3 = maxCurve.GetEnd();
                        if (point3D_1.Distance(point3D_3) < point3D_1.Distance(point3D_2))
                        {
                            Point3D point_Temp = point3D_2;

                            maxCurve.Reverse();
                            point3D_2 = point3D_3;
                            point3D_3 = point_Temp;
                        }

                        Segment3D segment3D_1 = new Segment3D(point3D_1, point3D_2);
                        Segment3D segment3D_2 = new Segment3D(point3D_3, minCurve.GetStart());

                        result = new List<IClosed3D>();
                        result.Add(new PolycurveLoop3D(new ICurve3D[] { minCurve, segment3D_1, maxCurve, segment3D_2 }));
                        return result;
                    }
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
            return TopProfiles(floor);
        }

        private static List<IClosed3D> Profiles_RoofBase(this RoofBase roofBase)
        {
            return TopProfiles(roofBase);
        }

        private static List<IClosed3D> Profiles_Ceiling(this Ceiling ceiling)
        {
            return BottomProfiles(ceiling);
        }

        private static List<IClosed3D> Profiles(this Autodesk.Revit.DB.Face face)
        {
            if (face is PlanarFace)
                return face.ToSAM_PolycurveLoop3Ds().Cast<IClosed3D>().ToList();

            return face.Triangulate().ToSAM_Triangle3Ds().Cast<IClosed3D>().ToList();
        }

        private static List<IClosed3D> Profiles(this IEnumerable<Autodesk.Revit.DB.Face> faces)
        {
            List<IClosed3D> result = new List<IClosed3D>();
            foreach (Autodesk.Revit.DB.Face face in faces)
                result.AddRange(face.Profiles());

            return result;
        }
    }
}
