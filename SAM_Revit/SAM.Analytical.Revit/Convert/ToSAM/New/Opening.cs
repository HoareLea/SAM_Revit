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
        public static IOpening ToSAM_Opening(this EnergyAnalysisOpening energyAnalysisOpening, ConvertSettings convertSettings)
        {
            if (energyAnalysisOpening == null)
                return null;

            IOpening result = convertSettings?.GetObject<IOpening>(energyAnalysisOpening.Id);
            if (result != null)
                return result;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            Polygon3D polygon3D = energyAnalysisOpening.GetPolyloop().ToSAM();
#else
            Polygon3D polygon3D = energyAnalysisOpening.GetPolyloops()?.FirstOrDefault().ToSAM();
#endif


            if (polygon3D == null)
                return null;

            FamilyInstance familyInstance = Core.Revit.Query.Element(energyAnalysisOpening) as FamilyInstance;
            if(familyInstance == null)
                return null;

            if (Core.Revit.Query.Simplified(familyInstance))
            {
                result = Core.Revit.Query.IJSAMObjects<IOpening>(familyInstance)?.FirstOrDefault();
                if (result != null)
                    return result;
            }

            OpeningType openingType = ToSAM_OpeningType(familyInstance.Symbol, convertSettings);

            Point3D point3D_Location = Geometry.Revit.Query.LocationPoint3D(familyInstance);
            if (point3D_Location == null)
                return null;

            Face3D face3D = new Face3D(Geometry.Spatial.Create.IClosedPlanar3D(polygon3D, point3D_Location));
            if(face3D == null)
            {
                return null;
            }

            result = Analytical.Create.Opening(openingType, face3D);
            result.UpdateParameterSets(familyInstance);

            convertSettings?.Add(energyAnalysisOpening.Id, result);

            return result;
        }
        
        public static IOpening ToSAM_Opening(this FamilyInstance familyInstance, ConvertSettings convertSettings)
        {
            if (familyInstance == null)
                return null;

            IOpening result = convertSettings?.GetObject<IOpening>(familyInstance.Id);
            if (result != null)
                return result;

            if (Core.Revit.Query.Simplified(familyInstance))
            {
                result = Core.Revit.Query.IJSAMObjects<IOpening>(familyInstance)?.FirstOrDefault();
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

            OpeningType openingType = ToSAM_OpeningType(familyInstance.Symbol, convertSettings);
            if(openingType == null)
            {
                return null;
            }

            BuiltInCategory builtInCategory_Host = BuiltInCategory.INVALID;

            HostObject hostObject = familyInstance.Host as HostObject;
            if(hostObject != null)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                builtInCategory_Host = (BuiltInCategory)hostObject.Category.Id.IntegerValue;
#else
                builtInCategory_Host = (BuiltInCategory)hostObject.Category.Id.Value;
#endif
            }

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

            List<Face3D> face3Ds = Geometry.Revit.Convert.ToSAM_Geometries<Face3D>(familyInstance);
            if (face3Ds == null || face3Ds.Count == 0)
                return null;

            List<Point2D> point2Ds = new List<Point2D>();
            foreach (Face3D face3D_Temp in face3Ds)
            {
                IClosedPlanar3D closedPlanar3D = face3D_Temp.GetExternalEdge3D();
                if (closedPlanar3D is ICurvable3D)
                {
                    List<ICurve3D> curve3Ds = ((ICurvable3D)closedPlanar3D).GetCurves();
                    foreach (ICurve3D curve3D in curve3Ds)
                    {
                        ICurve3D curve3D_Temp = plane.Project(curve3D);
                        point2Ds.Add(plane.Convert(curve3D_Temp.GetStart()));
                    }
                }
            }

            if (point2Ds == null || point2Ds.Count == 0)
                return null;

            Rectangle2D rectangle2D = Geometry.Planar.Create.Rectangle2D(point2Ds);

            result = Analytical.Create.Opening(openingType, new Face3D(plane, rectangle2D));
            result.UpdateParameterSets(familyInstance);

            convertSettings?.Add(familyInstance.Id, result);

            return result;
        }
    }
}