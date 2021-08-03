using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMCoreDuplicateViewsByTemplateName : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("33a15bff-cae9-45f6-8f06-b73da0bbc844");

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
        public SAMCoreDuplicateViewsByTemplateName()
          : base("SAMCore.DuplicateViewsByTemplateName", "SAMCore.DuplicateViewsByTemplateName",
              "Duplicates Views By Template Name",
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
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_templateName_Source", NickName = "_templateName_Source", Description = "Source Template Name", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_templateNames_Destination", NickName = "_templateNames_Destination", Description = "Destination Templates Names", Access = GH_ParamAccess.list }, ParamRelevance.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_viewTypes_", NickName = "_viewTypes_", Description = "Revit View Types to be considered", Access = GH_ParamAccess.list };
                param_String.SetPersistentData(new string[] { Core.ViewType.FloorPlan.ToString() });
                result.Add(new ParamDefinition(param_String, ParamRelevance.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new ParamDefinition(param_Boolean, ParamRelevance.Binding));
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.View() { Name = "views", NickName = "views", Description = "Revit Views", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
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
            index = Params.IndexOfInputParam("_templateName_Source");
            if (index == -1 || !dataAccess.GetData(index, ref templateName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<string> templateNames = new List<string>();
            index = Params.IndexOfInputParam("_templateNames_Destination");
            if (index == -1 || !dataAccess.GetDataList(index, templateNames))
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

            List<View> views = Core.Revit.Modify.DuplicateViews(document, templateName, templateNames, viewTypes?.ConvertAll(x => (ViewType)((int)x)));

            index = Params.IndexOfOutputParam("views");
            if (index != -1)
                dataAccess.SetDataList(index, views);
        }
    }
}