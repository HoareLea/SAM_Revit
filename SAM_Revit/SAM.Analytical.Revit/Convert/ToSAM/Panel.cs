using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Core.Revit;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Panel ToSAM(this EnergyAnalysisSurface energyAnalysisSurface, ConvertSettings convertSettings, Shell shell = null, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (energyAnalysisSurface == null)
                return null;

            Panel result = convertSettings?.GetObject<Panel>(energyAnalysisSurface.Id);
            if (result != null)
                return result;

            Document document = energyAnalysisSurface.Document;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            Polygon3D polygon3D = energyAnalysisSurface.GetPolyloop().ToSAM();
#else
            Polygon3D polygon3D = energyAnalysisSurface.GetPolyloops()?.FirstOrDefault()?.ToSAM();
#endif

            if (polygon3D == null)
                return null;

            if(shell != null)
            {
                Geometry.Spatial.Plane plane = polygon3D.GetPlane();
                
                if(plane != null)
                {
                    Point3D point3D = polygon3D.InternalPoint3D();
                    if(point3D != null)
                    {
                       Vector3D normal = shell.Normal(polygon3D.InternalPoint3D(), true, silverSpacing, tolerance);
                        if (!normal.SameHalf(plane.AxisZ))
                        {
                            plane.FlipZ();
                            polygon3D = new Polygon3D(plane, polygon3D.GetPoints().ConvertAll(x => plane.Convert(x)));
                        }
                    }
                }
            }

            HostObject hostObject = Core.Revit.Query.Element(document, energyAnalysisSurface.CADObjectUniqueId, energyAnalysisSurface.CADLinkUniqueId) as HostObject;
            if (hostObject == null)
                return Analytical.Create.Panel(null, PanelType.Air, new Face3D(polygon3D));

            ElementId elementId_Type = hostObject.GetTypeId();
            if (elementId_Type == null || elementId_Type == ElementId.InvalidElementId)
                return null;

            PanelType panelType = Query.PanelType(hostObject);
            Construction construction = ((HostObjAttributes)hostObject.Document.GetElement(elementId_Type)).ToSAM(convertSettings);
            if (construction == null)
                construction = Analytical.Query.DefaultConstruction(panelType); //Default Construction

            PanelType panelType_Temp = Query.PanelType(construction);
            if (panelType_Temp != PanelType.Undefined)
                panelType = panelType_Temp;

            Face3D face3D = new Face3D(polygon3D);
            result = Analytical.Create.Panel(construction, panelType, face3D);

            IEnumerable<EnergyAnalysisOpening> energyAnalysisOpenings = energyAnalysisSurface.GetAnalyticalOpenings();
            if(energyAnalysisOpenings != null && energyAnalysisOpenings.Count() != 0)
            {
                foreach(EnergyAnalysisOpening energyAnalysisOpening in energyAnalysisOpenings)
                {
                    Aperture aperture = energyAnalysisOpening.ToSAM(convertSettings);
                    if (aperture != null)
                        result.AddAperture(aperture);
                }
            }

            result.UpdateParameterSets(energyAnalysisSurface, ActiveSetting.Setting.GetValue<Core.TypeMap>(Core.Revit.ActiveSetting.Name.ParameterMap));

            convertSettings?.Add(energyAnalysisSurface.Id, result);

            if (convertSettings.UseProjectLocation)
            {
                Transform transform = Core.Revit.Query.ProjectTransform(energyAnalysisSurface.Document);
                if (transform != null)
                {
                    result = Query.Transform(transform, result);
                }
            }

            return result;
        }
    }
}