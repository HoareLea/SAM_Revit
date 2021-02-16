using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalUpdateConstructions : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e90c2e37-a316-4b88-97b1-a10e794cd46e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateConstructions()
          : base("SAMAnalytical.UpdateConstructionByExcel", "SAManalytical.UpdateConstructionByExcel",
              "Modify Update Analytical Construction from excel file heading column: Prefix, Name, Width, Function,SAM_BuildingElementType, template Family. New Name Family,SAM Types in Template ",
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

            inputParamManager.AddParameter(new Core.Grasshopper.GooDelimitedFileTableParam(), "_delimitedFileTable", "_delimitedFileTable", "SAM Core DelimitedFileTable", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_sourceColumn_", "_sourceColumn_", "Column with Source Name of Construction or ApertureConstruction", GH_ParamAccess.item, "Name");
            inputParamManager.AddTextParameter("_defaultColumn_", "_defaultColumn_", "Column Name for name of the Construction or ApertureConstruction will be copied from if not exists", GH_ParamAccess.item, "template Family");
            inputParamManager.AddTextParameter("_destinationColumn_", "_destinationColumn_", "Column with destination Name for Construction or ApertureConstruction", GH_ParamAccess.item, "New Name Family");
            inputParamManager.AddTextParameter("_typeColumn_", "_typeColumn_", "Column with Type Name for Construction or ApertureConstruction", GH_ParamAccess.item, "Category Name");
            inputParamManager.AddTextParameter("_thicknessColumn_", "_thicknessColumn_", "Column with thickness for Construction or ApertureConstruction", GH_ParamAccess.item, "Width");
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
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
            if (!dataAccess.GetData(9, ref run))
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
                constructionLibrary = ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            ApertureConstructionLibrary apertureConstructionLibrary = null;
            dataAccess.GetData(2, ref apertureConstructionLibrary);
            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);

            Core.DelimitedFileTable delimitedFileTable = null;
            if (!dataAccess.GetData(3, ref delimitedFileTable) || delimitedFileTable == null)
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

            string thicknessColumn = null;
            dataAccess.GetData(8, ref thicknessColumn);

            ConstructionLibrary constructionLibrary_Result = null;
            ApertureConstructionLibrary apertureConstructionLibrary_Result = null;

            List<Panel> panels_Result = new List<Panel>();
            List<Aperture> apertures_Result = new List<Aperture>();
            List<ElementType> elementTypes_Panels= new List<ElementType>();
            List<ElementType> elementTypes_Apertures = new List<ElementType>();
            
            Analytical.Revit.Modify.UpdateConstructions(
                document,
                panels,
                delimitedFileTable,
                constructionLibrary,
                apertureConstructionLibrary,
                out panels_Result,
                out apertures_Result,
                out elementTypes_Panels,
                out elementTypes_Apertures,
                out constructionLibrary_Result,
                out apertureConstructionLibrary_Result,
                sourceColumn,
                destinationColumn,
                templateColumn,
                typeColumn,
                thicknessColumn);

            dataAccess.SetDataList(0, elementTypes_Panels);
            dataAccess.SetDataList(1, elementTypes_Apertures);
            dataAccess.SetDataList(2, panels_Result.ConvertAll(x => new GooPanel(x)));
            dataAccess.SetDataList(3, apertures_Result.ConvertAll(x => new GooAperture(x)));
            dataAccess.SetData(4, new GooConstructionLibrary(constructionLibrary_Result));
            dataAccess.SetData(5, new GooApertureConstructionLibrary(apertureConstructionLibrary_Result));
        }
    }
}