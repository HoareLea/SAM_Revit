using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalUpdateConstruction : RhinoInside.Revit.GH.Components.TransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e90c2e37-a316-4b88-97b1-a10e794cd46e");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateConstruction()
          : base("SAMAnalytical.UpdateConstruction", "SAManalytical.UpdateConstruction",
              "Modify Update Analytical Construction",
              "SAM", "Revit")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooPanelParam(), "_panels_", "_panels_", "SAM Analytical Panels", GH_ParamAccess.list);
            index = inputParamManager.AddParameter(new GooConstructionParam(), "constructions_", "constructions_", "SAM Analytical Contructions", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;

            inputParamManager.AddTextParameter("_csv", "_csv", "csv", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_sourceColumn", "_sourceColumn", "Column with Source Name of Construction", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_defaultColumn", "_defaultColumn", "Column Name for name of the Construction will be copied from if not exists", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_destinationColumn", "_destinationColumn", "Column with destination Name for Construction", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("ElementTypes", "ElementTypes", "Revit ElementTypes", GH_ParamAccess.item);
            outputParamManager.AddGenericParameter("Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.item);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData<bool>(6, ref run))
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

            string csvOrPath = null;
            if (!dataAccess.GetData(2, ref csvOrPath) || string.IsNullOrWhiteSpace(csvOrPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string sourceColumn = null;
            if (!dataAccess.GetData(3, ref sourceColumn) || string.IsNullOrWhiteSpace(sourceColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string defaultColumn = null;
            if (!dataAccess.GetData(4, ref defaultColumn) || string.IsNullOrWhiteSpace(defaultColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string destinationColumn = null;
            if (!dataAccess.GetData(5, ref destinationColumn) || string.IsNullOrWhiteSpace(destinationColumn))
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

            Core.Revit.ConvertSettings convertSettings = null;

            if (convertSettings == null)
                convertSettings = Core.Revit.Query.ConvertSettings();

            List<Panel> panels_Result = new List<Panel>();
            List<ElementType> elementTypes_Result = new List<ElementType>();
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
                for (int i = 0; i < delimitedFileTable.Count; i++)
                {
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
                    {
                        name_default = null;
                    }

                    break;
                }

                if (string.IsNullOrWhiteSpace(name_destination))
                {
                    //result.Add(construction);
                    continue;
                }

                Construction construction_New = constructions.Find(x => x.Name == name_destination);
                if (construction_New == null)
                    construction_New = new Construction(construction, name_destination);

                HostObjAttributes hostObjAttributes = Analytical.Revit.Convert.ToRevit(construction_New, document, panel.PanelType, panel.Normal, convertSettings);
                if (hostObjAttributes == null)
                {
                    if (string.IsNullOrWhiteSpace(name_default))
                    {
                        Construction construction_Default = Analytical.Query.Construction(panel.PanelType);
                        if (construction_Default != null)
                            name_default = construction_Default.Name;
                    }

                    if (string.IsNullOrWhiteSpace(name_default))
                        continue;

                    hostObjAttributes = Analytical.Revit.Modify.DuplicateByConstruction(document, name_default, panel.PanelType, construction_New) as HostObjAttributes;
                    if (hostObjAttributes == null)
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Skipped - Could not duplicate construction for panel ({2}). Element Type Name for: {0}, could not be assinged from {1}", name, name_default, panel.PanelType));
                        continue;
                    }
                }
                elementTypes_Result.Add(hostObjAttributes);
                panels_Result.Add(new Panel(panel, construction_New));
            }

            dataAccess.SetDataList(0, elementTypes_Result);
            dataAccess.SetDataList(1, panels_Result.ConvertAll(x => new GooPanel(x)));
        }
    }
}