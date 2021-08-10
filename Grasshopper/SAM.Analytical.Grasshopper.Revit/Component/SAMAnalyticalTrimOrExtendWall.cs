using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    /// <summary>
    /// Gets the unique ID for this component. Do not change this ID after release.
    /// </summary>
    public class SAMAnalyticalTrimOrExtendWall : SAMTransactionalChainComponent
    {
        public override Guid ComponentGuid => new Guid("e237e813-ed88-483d-8124-3bb5551b7103");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalTrimOrExtendWall()
          : base("SAMAnalytical.TrimOrExtendWall", "SAMAnalytical.TrimOrExtendWall",
              "Modify Trim Or Extend Unconnected Revit Walls",
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.HostObject() { Name = "_walls", NickName = "_walls", Description = "Revit Walls", Access = GH_ParamAccess.list }, ParamRelevance.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_maxDistance_", NickName = "_maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.52);
                result.Add(new ParamDefinition(param_Number, ParamRelevance.Binding));

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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.HostObject() { Name = "walls", NickName = "walls", Description = "Adjusted Revit Walls", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(2, ref run) || !run)
                return;

            List<Autodesk.Revit.DB.HostObject> hostObjects = new List<Autodesk.Revit.DB.HostObject>();
            index = Params.IndexOfInputParam("_walls");
            if (index == -1 || !dataAccess.GetDataList(index, hostObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double maxDistance = double.NaN;
            index = Params.IndexOfInputParam("_maxDistance_");
            if (index == -1 || !dataAccess.GetData(index, ref maxDistance))
                return;

            hostObjects.ForEach(x => StartTransaction(x.Document));

            List<Autodesk.Revit.DB.Wall> result = Analytical.Revit.Modify.TrimOrExtendWall(hostObjects.FindAll(x => x is Autodesk.Revit.DB.Wall).Cast<Autodesk.Revit.DB.Wall>(), maxDistance);

            index = Params.IndexOfOutputParam("walls");
            if (index != -1)
                dataAccess.SetDataList(index, result);
        }
    }
}