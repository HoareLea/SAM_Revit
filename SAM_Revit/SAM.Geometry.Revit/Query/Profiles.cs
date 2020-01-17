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
        public static List<IClosed3D> Profiles(this HostObject hostObject, Transaction transaction = null)
        {
            if (hostObject == null || hostObject.Document == null)
                return null;

            if (hostObject is Floor)
                return Profiles_Floor((Floor)hostObject);

            if (hostObject is RoofBase)
                return Profiles_RoofBase((RoofBase)hostObject);

            if (hostObject is Ceiling)
                return Profiles_Ceiling((Ceiling)hostObject);

            List<IClosed3D> result = Profiles_FromSketch(hostObject, transaction);
            if (result != null && result.Count > 0)
                return result;

            if (hostObject is Wall)
                return Profiles_Wall((Wall)hostObject);

            return null;
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

        public static List<IClosed3D> Profiles_Wall(this Wall wall)
        {
            List<IClosed3D> result = null;

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

        private static List<IClosed3D> Profiles_FromSketch(this HostObject hostObject, Transaction transaction = null)
        {
            Document document = hostObject.Document;

            IEnumerable<ElementId> elementIds = GetRelatedElementIds(hostObject, transaction, false);

            if (elementIds == null || elementIds.Count() == 0)
                return null;

            List<IClosed3D> result = new List<IClosed3D>();
            foreach (ElementId id in elementIds)
            {
                Sketch sketch = document.GetElement(id) as Sketch;
                if (sketch == null)
                    continue;

                if (sketch.Profile == null)
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

        private static IEnumerable<ElementId> GetRelatedElementIds(this Element element, Transaction transaction = null, bool IncludeInserts = true)
        {
            Document document = element.Document;

            IEnumerable<ElementId> elementIds = null;

            bool close = false;
            if (transaction == null)
            {
                transaction = new Transaction(document, "Temp");
                transaction.Start();
                close = true;
            }

            using (SubTransaction subTrasaction = new SubTransaction(document))
            {
                
                //IMPORTANT: have to be two separate transactions othewise HostObject become Invalid

                subTrasaction.Start();

                try
                {
                    elementIds = document.Delete(element.Id);
                }
                catch
                {
                    elementIds = null;
                }

                subTrasaction.RollBack();

                if (IncludeInserts && element is HostObject && elementIds != null && elementIds.Count() > 0)
                {
                    subTrasaction.Start();
                    try
                    {
                        IList<ElementId> insertElementIDs = ((HostObject)element).FindInserts(true, true, true, true);
                        if (insertElementIDs != null && insertElementIDs.Count > 0)
                        {
                            IEnumerable<ElementId> elementIds_Temp = document.Delete(insertElementIDs);
                            if (elementIds_Temp != null && elementIds_Temp.Count() != 0)
                                elementIds = elementIds.ToList().FindAll(x => !elementIds_Temp.Contains(x));
                        }
                    }
                    catch
                    {

                    }
                    subTrasaction.RollBack();
                }
            }

            if (close)
            {
                FailureHandlingOptions failureHandlingOptions = transaction.GetFailureHandlingOptions().SetClearAfterRollback(true);
                transaction.RollBack(failureHandlingOptions);
            }

            return elementIds;
        }
    }
}
