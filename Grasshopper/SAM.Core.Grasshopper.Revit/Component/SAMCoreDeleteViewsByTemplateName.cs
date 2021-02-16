using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMCoreDeleteViewsByTemplateName : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5af69201-8ab1-4587-b97e-34201d42c216");

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
        public SAMCoreDeleteViewsByTemplateName()
          : base("SAMCore.DeleteViewsByTemplateName", "SAMCore.DeleteViewsByTemplateName",
              "Deletes Views By Template Name",
              "SAM", "Revit")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            inputParamManager.AddTextParameter("_templateName", "_templateName_Source", "Source Template Name", GH_ParamAccess.item);

            global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_inverted_", NickName = "_inverted_", Description = "Inverted", Access = GH_ParamAccess.item };
            boolean.SetPersistentData(true);
            inputParamManager.AddParameter(boolean);

            global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_viewTypes_", NickName = "_viewTypes_", Description = "Revit View Types to be considered", Access = GH_ParamAccess.list };
            @string.SetPersistentData(new string[] { Core.ViewType.FloorPlan.ToString() });
            inputParamManager.AddParameter(@string);
            
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("ElementIds", "ElementIds", "ElementIds", GH_ParamAccess.list);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(3, ref run) || !run)
                return;

            string templateName = null;
            if (!dataAccess.GetData(0, ref templateName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool inverted = true;
            if (!dataAccess.GetData(1, ref inverted))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Core.ViewType> viewTypes = null;

            List<string> viewTypeNames = new List<string>();
            if (!dataAccess.GetDataList(2, viewTypeNames))
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

            List<ElementId> elementIds = Core.Revit.Modify.DeleteViews(document, templateName, inverted, viewTypes?.ConvertAll(x => (ViewType)((int)x)));

            dataAccess.SetDataList(0, elementIds);
        }
    }
}