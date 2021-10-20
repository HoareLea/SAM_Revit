using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static HostObject HostObject(this Aperture aperture, Document document)
        {
            HostObject result = null;

            PanelGroup panelGroup = Analytical.Query.PanelGroup(Analytical.Query.PanelType(aperture.Plane.Normal));
            switch (panelGroup)
            {
                case PanelGroup.Wall:
                    IEnumerable<Autodesk.Revit.DB.Wall> walls = Geometry.Revit.Query.Elements<Autodesk.Revit.DB.Wall>(document, aperture.GetBoundingBox());
                    
                    if (walls != null && walls.Count() != 0)
                    {
                        Geometry.Spatial.Point3D point3D = Geometry.Spatial.Query.InternalPoint3D(aperture.GetFace3D());
                        double distance = double.MaxValue;
                        Autodesk.Revit.DB.Wall wall = null;
                        foreach (Autodesk.Revit.DB.Wall wall_Temp in walls)
                        {
                            List<Panel> panels = Convert.ToSAM(wall_Temp, new Core.Revit.ConvertSettings(true, false, false));
                            if(panels != null)
                            {
                                foreach (Panel panel in panels)
                                {
                                    double distance_Temp = panel.GetFace3D().Distance(point3D);
                                    if(distance_Temp < distance)
                                    {
                                        distance = distance_Temp;
                                        wall = wall_Temp;
                                    }
                                }
                            }

                        }
                        result = wall;
                    }
                    
                    break;
                case PanelGroup.Floor:
                    IEnumerable<Element> elements_Floor = Geometry.Revit.Query.Elements(document, aperture.GetBoundingBox(), Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                    if (elements_Floor != null && elements_Floor.Count() != 0)
                        result = elements_Floor.First() as HostObject;
                    break;
                case PanelGroup.Roof:
                    IEnumerable<Element> elements_Roof = Geometry.Revit.Query.Elements(document, aperture.GetBoundingBox(), Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
                    if (elements_Roof != null && elements_Roof.Count() != 0)
                        result = elements_Roof.First() as HostObject;
                    break;
            }

            if (result == null)
            {
                switch (panelGroup)
                {
                    case PanelGroup.Roof:
                        IEnumerable<Element> elements_Floor = Geometry.Revit.Query.Elements(document, aperture.GetBoundingBox(), Autodesk.Revit.DB.BuiltInCategory.OST_Floors);
                        if (elements_Floor != null && elements_Floor.Count() != 0)
                            result = elements_Floor.First() as HostObject;
                        break;
                    case PanelGroup.Floor:
                        IEnumerable<Element> elements_Roof = Geometry.Revit.Query.Elements(document, aperture.GetBoundingBox(), Autodesk.Revit.DB.BuiltInCategory.OST_Roofs);
                        if (elements_Roof != null && elements_Roof.Count() != 0)
                            result = elements_Roof.First() as HostObject;
                        break;
                }
            }

            return result;
        }
    }
}