using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Core;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Panel ToSAM(this EnergyAnalysisSurface energyAnalysisSurface, Point3D point3D_Space = null, double tolerance = Tolerance.Distance)
        {
            if (energyAnalysisSurface == null)
                return null;

            Document document = energyAnalysisSurface.Document;

            Polygon3D polygon3D = energyAnalysisSurface.GetPolyloop().ToSAM();
            if (polygon3D == null)
                return null;

            if(point3D_Space != null)
            {
                Geometry.Spatial.Plane plane = polygon3D.GetPlane();
                if(plane != null)
                {
                    Point3D point3D_Space_Projected = plane.Project(point3D_Space);

                    Vector3D vector3D = new Vector3D(point3D_Space, point3D_Space_Projected);
                    if (vector3D == null || vector3D.Length < tolerance)
                        vector3D = Vector3D.WorldZ.GetNegated();

                    if(!vector3D.SameHalf(plane.AxisZ))
                    {
                        plane.FlipZ();
                        polygon3D = new Polygon3D(plane, polygon3D.GetPoints().ConvertAll(x => plane.Convert(x)));
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
            Construction construction = ((HostObjAttributes)hostObject.Document.GetElement(elementId_Type)).ToSAM();
            if (construction == null)
                construction = Analytical.Query.Construction(panelType); //Default Construction

            PanelType panelType_Temp = Query.PanelType(construction);
            if (panelType_Temp != PanelType.Undefined)
                panelType = panelType_Temp;

            Face3D face3D = new Face3D(polygon3D);
            Panel panel = new Panel(construction, panelType, face3D);

            IEnumerable<EnergyAnalysisOpening> energyAnalysisOpenings = energyAnalysisSurface.GetAnalyticalOpenings();
            if(energyAnalysisOpenings != null && energyAnalysisOpenings.Count() != 0)
            {
                foreach(EnergyAnalysisOpening energyAnalysisOpening in energyAnalysisOpenings)
                {
                    Aperture aperture = energyAnalysisOpening.ToSAM();
                    if (aperture != null)
                        panel.AddAperture(aperture);
                }
            }

            return panel;
        }
    }
}