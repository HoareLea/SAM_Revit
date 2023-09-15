using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static List<ElementType> UpdatePanelTypes(this Document document, List<Panel> panels)
        {
            if(document == null || panels == null)
            {
                return null;
            }

            List<ElementType> result = new List<ElementType>();

            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                if (panel == null)
                {
                    continue;
                }

                Geometry.Spatial.Vector3D normal = panel.Normal;
                PanelType panelType = panel.PanelType;

                if (panelType == PanelType.Air || panelType == PanelType.Undefined)
                {
                    panels[i] = Analytical.Create.Panel(panel);
                    ElementType elementType = Convert.ToRevit_HostObjAttributes(panel, document, new Core.Revit.ConvertSettings(false, true, false));
                    if (elementType != null && result.Find(x => x.Id == elementType.Id) == null)
                    {
                        result.Add(elementType);
                    }

                    continue;
                }

                PanelType panelType_Normal = Query.PanelType(normal);
                if (panelType_Normal == PanelType.Undefined || panelType.PanelGroup() == panelType_Normal.PanelGroup())
                {
                    panels[i] = Analytical.Create.Panel(panel);
                    ElementType elementType = Convert.ToRevit_HostObjAttributes(panel, document, new Core.Revit.ConvertSettings(false, true, false));
                    if (elementType != null && result.Find(x => x.Id == elementType.Id) == null)
                    {
                        result.Add(elementType);
                    }

                    continue;
                }

                if (panelType.PanelGroup() == PanelGroup.Floor || panelType.PanelGroup() == PanelGroup.Roof)
                {
                    double value = normal.Unit.DotProduct(Geometry.Spatial.Vector3D.WorldY);
                    if (Math.Abs(value) <= Core.Revit.Tolerance.Tilt)
                    {
                        panels[i] = Analytical.Create.Panel(panel);
                        ElementType elementType = Convert.ToRevit_HostObjAttributes(panel, document, new Core.Revit.ConvertSettings(false, true, false));
                        if (elementType != null && result.Find(x => x.Id == elementType.Id) == null)
                        {
                            result.Add(elementType);
                        }

                        continue;
                    }
                }

                Construction construction = panel.Construction;
                if (construction != null)
                {
                    construction.SetValue(ConstructionParameter.DefaultPanelType, panelType_Normal);
                }

                HostObjAttributes hostObjAttributes = DuplicateByType(document, construction) as HostObjAttributes;
                if (hostObjAttributes == null)
                {
                    continue;
                }

                panels[i] = Analytical.Create.Panel(panel, panelType_Normal);

                if (panelType_Normal == PanelType.Roof)
                {
                    HashSet<string> names = new HashSet<string>();

                    List<Aperture> apertures = panels[i].Apertures;
                    if (apertures != null && apertures.Count != 0)
                    {
                        foreach (Aperture aperture in apertures)
                        {
                            ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                            if (apertureConstruction == null)
                            {
                                continue;
                            }

                            string name = apertureConstruction.FullName();
                            if (string.IsNullOrWhiteSpace(name))
                            {
                                continue;
                            }

                            FamilySymbol familySymbol = Create.FamilySymbol(apertureConstruction, document, PanelGroup.Roof);
                            if (familySymbol != null && result.Find(x => x.Id == familySymbol.Id) == null)
                            {
                                result.Add(familySymbol);
                            }
                        }
                    }
                }

                if (result.Find(x => x.Id == hostObjAttributes.Id) == null)
                    result.Add(hostObjAttributes);
            }

            return result;
        }
    }
}