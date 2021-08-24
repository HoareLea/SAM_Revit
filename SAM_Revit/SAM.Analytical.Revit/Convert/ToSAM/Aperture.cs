using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Core.Revit;
using SAM.Geometry.Planar;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Aperture ToSAM(this EnergyAnalysisOpening energyAnalysisOpening, ConvertSettings convertSettings)
        {
            if (energyAnalysisOpening == null)
                return null;

            Aperture result = convertSettings?.GetObject<Aperture>(energyAnalysisOpening.Id);
            if (result != null)
                return result;

            Polygon3D polygon3D = energyAnalysisOpening.GetPolyloop().ToSAM();
            if (polygon3D == null)
                return null;

            FamilyInstance familyInstance = Core.Revit.Query.Element(energyAnalysisOpening) as FamilyInstance;
            if (familyInstance == null)
                return new Aperture(null, polygon3D);

            if (Core.Revit.Query.Simplified(familyInstance))
            {
                result = Core.Revit.Query.IJSAMObject<Aperture>(familyInstance);
                if (result != null)
                    return result;
            }

            ApertureConstruction apertureConstruction = ToSAM_ApertureConstruction(familyInstance, convertSettings);

            Point3D point3D_Location = Geometry.Revit.Query.Location(familyInstance);
            if (point3D_Location == null)
                return null;

            result = new Aperture(apertureConstruction, polygon3D, point3D_Location);
            result.UpdateParameterSets(familyInstance, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));
            //result.Add(Core.Revit.Query.ParameterSet(familyInstance));

            convertSettings?.Add(energyAnalysisOpening.Id, result);

            return result;
        }

        public static Aperture ToSAM_Aperture(this FamilyInstance familyInstance, ConvertSettings convertSettings)
        {
            if (familyInstance == null)
                return null;

            Aperture result = convertSettings?.GetObject<Aperture>(familyInstance.Id);
            if (result != null)
                return result;

            if (Core.Revit.Query.Simplified(familyInstance))
            {
                result = Core.Revit.Query.IJSAMObject<Aperture>(familyInstance);
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
                        if(polygon3Ds != null && polygon3Ds.Count != 0)
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

            List<Point2D> point2Ds = null;
            //Face3D face3D = null;

            //Method 1 of extracting Geometry
            List<ISegmentable3D> segmentable3Ds = Geometry.Revit.Convert.ToSAM_Geometries<ISegmentable3D>(familyInstance, true);
            if((segmentable3Ds == null || segmentable3Ds.Count == 0) && familyInstance is Autodesk.Revit.DB.Panel)
            {
                List<Shell> shells = Geometry.Revit.Convert.ToSAM_Geometries<Shell>(familyInstance, true);
                if(shells != null && shells.Count > 0)
                {
                    foreach(Shell shell in shells)
                    {
                        List<ISegmentable3D> segmentable3Ds_Temp = shell?.GetEdge3Ds()?.ConvertAll(x => x as ISegmentable3D);
                        if(segmentable3Ds_Temp != null)
                        {
                            segmentable3Ds_Temp.ForEach(x => segmentable3Ds.AddRange(x.GetSegments()));
                        }
                    }
                }
            }

            if(segmentable3Ds != null)
            {
                point2Ds = new List<Point2D>();
                List<ISegmentable2D> segmentable2Ds = new List<ISegmentable2D>();
                foreach (ISegmentable3D segmentable3D in segmentable3Ds)
                {
                    ICurve3D curve3D = plane.Project(segmentable3D as ICurve3D);
                    if(curve3D == null)
                    {
                        continue;
                    }
                    
                    ISegmentable2D segmentable2D = plane.Convert(curve3D) as ISegmentable2D;
                    if(segmentable2D == null)
                    {
                        continue;
                    }
                    
                    
                    List<Point2D> point2Ds_Temp = segmentable2D?.GetPoints();
                    if (point2Ds_Temp != null && point2Ds_Temp.Count > 0)
                    {
                        point2Ds_Temp.ForEach(x => point2Ds.Add(x));
                    }

                    //segmentable2Ds.Add(segmentable2D);
                }

                //List<Face2D> face2Ds = segmentable2Ds.Face2Ds()?.Union();
                //if(face2Ds != null && face2Ds.Count > 0)
                //{
                //    face2Ds.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

                //    face3D = plane.Convert(face2Ds[0]);
                //}
            }

            if(point2Ds == null || point2Ds.Count < 3)
            {
                return result;
            }

            Face3D face3D = new Face3D(plane, Geometry.Planar.Create.Rectangle2D(point2Ds));

            //Method 2 of extracting Geometry
            //if (face3D == null || !face3D.IsValid() || face3D.GetArea() < Core.Tolerance.MacroDistance)
            //{
            //    List<Shell> shells = Geometry.Revit.Convert.ToSAM_Geometries<Shell>(familyInstance);
            //    if (shells == null || shells.Count == 0)
            //        return null;

            //    point2Ds = new List<Point2D>();

            //    foreach (Shell shell in shells)
            //    {
            //        List<Face3D> face3Ds = shell?.Face3Ds;
            //        if (face3Ds == null || face3Ds.Count == 0)
            //        {
            //            continue;
            //        }

            //        foreach (Face3D face3D_Temp in face3Ds)
            //        {
            //            IClosedPlanar3D closedPlanar3D = face3D_Temp.GetExternalEdge3D();
            //            if (closedPlanar3D is ICurvable3D)
            //            {
            //                List<ICurve3D> curve3Ds = ((ICurvable3D)closedPlanar3D).GetCurves();
            //                foreach (ICurve3D curve3D in curve3Ds)
            //                {
            //                    ICurve3D curve3D_Temp = plane.Project(curve3D);
            //                    point2Ds.Add(plane.Convert(curve3D_Temp.GetStart()));
            //                }
            //            }
            //        }
            //    }

            //    if (point2Ds == null || point2Ds.Count == 0)
            //        return null;

            //    List<Face3D> face3Ds_Temp = new List<Face3D>();
            //    foreach (Shell shell in shells)
            //    {
            //        List<Face3D> face3Ds = shell?.Face3Ds;
            //        if (face3Ds == null || face3Ds.Count == 0)
            //        {
            //            continue;
            //        }

            //        foreach (Face3D face3D_Temp in face3Ds)
            //        {
            //            Face3D face3D_Project = plane.Project(face3D_Temp);
            //            if (face3D_Project == null || !face3D_Project.IsValid() || face3D_Project.GetArea() < Core.Tolerance.MacroDistance)
            //            {
            //                continue;
            //            }

            //            face3Ds_Temp.Add(face3D_Project);
            //        }
            //    }

            //    if (face3Ds_Temp != null && face3Ds_Temp.Count > 0)
            //    {
            //        face3Ds_Temp = face3Ds_Temp.Union();
            //        face3Ds_Temp.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));
            //        face3D = new Face3D(face3Ds_Temp[0].GetExternalEdge3D());
            //    }
            //}

            //Method 3 of extracting Geometry
            //if (face3D == null || !face3D.IsValid() || face3D.GetArea() < Core.Tolerance.MacroDistance || familyInstance.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
            //{
            //    if(point2Ds != null && point2Ds.Count > 2)
            //    {
            //        face3D = new Face3D(plane, Geometry.Planar.Create.Rectangle2D(point2Ds));
            //    }
            //}

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
                    BoundingBox2D boundingBox2D = new BoundingBox2D(point2Ds);
                    double factor_Height = height / boundingBox2D.Height;
                    double factor_Width = width / boundingBox2D.Width;

                    point2Ds = point2Ds.ConvertAll(x => new Point2D(x.X * factor_Width, x.Y * factor_Height));
                }
            }

            result = Analytical.Create.Aperture(apertureConstruction, face3D);
            if(result == null)
            {
                return result;
            }

            result.UpdateParameterSets(familyInstance, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

            convertSettings?.Add(familyInstance.Id, result);

            return result;
        }
    }
}