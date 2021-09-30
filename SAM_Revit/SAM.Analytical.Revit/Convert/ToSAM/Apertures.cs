using Autodesk.Revit.DB;
using SAM.Core.Revit;
using SAM.Geometry.Planar;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<Aperture> ToSAM_Apertures(this FamilyInstance familyInstance, ConvertSettings convertSettings)
        {
            if (familyInstance == null)
                return null;

            List<Aperture> result = convertSettings?.GetObjects<Aperture>(familyInstance.Id);
            if (result != null)
                return result;

            if (Core.Revit.Query.Simplified(familyInstance))
            {
                result = Core.Revit.Query.IJSAMObjects<Aperture>(familyInstance);
                if (result != null)
                {
                    convertSettings?.Add(familyInstance.Id, result);
                    return result;
                }
            }

            Point3D point3D_Location = Geometry.Revit.Query.Location(familyInstance);
            if (point3D_Location == null)
            {
                List<Solid> solids = Core.Revit.Query.Solids(familyInstance, new Options());
                solids?.RemoveAll(x => x.Volume == 0);
                if (solids == null || solids.Count == 0)
                    return null;

                if (solids.Count > 1)
                    solids.Sort((x, y) => y.Volume.CompareTo(x.Volume));

                point3D_Location = solids[0].ComputeCentroid()?.ToSAM();
            }

            if (point3D_Location == null)
                return null;

            List<Face3D> face3Ds = null;

            PanelType panelType_Host = PanelType.Undefined;
            BuiltInCategory builtInCategory_Host = BuiltInCategory.INVALID;
            if (familyInstance.Host != null)
            {
                HostObject hostObject = familyInstance.Host as HostObject;
                if (hostObject != null)
                {
                    builtInCategory_Host = (BuiltInCategory)hostObject.Category.Id.IntegerValue;

                    Geometry.Spatial.Plane plane_Host = null;
                    if (hostObject is CurtainSystem && familyInstance is Autodesk.Revit.DB.Panel)
                    {
                        Autodesk.Revit.DB.Panel panel = (Autodesk.Revit.DB.Panel)familyInstance;
                        ElementId uGridLineElementId = null;
                        ElementId vGridLineElementId = null;

                        panel.GetRefGridLines(ref uGridLineElementId, ref vGridLineElementId);

                        CurtainSystem curtainSystem = (CurtainSystem)hostObject;

                        List<Polygon3D> polygon3Ds = curtainSystem.CurtainCell(uGridLineElementId, vGridLineElementId)?.Polygon3Ds();
                        if (polygon3Ds != null && polygon3Ds.Count != 0)
                        {
                            polygon3Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
                            plane_Host = polygon3Ds[0].GetPlane();
                        }
                    }
                    else
                    {
                        List<Face3D> face3Ds_Temp = hostObject.Profiles();
                        if (face3Ds_Temp != null && face3Ds_Temp.Count != 0)
                        {
                            plane_Host = face3Ds_Temp.Closest(point3D_Location)?.GetPlane();
                        }
                    }

                    if (plane_Host != null)
                        point3D_Location = plane_Host.Project(point3D_Location);

                    HostObjAttributes hostObjAttributes = familyInstance.Document.GetElement(hostObject.GetTypeId()) as HostObjAttributes;
                    if (hostObjAttributes != null)
                        panelType_Host = hostObjAttributes.PanelType();

                    if (panelType_Host == PanelType.Undefined)
                        panelType_Host = hostObject.PanelType();

                    //Method 1 of extracting Geometry
                    face3Ds = hostObject.Profiles(familyInstance.Id);
                }
            }

            ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction(familyInstance, convertSettings);
            if (apertureConstruction == null && panelType_Host != PanelType.Undefined)
                apertureConstruction = Analytical.Query.DefaultApertureConstruction(panelType_Host, familyInstance.ApertureType()); //Default Aperture Construction

            Vector3D axisX = null;
            Vector3D normal = null;
            Vector3D axisY = null;
            if (builtInCategory_Host == BuiltInCategory.OST_Roofs)
            {
                axisX = familyInstance.HandOrientation.ToSAM_Vector3D(false);
                axisY = familyInstance.FacingOrientation.ToSAM_Vector3D(false);
                normal = Geometry.Spatial.Query.AxisY(axisY, axisX);
            }
            else
            {
                axisX = familyInstance.HandOrientation.ToSAM_Vector3D(false);
                normal = familyInstance.FacingOrientation.ToSAM_Vector3D(false);
                axisY = Geometry.Spatial.Query.AxisY(normal, axisX);
            }


            Geometry.Spatial.Plane plane = Geometry.Spatial.Create.Plane(point3D_Location, axisX, axisY);
            if (!plane.Normal.SameHalf(normal))
                plane.FlipZ(false);

            //Method 2 of extracting Geometry
            if (face3Ds == null || face3Ds.Count == 0)
            {
                List<Point2D> point2Ds = null;

                List<ISegmentable3D> segmentable3Ds = Geometry.Revit.Convert.ToSAM_Geometries<ISegmentable3D>(familyInstance, true);
                if ((segmentable3Ds == null || segmentable3Ds.Count == 0) && familyInstance is Autodesk.Revit.DB.Panel)
                {
                    List<Shell> shells = Geometry.Revit.Convert.ToSAM_Geometries<Shell>(familyInstance, true);
                    if (shells != null && shells.Count > 0)
                    {
                        foreach (Shell shell in shells)
                        {
                            List<ISegmentable3D> segmentable3Ds_Temp = shell?.GetEdge3Ds()?.ConvertAll(x => x as ISegmentable3D);
                            if (segmentable3Ds_Temp != null)
                            {
                                segmentable3Ds_Temp.ForEach(x => segmentable3Ds.AddRange(x.GetSegments()));
                            }
                        }
                    }
                }

                if (segmentable3Ds != null)
                {
                    point2Ds = new List<Point2D>();
                    List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();
                    foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                    {
                        ICurve3D curve3D = plane.Project(segmentable3D as ICurve3D);
                        if (curve3D == null)
                        {
                            continue;
                        }

                        ISegmentable2D segmentable2D = plane.Convert(curve3D) as ISegmentable2D;
                        if (segmentable2D == null)
                        {
                            continue;
                        }


                        List<Point2D> point2Ds_Temp = segmentable2D?.GetPoints();
                        if (point2Ds_Temp != null && point2Ds_Temp.Count > 0)
                        {
                            point2Ds_Temp.ForEach(x => point2Ds.Add(x));
                        }
                    }
                }

                if (point2Ds == null || point2Ds.Count < 3)
                {
                    return result;
                }

                Face3D face3D = new Face3D(plane, Geometry.Planar.Create.Rectangle2D(point2Ds));
                if (face3D != null && face3D.IsValid() && face3D.GetArea() > Core.Tolerance.MacroDistance)
                {
                    face3Ds = new List<Face3D>() { face3D };
                }

            }

            //Method 3 of extracting Geometry
            if (face3Ds == null || face3Ds.Count == 0)
            {
                List<Shell> shells = Geometry.Revit.Convert.ToSAM_Geometries<Shell>(familyInstance);
                if (shells == null || shells.Count == 0)
                    return null;

                List<Point2D> point2Ds = new List<Point2D>();
                foreach (Shell shell in shells)
                {
                    List<Face3D> face3Ds_Shell = shell?.Face3Ds;
                    if (face3Ds_Shell == null || face3Ds_Shell.Count == 0)
                    {
                        continue;
                    }

                    foreach (Face3D face3D_Temp in face3Ds_Shell)
                    {
                        ISegmentable3D segmentable3D = face3D_Temp.GetExternalEdge3D() as ISegmentable3D;
                        if (segmentable3D == null)
                        {
                            continue;
                        }

                        segmentable3D?.GetPoints()?.ForEach(x => point2Ds.Add(plane.Convert(x)));
                    }
                }

                Face3D face3D = new Face3D(plane, Geometry.Planar.Create.Rectangle2D(point2Ds));
                if (face3D != null && face3D.IsValid() && face3D.GetArea() > Core.Tolerance.MacroDistance)
                {
                    face3Ds = new List<Face3D>() { face3D };
                }
            }

            if(face3Ds == null || face3Ds.Count == 0)
            {
                return result;
            }

            //TODO: Working on SAM Families (requested by Michal)

            string parameterName_Height = Core.Revit.Query.Name(ActiveSetting.Setting, typeof(Aperture), typeof(FamilyInstance), "GetHeight");
            string parameterName_Width = Core.Revit.Query.Name(ActiveSetting.Setting, typeof(Aperture), typeof(FamilyInstance), "GetWidth");
            if (!string.IsNullOrWhiteSpace(parameterName_Height) && !string.IsNullOrWhiteSpace(parameterName_Width))
            {
                Parameter parameter_Height = familyInstance.LookupParameter(parameterName_Height);
                Parameter parameter_Width = familyInstance.LookupParameter(parameterName_Width);
                if (parameter_Height != null && parameter_Width != null && parameter_Height.HasValue && parameter_Width.HasValue && parameter_Height.StorageType == StorageType.Double && parameter_Width.StorageType == StorageType.Double)
                {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                    double height = UnitUtils.ConvertFromInternalUnits(parameter_Height.AsDouble(), DisplayUnitType.DUT_METERS);
                    double width = UnitUtils.ConvertFromInternalUnits(parameter_Width.AsDouble(), DisplayUnitType.DUT_METERS);
#else
                    double height = UnitUtils.ConvertFromInternalUnits(parameter_Height.AsDouble(), UnitTypeId.Meters);
                    double width = UnitUtils.ConvertFromInternalUnits(parameter_Width.AsDouble(), UnitTypeId.Meters);
#endif
                }
            }

            result = new List<Aperture>();
            foreach(Face3D face3D_Temp in face3Ds)
            {
                Aperture aperture = Analytical.Create.Aperture(apertureConstruction, face3D_Temp);
                if(aperture == null)
                {
                    continue;
                }

                aperture.UpdateParameterSets(familyInstance, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
                result.Add(aperture);
            }

            convertSettings?.Add(familyInstance.Id, result);

            return result;
        }
    }
}