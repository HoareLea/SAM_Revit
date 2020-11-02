using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;

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
        public override string LatestComponentVersion => "1.0.1";

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
            index = inputParamManager.AddParameter(new GooConstructionParam(), "constructions_", "constructions_", "SAM Analytical Contructions", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooApertureConstructionParam(), "apertureConstructions_", "apertureConstructions_", "SAM Analytical ApertureContructions", GH_ParamAccess.list);
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

            List<Construction> constructions = new List<Construction>();
            dataAccess.GetDataList(1, constructions);
            if (constructions == null)
                constructions = new List<Construction>();

            List<ApertureConstruction> apertureConstructions = new List<ApertureConstruction>();
            dataAccess.GetDataList(2, apertureConstructions);
            if (apertureConstructions == null)
                apertureConstructions = new List<ApertureConstruction>();

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

            string defaultColumn = null;
            if (!dataAccess.GetData(5, ref defaultColumn) || string.IsNullOrWhiteSpace(defaultColumn))
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

            int index_Source = delimitedFileTable.GetIndex(sourceColumn);
            if (index_Source == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Default = delimitedFileTable.GetIndex(defaultColumn);
            if (index_Default == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Destination = delimitedFileTable.GetIndex(destinationColumn);
            if (index_Destination == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_Type = delimitedFileTable.GetIndex(typeColumn);
            if (index_Type == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Core.Revit.ConvertSettings convertSettings = null;

            if (convertSettings == null)
                convertSettings = Core.Revit.Query.ConvertSettings();

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

                string name_destination = null;
                string name_default = null;
                string name_source = null;
                PanelType panelType = PanelType.Undefined;
                for (int i = 0; i < delimitedFileTable.Count; i++)
                {
                    string typeName = null;
                    if (delimitedFileTable.TryGetValue(i, index_Type, out typeName))
                    {
                        ApertureType apertureType = Analytical.Query.ApertureType(typeName);
                        if (apertureType != ApertureType.Undefined)
                            continue;

                        panelType = Analytical.Query.PanelType(typeName as object);
                    }

                    if (!delimitedFileTable.TryGetValue(i, index_Source, out name_source))
                        continue;

                    if (!name.Equals(name_source))
                        continue;

                    if (!delimitedFileTable.TryGetValue(i, index_Destination, out name_destination))
                    {
                        name_destination = null;
                        continue;
                    }

                    if (!delimitedFileTable.TryGetValue(i, index_Default, out name_default))
                        name_default = null;

                    break;
                }

                if (string.IsNullOrWhiteSpace(name_destination))
                    continue;

                if (panelType == PanelType.Undefined)
                    panelType = panel.PanelType;

                Construction construction_New = constructions.Find(x => x.Name == name_destination);
                if (construction_New == null)
                {
                    construction_New = new Construction(construction, name_destination);
                    construction_New.SetValue(ConstructionParameter.Description, construction.Name);
                }
                    
                HostObjAttributes hostObjAttributes = Analytical.Revit.Convert.ToRevit(construction_New, document, panelType, panel.Normal, convertSettings);
                if (hostObjAttributes == null)
                {
                    if (string.IsNullOrWhiteSpace(name_default))
                    {
                        Construction construction_Default = Analytical.Query.DefaultConstruction(panelType);
                        if (construction_Default != null)
                            name_default = construction_Default.Name;
                    }

                    if (string.IsNullOrWhiteSpace(name_default))
                        continue;

                    hostObjAttributes = Analytical.Revit.Modify.DuplicateByType(document, name_default, panelType, construction_New) as HostObjAttributes;
                    if (hostObjAttributes == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Skipped - Could not duplicate construction for panel ({2}). Element Type Name for: {0}, could not be assinged from {1}", name, name_default, panel.PanelType));
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

                        name_destination = null;
                        name_default = null;
                        name_source = null;
                        ApertureType apertureType = ApertureType.Undefined;
                        for (int i = 0; i < delimitedFileTable.Count; i++)
                        {
                            string typeName = null;
                            if (!delimitedFileTable.TryGetValue(i, index_Type, out typeName))
                                continue;

                            apertureType = Analytical.Query.ApertureType(typeName);
                            if (apertureType == ApertureType.Undefined)
                                continue;

                            if (!delimitedFileTable.TryGetValue(i, index_Source, out name_source))
                                continue;

                            if (!name.Equals(name_source))
                                continue;

                            if (!delimitedFileTable.TryGetValue(i, index_Destination, out name_destination))
                            {
                                name_destination = null;
                                continue;
                            }

                            if (!delimitedFileTable.TryGetValue(i, index_Default, out name_default))
                                name_default = null;

                            break;
                        }

                        if (string.IsNullOrWhiteSpace(name_destination))
                            continue;

                        if (apertureType == ApertureType.Undefined)
                            continue;

                        ApertureConstruction apertureConstruction_New = apertureConstructions.Find(x => x.Name == name_destination);
                        if (apertureConstruction_New == null)
                        {
                            apertureConstruction_New = new ApertureConstruction(apertureConstruction, name_destination);
                            apertureConstruction_New.SetValue(ApertureConstructionParameter.Description, apertureConstruction.Name);
                        }

                        FamilySymbol familySymbol = Analytical.Revit.Convert.ToRevit(apertureConstruction_New, document, convertSettings);
                        if(familySymbol == null)
                        {
                            if (string.IsNullOrWhiteSpace(name_default))
                            {
                                ApertureConstruction apertureConstruction_Default = Analytical.Query.DefaultApertureConstruction(panelType, apertureType);
                                if (apertureConstruction_Default != null)
                                    name_default = apertureConstruction_Default.Name;
                            }

                            if (string.IsNullOrWhiteSpace(name_default))
                                continue;

                            familySymbol = Analytical.Revit.Modify.DuplicateByType(document, name_default, apertureConstruction_New) as FamilySymbol;
                            if (familySymbol == null)
                            {
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Skipped - Could not duplicate construction for panel ({2}). Element Type Name for: {0}, could not be assinged from {1}", name, name_default, panel.PanelType));
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
        }
    }
}