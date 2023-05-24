using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Core.Revit;
using SAM.Geometry.Planar;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

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

            Point3D point3D_Location = Geometry.Revit.Query.LocationPoint3D(familyInstance);
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
            HostObject hostObject = null;

            if (familyInstance.Host != null)
            {
                hostObject = familyInstance.Host as HostObject;

                if (hostObject != null)
                {
                    builtInCategory_Host = (BuiltInCategory)hostObject.Category.Id.IntegerValue;
                }
            }

            //Method 1 of extracting Geometry
            if (face3Ds == null || face3Ds.Count == 0)
            {
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

            //Method 2 of extracting Geometry
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
                {
                    face3Ds = face3Ds?.ConvertAll(x => plane_Host.Project(x));
                    point3D_Location = plane_Host.Project(point3D_Location);
                }


                HostObjAttributes hostObjAttributes = familyInstance.Document.GetElement(hostObject.GetTypeId()) as HostObjAttributes;
                if (hostObjAttributes != null)
                    panelType_Host = hostObjAttributes.PanelType();

                if (panelType_Host == PanelType.Undefined)
                    panelType_Host = hostObject.PanelType();

                List<Face3D> face3Ds_Profiles = hostObject.Profiles(familyInstance.Id);
                face3Ds_Profiles?.RemoveAll(x => x == null || !x.IsValid());
                if (face3Ds_Profiles != null && face3Ds_Profiles.Count > 0)
                {
                    if(face3Ds == null || (face3Ds != null && face3Ds_Profiles.ConvertAll(x => x.GetArea()).Sum() <= face3Ds.ConvertAll(x => x.GetArea()).Sum() ))
                    {
                        face3Ds = face3Ds_Profiles;
                    }
                }
            }

            ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction(familyInstance, convertSettings);
            if (apertureConstruction == null && panelType_Host != PanelType.Undefined)
                apertureConstruction = Analytical.Query.DefaultApertureConstruction(panelType_Host, familyInstance.ApertureType()); //Default Aperture Construction

            if (face3Ds == null || face3Ds.Count == 0)
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
            foreach (Face3D face3D_Temp in face3Ds)
            {
                Aperture aperture = Analytical.Create.Aperture(apertureConstruction, face3D_Temp);
                if (aperture == null)
                {
                    continue;
                }

                aperture.UpdateParameterSets(familyInstance, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
                result.Add(aperture);
            }

            convertSettings?.Add(familyInstance.Id, result);

            if (convertSettings.UseProjectLocation)
            {
                Transform transform = Core.Revit.Query.ProjectTransform(familyInstance.Document);
                if (transform != null)
                {
                    result = result.ConvertAll(x => Query.Transform(transform, x));
                }
            }

            return result;
        }
    }
}