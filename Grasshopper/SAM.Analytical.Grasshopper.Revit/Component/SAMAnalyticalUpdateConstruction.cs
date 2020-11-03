using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalUpdateConstruction : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e90c2e37-a316-4b88-97b1-a10e794cd46e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateConstruction()
          : base("SAMAnalytical.UpdateConstruction", "SAManalytical.UpdateConstruction",
              "Modify Update Analytical Construction from csv file heading column: Prefix, Name, Width, Function,SAM_BuildingElementType, template Family. New Name Family,SAM Types in Template ",
              "SAM", "Revit")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooPanelParam(), "_panels_", "_panels_", "SAM Analytical Panels", GH_ParamAccess.list);
            index = inputParamManager.AddParameter(new GooConstructionLibraryParam(), "constructionLibrary_", "constructionLibrary_", "SAM Analytical ContructionLibrary", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooApertureConstructionLibraryParam(), "apertureConstructionLibrary_", "apertureConstructionLibrary_", "SAM Analytical ApertureContructionLibrary", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddTextParameter("_csv", "_csv", "file path to csv", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_sourceColumn_", "_sourceColumn_", "Column with Source Name of Construction or ApertureConstruction", GH_ParamAccess.item, "Name");
            inputParamManager.AddTextParameter("_defaultColumn_", "_defaultColumn_", "Column Name for name of the Construction or ApertureConstruction will be copied from if not exists", GH_ParamAccess.item, "template Family");
            inputParamManager.AddTextParameter("_destinationColumn_", "_destinationColumn_", "Column with destination Name for Construction or ApertureConstruction", GH_ParamAccess.item, "New Name Family");
            inputParamManager.AddTextParameter("_typeColumn_", "_typeColumn_", "Column with Type Name for Construction or ApertureConstruction", GH_ParamAccess.item, "SAM_BuildingElementType");
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("ElementTypes_Panels", "ElementTypes", "Revit ElementTypes", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("ElementTypes_Apertures", "ElementTypes", "Revit ElementTypes", GH_ParamAccess.list);
            
            outputParamManager.AddGenericParameter("Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("Apertures", "Apertures", "SAM Analytical Apertures", GH_ParamAccess.list);

            outputParamManager.AddParameter(new GooConstructionLibraryParam(), "ConstructionLibrary", "ConstructionLibrary", "SAM Analytical ConstructionLibrary", GH_ParamAccess.item);
            outputParamManager.AddParameter(new GooApertureConstructionLibraryParam(), "ApertureConstructionLibrary", "ApertureConstructionLibrary", "SAM Analytical ApertureConstructionLibrary", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData<bool>(8, ref run))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(3, false);
                return;
            }
            if (!run)
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            if (!dataAccess.GetDataList(0, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConstructionLibrary constructionLibrary = null;
            dataAccess.GetData(1, ref constructionLibrary);
            if (constructionLibrary == null)
                constructionLibrary = Analytical.Query.DefaultConstructionLibrary();

            ApertureConstructionLibrary apertureConstructionLibrary = null;
            dataAccess.GetData(2, ref apertureConstructionLibrary);
            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = Analytical.Query.DefaultApertureConstructionLibrary();

            string csvOrPath = null;
            if (!dataAccess.GetData(3, ref csvOrPath) || string.IsNullOrWhiteSpace(csvOrPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string sourceColumn = null;
            if (!dataAccess.GetData(4, ref sourceColumn) || string.IsNullOrWhiteSpace(sourceColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string templateColumn = null;
            if (!dataAccess.GetData(5, ref templateColumn) || string.IsNullOrWhiteSpace(templateColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string destinationColumn = null;
            if (!dataAccess.GetData(6, ref destinationColumn) || string.IsNullOrWhiteSpace(destinationColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string typeColumn = null;
            if (!dataAccess.GetData(7, ref typeColumn) || string.IsNullOrWhiteSpace(typeColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Core.DelimitedFileTable delimitedFileTable = null;
            if (Core.Query.ValidFilePath(csvOrPath))
            {
                delimitedFileTable = new Core.DelimitedFileTable(new Core.DelimitedFileReader(Core.DelimitedFileType.Csv, csvOrPath));
            }
            else
            {
                string[] lines = csvOrPath.Split('\n');
                delimitedFileTable = new Core.DelimitedFileTable(new Core.DelimitedFileReader(Core.DelimitedFileType.Csv, lines));
            }

            if (delimitedFileTable == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Source = delimitedFileTable.GetColumnIndex(sourceColumn);
            if (index_Source == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Template = delimitedFileTable.GetColumnIndex(templateColumn);
            if (index_Template == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Destination = delimitedFileTable.GetColumnIndex(destinationColumn);
            if (index_Destination == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Type = delimitedFileTable.GetColumnIndex(typeColumn);
            if (index_Type == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Core.Revit.ConvertSettings convertSettings = null;

            if (convertSettings == null)
                convertSettings = Core.Revit.Query.ConvertSettings();

            ConstructionLibrary constructionLibrary_Result = new ConstructionLibrary(constructionLibrary.Name);
            ApertureConstructionLibrary apertureConstructionLibrary_Result = new ApertureConstructionLibrary(apertureConstructionLibrary.Name);

            List<Panel> panels_Result = new List<Panel>();
            List<Aperture> apertures_Result = new List<Aperture>();
            List<ElementType> elementTypes_Panels= new List<ElementType>();
            List<ElementType> elementTypes_Apertures = new List<ElementType>();
            foreach (Panel panel in panels)
            {
                Construction construction = panel?.Construction;
                if (construction == null)
                    continue;

                string name = construction.Name;
                if (name == null)
                {
                    //result.Add(construction);
                    continue;
                }

                string name_Destination = null;
                string name_Template = null;
                string name_Source = null;
                PanelType panelType = PanelType.Undefined;
                for (int i = 0; i < delimitedFileTable.RowCount; i++)
                {
                    string typeName = null;
                    if (delimitedFileTable.TryGetValue(i, index_Type, out typeName))
                    {
                        ApertureType apertureType = Analytical.Query.ApertureType(typeName);
                        if (apertureType != ApertureType.Undefined)
                            continue;

                        panelType = Analytical.Query.PanelType(typeName as object);
                    }

                    if (!delimitedFileTable.TryGetValue(i, index_Source, out name_Source))
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

                if (panelType == PanelType.Undefined)
                    panelType = panel.PanelType;

                Construction construction_New = constructionLibrary_Result.GetConstructions(name_Destination)?.FirstOrDefault();
                if(construction_New == null)
                {
                    Construction construction_Temp = constructionLibrary.GetConstructions(name_Template)?.FirstOrDefault();
                    if (construction_Temp == null)
                        continue;

                    if (name_Destination.Equals(name_Template))
                        construction_New = construction_Temp;
                    else
                        construction_New = new Construction(construction_Temp, name_Destination);

                    construction_New.SetValue(ConstructionParameter.Description, construction.Name);
                    construction_New.SetValue(ConstructionParameter.DefaultPanelType, panelType.Text());

                    constructionLibrary_Result.Add(construction_New);
                }
                    
                HostObjAttributes hostObjAttributes = Analytical.Revit.Convert.ToRevit(construction_New, document, panelType, panel.Normal, convertSettings);
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

                    hostObjAttributes = Analytical.Revit.Modify.DuplicateByType(document, name_Template, panelType, construction_New) as HostObjAttributes;
                    if (hostObjAttributes == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Skipped - Could not duplicate construction for panel ({2}). Element Type Name for: {0}, could not be assinged from {1}", name, name_Template, panel.PanelType));
                        continue;
                    }
                }

                Panel panel_New = new Panel(panel, construction_New);
                if (panel_New.PanelType != panelType)
                    panel_New = new Panel(panel_New, panelType);

                List<Aperture> apertures = panel_New.Apertures;
                if(apertures != null && apertures.Count != 0)
                {                    
                    foreach(Aperture aperture in apertures)
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
                            if (!delimitedFileTable.TryGetValue(i, index_Type, out typeName))
                                continue;

                            apertureType = Analytical.Query.ApertureType(typeName);
                            if (apertureType == ApertureType.Undefined)
                                continue;

                            if (!delimitedFileTable.TryGetValue(i, index_Source, out name_Source))
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

                        ApertureConstruction apertureConstruction_New = apertureConstructionLibrary_Result.GetApertureConstructions(name_Destination)?.FirstOrDefault();
                        if (apertureConstruction_New == null)
                        {
                            ApertureConstruction apertureConstruction_Temp = apertureConstructionLibrary.GetApertureConstructions(name_Template)?.FirstOrDefault();
                            if (apertureConstruction_Temp == null)
                                continue;

                            if (name_Destination.Equals(name_Template))
                                apertureConstruction_New = apertureConstruction_Temp;
                            else
                                apertureConstruction_New = new ApertureConstruction(apertureConstruction_Temp, name_Destination);

                            apertureConstruction_New.SetValue(ApertureConstructionParameter.Description, apertureConstruction.Name);

                            apertureConstructionLibrary_Result.Add(apertureConstruction_New);
                        }

                        FamilySymbol familySymbol = Analytical.Revit.Convert.ToRevit(apertureConstruction_New, document, convertSettings);
                        if(familySymbol == null)
                        {
                            if (string.IsNullOrWhiteSpace(name_Template))
                            {
                                ApertureConstruction apertureConstruction_Default = Analytical.Query.DefaultApertureConstruction(panelType, apertureType);
                                if (apertureConstruction_Default != null)
                                    name_Template = apertureConstruction_Default.Name;
                            }

                            if (string.IsNullOrWhiteSpace(name_Template))
                                continue;

                            familySymbol = Analytical.Revit.Modify.DuplicateByType(document, name_Template, apertureConstruction_New) as FamilySymbol;
                            if (familySymbol == null)
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Skipped - Could not duplicate construction for panel ({2}). Element Type Name for: {0}, could not be assinged from {1}", name, name_Template, panel.PanelType));
                                continue;
                            }
                        }

                        Aperture aperture_New = new Aperture(aperture, apertureConstruction_New);
                        if(panel_New.AddAperture(aperture_New))
                        {
                            elementTypes_Apertures.Add(familySymbol);
                            apertures_Result.Add(aperture_New);
                        }
                    }

                }

                elementTypes_Panels.Add(hostObjAttributes);
                panels_Result.Add(panel_New);
            }

            dataAccess.SetDataList(0, elementTypes_Panels);
            dataAccess.SetDataList(1, elementTypes_Apertures);
            dataAccess.SetDataList(2, panels_Result.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(3, apertures_Result.ConvertAll(x => new GooAperture(x)));
            dataAccess.SetData(4, new GooConstructionLibrary(constructionLibrary_Result));
            dataAccess.SetData(5, new GooApertureConstructionLibrary(apertureConstructionLibrary_Result));
        }
    }
}