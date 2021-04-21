using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMCoreRenameSpaces : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("4c499172-736e-4a58-9898-50237b2f48aa");

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
        public SAMCoreRenameSpaces()
          : base("SAMCore.RenameSpaces", "SAMCore.RenameSpaces",
              "Renames Spaces",
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
                result.Add(ParamDefinition.FromParam(new Core.Grasshopper.GooTextMapParam() { Name = "_textMap_", NickName = "_textMap_", Description = "SAM Core TextMap", Optional = true, Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "_spaces_", NickName = "_spaces_", Description = "Revit Spaces", Optional = true, Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Integer param_Integer = new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "maxLength_", NickName = "maxLength_", Description = "Max Length of the name", Access = GH_ParamAccess.item };
                param_Integer.SetPersistentData(int.MaxValue);
                result.Add(ParamDefinition.FromParam(param_Integer, ParamVisibility.Voluntary));

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
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "spaces", NickName = "spaces", Description = "Revit Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            TextMap textMap = null;
            index = Params.IndexOfInputParam("_spaces_");
            if(index == -1 || !dataAccess.GetData(index, ref textMap) || textMap == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Autodesk.Revit.DB.Mechanical.Space> spaces = new List<Autodesk.Revit.DB.Mechanical.Space>();
            index = Params.IndexOfOutputParam("_spaces_");
            if (index != -1)
                dataAccess.GetDataList(index, spaces);

            int maxLength = int.MaxValue;
            index = Params.IndexOfInputParam("maxLength_");
            if (index == -1 || !dataAccess.GetData(index, ref maxLength))
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

            spaces.ForEach(x => StartTransaction(x.Document));

            spaces = Core.Revit.Modify.RenameSpaces(spaces, textMap, maxLength);

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces);
        }
    }
}