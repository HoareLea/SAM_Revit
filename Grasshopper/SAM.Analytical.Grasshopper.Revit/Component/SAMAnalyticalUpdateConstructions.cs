using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalUpdateConstructions : SAMTransactionalChainComponent
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
        public SAMAnalyticalUpdateConstructions()
          : base("SAMAnalytical.UpdateConstructionByExcel", "SAManalytical.UpdateConstructionByExcel",
              "Modify Update Analytical Construction from excel file heading column: Prefix, Name, Width, Function,SAM_BuildingElementType, template Family. New Name Family,SAM Types in Template ",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override ParamDefinition[] Inputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(ParamDefinition.FromParam(new GooPanelParam() { Name = "_panels_", NickName = "_panels_", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new GooConstructionLibraryParam() { Name = "constructionLibrary_", NickName = "constructionLibrary_", Description = "SAM Analytical ContructionLibrary", Optional = true, Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                result.Add(ParamDefinition.FromParam(new GooApertureConstructionLibraryParam() { Name = "apertureConstructionLibrary_", NickName = "apertureConstructionLibrary_", Description = "SAM Analytical ApertureContructionLibrary", Optional = true, Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));

                result.Add(ParamDefinition.FromParam(new Core.Grasshopper.GooDelimitedFileTableParam() { Name = "_delimitedFileTable", NickName = "_delimitedFileTable", Description = "SAM Analytical DelimitedFileTable", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String;

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_sourceColumn_", NickName = "_sourceColumn_", Description = "Column with Source Name of Construction or ApertureConstruction", Optional = true, Access = GH_ParamAccess.item };
                param_String.SetPersistentData("Name");
                result.Add(ParamDefinition.FromParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_defaultColumn_", NickName = "_defaultColumn_", Description = "Column Name for name of the Construction or ApertureConstruction will be copied from if not exists", Optional = true, Access = GH_ParamAccess.item };                param_String.SetPersistentData("template Family");
                result.Add(ParamDefinition.FromParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_destinationColumn_", NickName = "_destinationColumn_", Description = "Column with destination Name for Construction or ApertureConstruction", Optional = true, Access = GH_ParamAccess.item };
                param_String.SetPersistentData("New Name Family");
                result.Add(ParamDefinition.FromParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_typeColumn_", NickName = "_typeColumn_", Description = "Column with Type Name for Construction or ApertureConstruction", Optional = true, Access = GH_ParamAccess.item };
                param_String.SetPersistentData("Category Name");
                result.Add(ParamDefinition.FromParam(param_String, ParamVisibility.Binding));

                param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_thicknessColumn_", NickName = "_thicknessColumn_", Description = "Column with thickness for Construction or ApertureConstruction", Optional = true, Access = GH_ParamAccess.item };
                param_String.SetPersistentData("Width");
                result.Add(ParamDefinition.FromParam(param_String, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(ParamDefinition.FromParam(param_Boolean, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override ParamDefinition[] Outputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.ElementType() { Name = "elementTypes_Panels", NickName = "elementTypes_Panels", Description = "Revit ElementTypes for Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.ElementType() { Name = "elementTypes_Apertures", NickName = "elementTypes_Apertures", Description = "Revit ElementTypes for Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new GooPanelParam() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new GooApertureParam() { Name = "apertures", NickName = "apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                result.Add(ParamDefinition.FromParam(new GooConstructionLibraryParam() { Name = "constructionLibrary", NickName = "constructionLibrary", Description = "SAM Analytical ConstructionLibrary", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new GooConstructionLibraryParam() { Name = "apertureConstructionLibrary", NickName = "apertureConstructionLibrary", Description = "SAM Analytical ApertureConstructionLibrary", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            
            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = new List<Panel>();
            index = Params.IndexOfInputParam("_panels_");
            if (index == -1 || !dataAccess.GetDataList(index, panels))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConstructionLibrary constructionLibrary = null;
            index = Params.IndexOfInputParam("constructionLibrary_");
            if (index != -1)
                dataAccess.GetData(index, ref constructionLibrary);

            if (constructionLibrary == null)
                constructionLibrary = ActiveSetting.Setting.GetValue<ConstructionLibrary>(AnalyticalSettingParameter.DefaultConstructionLibrary);

            ApertureConstructionLibrary apertureConstructionLibrary = null;
            index = Params.IndexOfInputParam("apertureConstructionLibrary_");
            if (index != -1)
                dataAccess.GetData(index, ref apertureConstructionLibrary);

            if (apertureConstructionLibrary == null)
                apertureConstructionLibrary = ActiveSetting.Setting.GetValue<ApertureConstructionLibrary>(AnalyticalSettingParameter.DefaultApertureConstructionLibrary);

            Core.DelimitedFileTable delimitedFileTable = null;
            index = Params.IndexOfInputParam("_delimitedFileTable");
            if (index == -1 || !dataAccess.GetData(index, ref delimitedFileTable) || delimitedFileTable == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string sourceColumn = null;
            index = Params.IndexOfInputParam("_sourceColumn_");
            if (index == -1 || !dataAccess.GetData(index, ref sourceColumn) || string.IsNullOrWhiteSpace(sourceColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string templateColumn = null;
            index = Params.IndexOfInputParam("_defaultColumn_");
            if (index == -1 || !dataAccess.GetData(index, ref templateColumn) || string.IsNullOrWhiteSpace(templateColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string destinationColumn = null;
            index = Params.IndexOfInputParam("_destinationColumn_");
            if (index == -1 ||!dataAccess.GetData(index, ref destinationColumn) || string.IsNullOrWhiteSpace(destinationColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string typeColumn = null;
            index = Params.IndexOfInputParam("_typeColumn_");
            if (index == -1 || !dataAccess.GetData(index, ref typeColumn) || string.IsNullOrWhiteSpace(typeColumn))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string thicknessColumn = null;
            index = Params.IndexOfInputParam("_thicknessColumn_");
            if (index != -1)
                dataAccess.GetData(index, ref thicknessColumn);

            ConstructionLibrary constructionLibrary_Result = null;
            ApertureConstructionLibrary apertureConstructionLibrary_Result = null;

            List<Panel> panels_Result = new List<Panel>();
            List<Aperture> apertures_Result = new List<Aperture>();
            List<ElementType> elementTypes_Panels= new List<ElementType>();
            List<ElementType> elementTypes_Apertures = new List<ElementType>();

            StartTransaction(document);

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

            index = Params.IndexOfOutputParam("elementTypes_Panels");
            if(index != -1)
            dataAccess.SetDataList(index, elementTypes_Panels);

            index = Params.IndexOfOutputParam("elementTypes_Apertures");
            if (index != -1)
                dataAccess.SetDataList(index, elementTypes_Apertures);

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataList(index, panels_Result.ConvertAll(x => new GooPanel(x)));

            index = Params.IndexOfOutputParam("apertures");
            if (index != -1)
                dataAccess.SetDataList(index, apertures_Result.ConvertAll(x => new GooAperture(x)));

            index = Params.IndexOfOutputParam("constructionLibrary");
            if (index != -1)
                dataAccess.SetData(index, new GooConstructionLibrary(constructionLibrary_Result));

            index = Params.IndexOfOutputParam("apertureConstructionLibrary");
            if (index != -1)
                dataAccess.SetData(index, new GooApertureConstructionLibrary(apertureConstructionLibrary_Result));
        }
    }
}