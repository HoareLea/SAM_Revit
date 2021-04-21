using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMCoreDeleteViewsByTemplateName : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5af69201-8ab1-4587-b97e-34201d42c216");

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
        public SAMCoreDeleteViewsByTemplateName()
          : base("SAMCore.DeleteViewsByTemplateName", "SAMCore.DeleteViewsByTemplateName",
              "Deletes Views By Template Name",
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
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_templateName", NickName = "_templateName", Description = "Source Template Name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = null;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_inverted_", NickName = "_inverted_", Description = "Inverted", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(true);
                result.Add(ParamDefinition.FromParam(param_Boolean, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_viewTypes_", NickName = "_viewTypes_", Description = "Revit View Types to be considered", Access = GH_ParamAccess.list };
                param_String.SetPersistentData(new string[] { Core.ViewType.FloorPlan.ToString() });
                result.Add(ParamDefinition.FromParam(param_String, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
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
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "ids", NickName = "ids", Description = "Revit Element Ids", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            string templateName = null;
            index = Params.IndexOfInputParam("_templateName");
            if (index == -1|| !dataAccess.GetData(index, ref templateName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool inverted = true;
            index = Params.IndexOfInputParam("_inverted_");
            if (index == -1 || !dataAccess.GetData(index, ref inverted))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Core.ViewType> viewTypes = null;

            List<string> viewTypeNames = new List<string>();
            index = Params.IndexOfInputParam("_viewTypes_");
            if (index != -1 && dataAccess.GetDataList(index, viewTypeNames))
            {
                if(viewTypeNames != null && viewTypeNames.Count != 0)
                {
                    viewTypes = new List<Core.ViewType>();
                    foreach (string viewTypeName in viewTypeNames)
                        if (Enum.TryParse(viewTypeName, true, out Core.ViewType viewType))
                            viewTypes.Add(viewType);
                }
            }

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            StartTransaction(document);

            List<ElementId> elementIds = Core.Revit.Modify.DeleteViews(document, templateName, inverted, viewTypes?.ConvertAll(x => (ViewType)((int)x)));

            index = Params.IndexOfOutputParam("ids");
            if (index != -1)
                dataAccess.SetDataList(index, elementIds?.ConvertAll(x => x.IntegerValue));
        }
    }
}