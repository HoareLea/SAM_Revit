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
    public class SAMCoreRenameSpaces : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4c499172-736e-4a58-9898-50237b2f48aa");

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
        public SAMCoreRenameSpaces()
          : base("SAMCore.RenameSpaces", "SAMCore.RenameSpaces",
              "Renames Spaces",
              "SAM", "Revit")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new Core.Grasshopper.GooTextMapParam(), "_textMap", "_textMap", "SAM Core Text Map", GH_ParamAccess.item);
            int index = inputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.SpatialElement(), "_spaces_", "_spaces_", "Revit Spaces", GH_ParamAccess.list);
            inputParamManager[index].Optional = true;

            global::Grasshopper.Kernel.Parameters.Param_Integer param_Integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "maxLength_", NickName = "maxLength_", Description = "Max Length of the name", Access = GH_ParamAccess.item};
            param_Integer.SetPersistentData(int.MaxValue);
            inputParamManager.AddParameter(param_Integer);

            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.SpatialElement(), "Spaces", "Spaces", "Spaces", GH_ParamAccess.list);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(3, ref run) || !run)
                return;

            TextMap textMap = null;
            if(!dataAccess.GetData(0, ref textMap) || textMap == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Autodesk.Revit.DB.Mechanical.Space> spaces = new List<Autodesk.Revit.DB.Mechanical.Space>();
            dataAccess.GetDataList(1, spaces);

            int maxLength = int.MaxValue;
            if (!dataAccess.GetData(2, ref maxLength))
                maxLength = int.MaxValue;

            if (spaces == null || spaces.Count == 0)
            {
                Document document = RhinoInside.Revit.Revit.ActiveUIDocument.Document;
                spaces = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();
            }

            if (spaces == null || spaces.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            spaces = Core.Revit.Modify.RenameSpaces(spaces, textMap, maxLength);

            dataAccess.SetDataList(0, spaces);
        }
    }
}