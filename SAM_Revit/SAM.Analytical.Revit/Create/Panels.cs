using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Create
    {
        public static List<Panel> Panels(this SpatialElement spatialElement, Core.Revit.ConvertSettings convertSettings)
        {
            if (spatialElement == null)
                return null;

            SpatialElementBoundaryOptions spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            spatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            spatialElementBoundaryOptions.StoreFreeBoundaryFaces = true;

            return Panels(spatialElement, spatialElementBoundaryOptions, convertSettings);
        }

        public static List<Panel> Panels(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions, Core.Revit.ConvertSettings convertSettings)
        {
            if (spatialElement == null || spatialElementBoundaryOptions == null)
                return null;

            SpatialElementGeometryCalculator spatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, spatialElementBoundaryOptions);

            return Panels(spatialElement, spatialElementGeometryCalculator, convertSettings);
        }

        public static List<Panel> Panels(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator, Core.Revit.ConvertSettings convertSettings)
        {
            if (spatialElement == null || spatialElementGeometryCalculator == null)
                return null;

            SpatialElementGeometryResults spatialElementGeometryResults = spatialElementGeometryCalculator.CalculateSpatialElementGeometry(spatialElement);
            if (spatialElementGeometryResults == null)
                return null;

            Solid solid = spatialElementGeometryResults.GetGeometry();
            if (solid == null)
                return null;

            List<Tuple<Face, LinkElementId, SubfaceType>> tuples = new List<Tuple<Face, LinkElementId, SubfaceType>>();
            foreach (Face face in solid.Faces)
            {
                IList<SpatialElementBoundarySubface> spatialElementBoundarySubfaces = spatialElementGeometryResults.GetBoundaryFaceInfo(face);
                if (spatialElementBoundarySubfaces == null || spatialElementBoundarySubfaces.Count == 0)
                {
                    tuples.Add(new Tuple<Face, LinkElementId, SubfaceType>(face, null, SubfaceType.Side));
                    continue;
                }

                foreach (SpatialElementBoundarySubface spatialElementBoundarySubface in spatialElementBoundarySubfaces)
                {
                    if (spatialElementBoundarySubface == null)
                        continue;

                    Face face_Subface = spatialElementBoundarySubface.GetSubface();
                    //Face face_Subface = spatialElementBoundarySubface.GetSpatialElementFace();
                    LinkElementId linkElementId = spatialElementBoundarySubface.SpatialBoundaryElement;

                    tuples.Add(new Tuple<Face, LinkElementId, SubfaceType>(face_Subface, linkElementId, spatialElementBoundarySubface.SubfaceType));
                }
            }

            List<Panel> result = new List<Panel>();
            foreach (Tuple<Face, LinkElementId, SubfaceType> tuple in tuples)
            {
                List<Geometry.Spatial.Face3D> face3Ds = Geometry.Revit.Convert.ToSAM(tuple.Item1);
                if (face3Ds == null || face3Ds.Count == 0)
                    continue;

                foreach (Geometry.Spatial.Face3D face3D in face3Ds)
                {
                    PanelType panelType = PanelType.Undefined;
                    Construction construction = null;
                    Panel panel = null;

                    if (tuple.Item2 != null)
                    {
                        Element element = Core.Revit.Query.Element(spatialElement.Document, tuple.Item2);
                        if (element != null)
                        {
                            HostObject hostObject = element as HostObject;
                            if (hostObject != null)
                            {
                                List<Panel> panels = hostObject.ToSAM(convertSettings);
                                if (panels != null && panels.Count > 0)
                                    panel = panels[0];

                                if (panel != null)
                                {
                                    construction = panel.Construction;
                                    panelType = Analytical.Query.PanelType(construction?.Name);

                                    if (panelType == PanelType.Undefined)
                                        panelType = panel.PanelType;
                                }

                                if (panelType == PanelType.Undefined)
                                    panelType = Query.PanelType(hostObject);

                                if (construction == null)
                                {
                                    ElementId elementId_Type = hostObject.GetTypeId();
                                    if (elementId_Type != null && elementId_Type != ElementId.InvalidElementId)
                                        construction = ((HostObjAttributes)hostObject.Document.GetElement(elementId_Type)).ToSAM(convertSettings);
                                }
                            }
                        }
                    }

                    if (panelType == PanelType.Undefined)
                        panelType = Analytical.Query.PanelType(face3D.GetPlane()?.Normal);

                    if (panelType == PanelType.Undefined)
                    {
                        switch (tuple.Item3)
                        {
                            case SubfaceType.Bottom:
                                panelType = PanelType.Floor;
                                break;

                            case SubfaceType.Top:
                                panelType = PanelType.Roof;
                                break;

                            case SubfaceType.Side:
                                panelType = PanelType.Wall;
                                break;
                        }
                    }

                    if (construction == null)
                        construction = Analytical.Query.DefaultConstruction(panelType); //Default Construction

                    if (panel == null)
                    {
                        panel = Analytical.Create.Panel(construction, panelType, face3D);
                    }
                    else
                    {
                        panel = Analytical.Create.Panel(panel.Guid, panel, face3D);
                    }

                    result.Add(panel);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets Wall Panels from Spatial Element base on its Boundary
        /// </summary>
        /// <param name="spatialElement">Revit Spatial Element such as Space or Room</param>
        /// <param name="elevation_Bottom">Bottom Elevation</param>
        /// <param name="elevation_Top">Top Elevation</param>
        /// <param name="convertSettings">Convert Settings</param>
        /// <param name="spatialElementBoundaryOptions">Revit SpactialElementBoundaryOptions. Center Spatial Element Boundary Location will be used when set to null</param>
        /// <returns>Wall Panels</returns>
        public static List<Panel> Panels(this SpatialElement spatialElement, double elevation_Bottom, double elevation_Top, Core.Revit.ConvertSettings convertSettings, SpatialElementBoundaryOptions spatialElementBoundaryOptions = null)
        {
            if (spatialElement == null || double.IsNaN(elevation_Top) || double.IsNaN(elevation_Bottom))
                return null;
            
            if(spatialElementBoundaryOptions == null)
            {
                spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
                spatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            }

            Document document = spatialElement.Document;

            Geometry.Spatial.Plane plane = Geometry.Spatial.Plane.WorldXY;
            Geometry.Spatial.Plane plane_Top = plane.GetMoved(new Geometry.Spatial.Vector3D(0, 0, elevation_Top)) as Geometry.Spatial.Plane;
            Geometry.Spatial.Plane plane_Bottom = plane.GetMoved(new Geometry.Spatial.Vector3D(0, 0, elevation_Bottom)) as Geometry.Spatial.Plane;

            List<Panel> panels = new List<Panel>();

            IList<IList<BoundarySegment>> boundaries = spatialElement.GetBoundarySegments(spatialElementBoundaryOptions);
            foreach(IList<BoundarySegment> boundarySegments in boundaries)
            {
                if (boundarySegments == null)
                    continue;

                foreach (BoundarySegment boundarySegment in boundarySegments)
                {
                    if (boundarySegment == null)
                        continue;

                    Curve curve = boundarySegment.GetCurve();
                    if (curve == null)
                        continue;

                    HostObject hostObject = null;
                    if (boundarySegment.ElementId != null && boundarySegment.ElementId != ElementId.InvalidElementId)
                    {
                        if (boundarySegment.LinkElementId == null || boundarySegment.LinkElementId == ElementId.InvalidElementId)
                        {
                            hostObject = document.GetElement(boundarySegment.ElementId) as HostObject;
                        }
                        else
                        {
                            RevitLinkInstance revitLinkInstance = document.GetElement(boundarySegment.LinkElementId) as RevitLinkInstance;
                            if (revitLinkInstance != null)
                            {
                                Document document_Linked = revitLinkInstance.GetLinkDocument();
                                if (document_Linked != null)
                                    hostObject = document_Linked.GetElement(boundarySegment.ElementId) as HostObject;
                            }
                        }
                    }

                    Panel panel_Temp = null;

                    if (hostObject != null)
                    {
                        List<Panel> panels_Temp = hostObject.ToSAM(convertSettings);
                        if (panels_Temp != null && panels_Temp.Count > 0)
                            panel_Temp = panels_Temp[0];
                    }

                    List<Geometry.Spatial.Segment3D> segment3Ds = Geometry.Revit.Convert.ToSAM_Segment3Ds(curve);
                    if (segment3Ds == null || segment3Ds.Count == 0)
                        continue;

                    foreach (Geometry.Spatial.Segment3D segment3D in segment3Ds)
                    {
                        Geometry.Spatial.Segment3D segment3D_Top = Geometry.Spatial.Query.Project(plane_Top, segment3D);
                        Geometry.Spatial.Segment3D segment3D_Bottom = Geometry.Spatial.Query.Project(plane_Bottom, segment3D);

                        Geometry.Spatial.Polygon3D polygon3D = Geometry.Spatial.Create.Polygon3D(new Geometry.Spatial.Point3D[] { segment3D_Top[0], segment3D_Top[1], segment3D_Bottom[1], segment3D_Bottom[0] });
                        if (polygon3D == null)
                            continue;

                        Geometry.Spatial.Face3D face3D = new Geometry.Spatial.Face3D(polygon3D);

                        Panel panel = null;
                        if (panel_Temp != null)
                            panel = Analytical.Create.Panel(Guid.NewGuid(), panel_Temp, face3D);

                        if (panel_Temp != null)
                        {
                            Geometry.Spatial.Vector3D normal = polygon3D.GetPlane()?.Normal;
                            PanelType panelType = Analytical.Query.PanelType(normal);

                            Construction construction = Query.DefaultAirConstruction(panelType.PanelGroup());
                            panel = Analytical.Create.Panel(construction, panelType, face3D);
                        }

                        if (panel != null)
                            panels.Add(panel);
                    }
                }
            }

            return panels;
        }
    }
}