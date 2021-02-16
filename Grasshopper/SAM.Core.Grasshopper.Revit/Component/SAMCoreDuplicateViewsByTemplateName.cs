using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMCoreDuplicateViewsByTemplateName : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("33a15bff-cae9-45f6-8f06-b73da0bbc844");

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
        public SAMCoreDuplicateViewsByTemplateName()
          : base("SAMCore.DuplicateViewsByTemplateName", "SAMCore.DuplicateViewsByTemplateName",
              "Duplicates Views By Template Name",
              "SAM", "Revit")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index = -1;

            inputParamManager.AddTextParameter("_templateName_Source", "_templateName_Source", "Source Template Name", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_templateNames_Destination", "_templateName_Destionation", "Destination Templates Name", GH_ParamAccess.list);

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
            outputParamManager.AddGenericParameter("Views", "Views", "Views", GH_ParamAccess.list);
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

            List<string> templateNames = new List<string>();
            if (!dataAccess.GetDataList(1, templateNames))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Core.ViewType> viewTypes = null;

            List<string> viewTypeNames = new List<string>();
            if (dataAccess.GetDataList(2, viewTypeNames))
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

            List<View> views = Core.Revit.Modify.DuplicateViews(document, templateName, templateNames, viewTypes?.ConvertAll(x => (Autodesk.Revit.DB.ViewType)((int)x)));

            dataAccess.SetDataList(0, views);
        }
    }
}