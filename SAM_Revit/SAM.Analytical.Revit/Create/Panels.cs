using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Create
    {
        public static List<Autodesk.Revit.DB.Panel> Panels(this Wall wall)
        {
            if (wall == null)
                return null;

            CurtainGrid curtainGrid = wall.CurtainGrid;
            if (curtainGrid == null)
                return null;

            IEnumerable<ElementId> elementIds = curtainGrid.GetPanelIds();
            if (elementIds == null || elementIds.Count() == 0)
                return null;

            List<Autodesk.Revit.DB.Panel> result = new List<Autodesk.Revit.DB.Panel>();
            foreach (ElementId elementId in elementIds)
            {
                Autodesk.Revit.DB.Panel panel = wall.Document.GetElement(elementId) as Autodesk.Revit.DB.Panel;
                if (panel == null)
                    continue;

                ElementId elementId_Host = panel.FindHostPanel();
                if (elementId_Host == null || elementId_Host == ElementId.InvalidElementId)
                {
                    result.Add(panel);
                    continue;
                }

                List<Autodesk.Revit.DB.Panel> panels = Panels(wall.Document.GetElement(elementId_Host) as Wall);
                if (panels != null && panels.Count > 0)
                    result.AddRange(panels);
            }

            return result;
        }

        public static List<Panel> Panels(this SpatialElement spatialElement)
        {
            if (spatialElement == null)
                return null;

            SpatialElementBoundaryOptions spatialElementBoundaryOptions = new SpatialElementBoundaryOptions();
            spatialElementBoundaryOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            spatialElementBoundaryOptions.StoreFreeBoundaryFaces = true;

            return Panels(spatialElement, spatialElementBoundaryOptions);
        }

        public static List<Panel> Panels(this SpatialElement spatialElement, SpatialElementBoundaryOptions spatialElementBoundaryOptions)
        {
            if (spatialElement == null || spatialElementBoundaryOptions == null)
                return null;

            SpatialElementGeometryCalculator spatialElementGeometryCalculator = new SpatialElementGeometryCalculator(spatialElement.Document, spatialElementBoundaryOptions);

            return Panels(spatialElement, spatialElementGeometryCalculator);
        }

        public static List<Panel> Panels(this SpatialElement spatialElement, SpatialElementGeometryCalculator spatialElementGeometryCalculator)
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
                Geometry.Spatial.Face3D face3D = Geometry.Revit.Convert.ToSAM(tuple.Item1);
                if (face3D == null)
                    continue;

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
                            List<Panel> panels = hostObject.ToSAM();
                            if (panels != null && panels.Count > 0)
                                panel = panels[0];

                            if (panel != null)
                            {
                                panelType = panel.PanelType;
                                construction = panel.Construction;
                            }

                            if (panelType == PanelType.Undefined)
                                panelType = Query.PanelType(hostObject);

                            if (construction == null)
                            {
                                ElementId elementId_Type = hostObject.GetTypeId();
                                if (elementId_Type != null && elementId_Type != ElementId.InvalidElementId)
                                    construction = ((HostObjAttributes)hostObject.Document.GetElement(elementId_Type)).ToSAM();
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
                    construction = Analytical.Query.Construction(panelType); //Default Construction

                if (panel == null)
                {
                    panel = new Panel(construction, panelType, face3D);
                }
                else
                {
                    panel = new Panel(panel.Guid, panel, face3D);
                }

                result.Add(panel);
            }

            return result;
        }
    }
}