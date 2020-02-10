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
        public static List<Spatial.Face> Profiles(this HostObject hostObject)
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

            if (hostObject is FaceWall)
                return Profiles_FaceWall((FaceWall)hostObject);

            return null;
        }

        public static List<Spatial.Face> Profiles_FaceWall(this FaceWall faceWall)
        {
            GeometryElement geometryElement = faceWall.get_Geometry(new Options());
            if (geometryElement == null)
                return null;

            List<Spatial.Face> result = new List<Spatial.Face>();
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

            return result;
        }

        public static List<Spatial.Face> Profiles_Wall(this Wall wall)
        {
            List<Spatial.Face> result = Profiles_FromSketch(wall);
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

                    result = new List<Spatial.Face>();
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

                        result.Add(new Spatial.Face(new Polygon3D(new Point3D[] { point3D_1, point3D_2, point3D_3, minCurve.GetStart() })));
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

            result = new List<Spatial.Face>();
            foreach (CurveLoop curveLoop in curveLoops)
            {
                Polygon3D polygon3D = curveLoop.ToSAM_Polygon3D();
                if (polygon3D != null)
                    result.Add(new Spatial.Face(polygon3D));
            }

            return result;
        }

        private static List<Spatial.Face> Profiles_Floor(this Floor floor)
        {
            //List<IClosed3D> closed3Ds = Profiles_FromSketch(floor);
            //if (closed3Ds == null || closed3Ds.Count() == 0)
            //    closed3Ds = TopProfiles(floor);

            //return closed3Ds;

            List<Spatial.Face> faces = TopProfiles(floor);
            

      
            return faces;
        }

        private static List<Spatial.Face> Profiles_RoofBase(this RoofBase roofBase)
        {
        //    List<IClosed3D> closed3Ds = Profiles_FromSketch(roofBase);
        //    if(closed3Ds != null && closed3Ds.Count() > 0)
        //    {
        //        double offset = 0;

        //        Parameter parameter = roofBase.get_Parameter(BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM);
        //        if(parameter != null)
        //        {
        //            offset = parameter.AsDouble();
        //            if (!double.IsNaN(offset))
        //                offset = UnitUtils.ConvertFromInternalUnits(offset, DisplayUnitType.DUT_METERS);
        //        }
                
        //        if(offset != 0)
        //        {
        //            Vector3D vector3D = new Vector3D(0, 0, offset);
        //            closed3Ds = closed3Ds.ConvertAll(x => (IClosed3D)x.GetMoved(vector3D));
        //        }
        //    }
        //    else
        //    {
        //        closed3Ds = TopProfiles(roofBase);
        //    }                

        //    return closed3Ds;

            return TopProfiles(roofBase);
        }

        private static List<Spatial.Face> Profiles_Ceiling(this Ceiling ceiling)
        {
        //    List<IClosed3D> closed3Ds = Profiles_FromSketch(ceiling);
        //    if (closed3Ds == null || closed3Ds.Count() == 0)
        //        closed3Ds = BottomProfiles(ceiling);

        //    return closed3Ds;

            return BottomProfiles(ceiling);
        }

        private static List<Spatial.Face> Profiles_FromSketch(this HostObject hostObject)
        {
            IEnumerable<ElementId> elementIds = hostObject.GetDependentElements(new ElementClassFilter(typeof(Sketch)));
            if (elementIds == null || elementIds.Count() == 0)
                return null;

            Document document = hostObject.Document;

            List<Spatial.Face> result = new List<Spatial.Face>();
            foreach (ElementId elementId in elementIds)
            {
                Sketch sketch = document.GetElement(elementId) as Sketch;
                if (sketch == null)
                    continue;

                List<Spatial.Face> faces = Convert.ToSAM_Faces(sketch);
                if (faces == null)
                    continue;

                foreach (Spatial.Face face in faces)
                    if (face != null)
                        result.Add(face);
            }

            return result;
        }
    }
}
