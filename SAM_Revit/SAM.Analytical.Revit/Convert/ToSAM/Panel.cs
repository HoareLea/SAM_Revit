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
        public static Panel ToSAM(this EnergyAnalysisSurface energyAnalysisSurface, Core.Revit.ConvertSettings convertSettings, Shell shell = null, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (energyAnalysisSurface == null)
                return null;

            Panel result = convertSettings?.GetObject<Panel>(energyAnalysisSurface.Id);
            if (result != null)
                return result;

            Document document = energyAnalysisSurface.Document;

            Polygon3D polygon3D = energyAnalysisSurface.GetPolyloop().ToSAM();
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
                return new Panel(null, PanelType.Air, new Face3D(polygon3D));

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
            result = new Panel(construction, panelType, face3D);

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

            //result.Add(Core.Revit.Query.ParameterSet(energyAnalysisSurface));

            convertSettings?.Add(energyAnalysisSurface.Id, result);

            return result;
        }
    }
}