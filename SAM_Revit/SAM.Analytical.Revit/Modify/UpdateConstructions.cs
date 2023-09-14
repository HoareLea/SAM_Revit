using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static bool UpdateConstructions(
            this Document document,
            IEnumerable<Panel> panels,
            Core.DelimitedFileTable delimitedFileTable,
            ConstructionLibrary constructionLibrary,
            ApertureConstructionLibrary apertureConstructionLibrary,
            out List<Panel> panels_Result,
            out List<Aperture> apertures_Result,
            out List<ElementType> elementTypes_Panels,
            out List<ElementType> elementTypes_Apertures,
            out ConstructionLibrary constructionLibrary_Result,
            out ApertureConstructionLibrary apertureConstructionLibrary_Result,
            string sourceColumnName,
            string destinationColumnName,
            string templateColumnName,
            string typeColumnName,
            string thicknessColumnName = null)
        {
            panels_Result = null;
            apertures_Result = null;
            elementTypes_Panels = null;
            elementTypes_Apertures = null;
            constructionLibrary_Result = null;
            apertureConstructionLibrary_Result = null;

            if (document == null || panels == null || delimitedFileTable == null || constructionLibrary == null || apertureConstructionLibrary == null)
                return false;

            int index_Source = delimitedFileTable.GetColumnIndex(sourceColumnName);
            if (index_Source == -1)
                return false;

            int index_Template = delimitedFileTable.GetColumnIndex(templateColumnName);
            if (index_Template == -1)
                return false;

            int index_Destination = delimitedFileTable.GetColumnIndex(destinationColumnName);
            if (index_Destination == -1)
                return false;

            int index_Type = delimitedFileTable.GetColumnIndex(typeColumnName);
            if (index_Type == -1)
                return false;

            panels_Result = new List<Panel>();
            apertures_Result = new List<Aperture>();
            elementTypes_Panels = new List<ElementType>();
            elementTypes_Apertures = new List<ElementType>();

            int index_Thickness = delimitedFileTable.GetColumnIndex(thicknessColumnName);

            constructionLibrary_Result = new ConstructionLibrary(constructionLibrary.Name);
            apertureConstructionLibrary_Result = new ApertureConstructionLibrary(apertureConstructionLibrary.Name);

            Core.Revit.ConvertSettings convertSettings = Core.Revit.Query.ConvertSettings();

            foreach (Panel panel in panels)
            {
                Construction construction = panel?.Construction;
                if (construction == null)
                    continue;

                string name = construction.Name;
                if (name == null)
                    continue;

                string name_Destination = null;
                string name_Template = null;
                string name_Source = null;
                PanelType panelType = PanelType.Undefined;
                double thickness = double.NaN;
                for (int i = 0; i < delimitedFileTable.RowCount; i++)
                {
                    string typeName = null;
                    if (delimitedFileTable.TryConvert(i, index_Type, out typeName))
                    {
                        ApertureType apertureType = Analytical.Query.ApertureType(typeName);
                        if (apertureType != ApertureType.Undefined)
                            continue;

                        panelType = Analytical.Query.PanelType(typeName as object);
                    }

                    if (!delimitedFileTable.TryConvert(i, index_Source, out name_Source))
                        continue;

                    if (!name.Equals(name_Source))
                        continue;

                    if (!delimitedFileTable.TryConvert(i, index_Destination, out name_Destination))
                    {
                        name_Destination = null;
                        continue;
                    }

                    if (!delimitedFileTable.TryConvert(i, index_Template, out name_Template))
                        name_Template = null;

                    if (index_Thickness != -1)
                    {
                        if (!delimitedFileTable.TryConvert(i, index_Thickness, out thickness))
                            thickness = double.NaN;
                    }

                    break;
                }

                if (string.IsNullOrWhiteSpace(name_Destination))
                    name_Destination = name_Template;

                if (string.IsNullOrWhiteSpace(name_Destination))
                    continue;

                if (panelType == PanelType.Undefined)
                    panelType = panel.PanelType;

                Construction construction_New = constructionLibrary_Result.GetConstructions(name_Destination)?.FirstOrDefault();
                if (construction_New == null)
                {
                    Construction construction_Temp = constructionLibrary.GetConstructions(name_Template)?.FirstOrDefault();
                    if (construction_Temp == null)
                        continue;

                    if (name_Destination.Equals(name_Template))
                        construction_New = construction_Temp;
                    else
                        construction_New = new Construction(construction_Temp, name_Destination);

                    construction_New.SetValue(ConstructionParameter.Description, construction.Name);
                    construction_New.RemoveValue(ConstructionParameter.DefaultPanelType);

                    if (!double.IsNaN(thickness))
                        construction_New.SetValue(ConstructionParameter.DefaultThickness, thickness);

                    constructionLibrary_Result.Add(construction_New);
                }

                HostObjAttributes hostObjAttributes = Convert.ToRevit(construction_New, document, panelType, panel.Normal, convertSettings);
                if (hostObjAttributes == null)
                {
                    if (string.IsNullOrWhiteSpace(name_Template))
                    {
                        Construction construction_Default = Analytical.Query.DefaultConstruction(panelType);
                        if (construction_Default != null)
                            name_Template = construction_Default.Name;
                    }

                    if (string.IsNullOrWhiteSpace(name_Template))
                        continue;

                    hostObjAttributes = DuplicateByType(document, name_Template, panelType, construction_New) as HostObjAttributes;
                }

                Construction construction_New_Temp = new Construction(hostObjAttributes.ToSAM(new Core.Revit.ConvertSettings(false, true, false)), construction_New.Guid);
                construction_New_Temp = new Construction(construction_New_Temp, construction_New.ConstructionLayers);
                constructionLibrary_Result.Add(construction_New_Temp);

                Panel panel_New = Analytical.Create.Panel(panel, construction_New_Temp);
                if (panel_New.PanelType != panelType)
                    panel_New = Analytical.Create.Panel(panel_New, panelType);

                List<Aperture> apertures = panel_New.Apertures;
                if (apertures != null && apertures.Count != 0)
                {
                    foreach (Aperture aperture in apertures)
                    {
                        panel_New.RemoveAperture(aperture.Guid);

                        ApertureConstruction apertureConstruction = aperture?.ApertureConstruction;
                        if (apertureConstruction == null)
                            continue;

                        name = apertureConstruction.Name;
                        if (name == null)
                            continue;

                        name_Destination = null;
                        name_Template = null;
                        name_Source = null;
                        ApertureType apertureType = ApertureType.Undefined;
                        for (int i = 0; i < delimitedFileTable.RowCount; i++)
                        {
                            string typeName = null;
                            if (!delimitedFileTable.TryGetValue(i, index_Type, out typeName) || string.IsNullOrWhiteSpace(typeName))
                                continue;

                            apertureType = Analytical.Query.ApertureType(typeName);
                            if (apertureType == ApertureType.Undefined)
                            {
                                if(typeName.Trim().Equals("Curtain Panels"))
                                {
                                    apertureType = ApertureType.Window;
                                }
                            }

                            if (apertureType == ApertureType.Undefined)
                                continue;

                            if (!delimitedFileTable.TryGetValue(i, index_Source, out name_Source) || string.IsNullOrWhiteSpace(name_Source))
                                continue;

                            if (!name.Equals(name_Source))
                                continue;

                            if (!delimitedFileTable.TryGetValue(i, index_Destination, out name_Destination))
                            {
                                name_Destination = null;
                                continue;
                            }

                            if (!delimitedFileTable.TryGetValue(i, index_Template, out name_Template))
                                name_Template = null;

                            break;
                        }


                        if (string.IsNullOrWhiteSpace(name_Destination))
                            name_Destination = name_Template;

                        if (string.IsNullOrWhiteSpace(name_Destination))
                            continue;

                        if (apertureType == ApertureType.Undefined)
                            continue;

                        ApertureConstruction apertureConstruction_New = apertureConstructionLibrary_Result.GetApertureConstructions(name_Destination, Core.TextComparisonType.Equals, true, apertureType)?.FirstOrDefault();
                        if (apertureConstruction_New == null)
                        {
                            ApertureConstruction apertureConstruction_Temp = apertureConstructionLibrary.GetApertureConstructions(name_Template, Core.TextComparisonType.Equals, true, apertureType)?.FirstOrDefault();
                            if (apertureConstruction_Temp == null)
                                continue;

                            if (name_Destination.Equals(name_Template))
                                apertureConstruction_New = apertureConstruction_Temp;
                            else
                                apertureConstruction_New = new ApertureConstruction(apertureConstruction_Temp, name_Destination);

                            apertureConstruction_New.SetValue(ApertureConstructionParameter.Description, apertureConstruction.Name);

                            apertureConstructionLibrary_Result.Add(apertureConstruction_New);
                        }

                        FamilySymbol familySymbol = Convert.ToRevit(apertureConstruction_New, document, convertSettings, panelType.PanelGroup());
                        if (familySymbol == null)
                        {
                            if (string.IsNullOrWhiteSpace(name_Template))
                            {
                                ApertureConstruction apertureConstruction_Default = Analytical.Query.DefaultApertureConstruction(panelType, apertureType);
                                if (apertureConstruction_Default != null)
                                    name_Template = apertureConstruction_Default.Name;
                            }

                            if (string.IsNullOrWhiteSpace(name_Template))
                                continue;

                            familySymbol = DuplicateByType(document, name_Template, apertureConstruction_New) as FamilySymbol;

                        }

                        Aperture aperture_New = new Aperture(aperture, apertureConstruction_New);
                        if (panel_New.AddAperture(aperture_New))
                        {
                            elementTypes_Apertures.Add(familySymbol);
                            apertures_Result.Add(aperture_New);
                        }
                    }

                }

                elementTypes_Panels.Add(hostObjAttributes);
                panels_Result.Add(panel_New);
            }

            return true;

        }
    }
}