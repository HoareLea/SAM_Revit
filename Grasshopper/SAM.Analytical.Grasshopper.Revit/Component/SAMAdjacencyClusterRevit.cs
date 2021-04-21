using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAdjacencyClusterRevit : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("54287c2b-9aaf-478d-ac32-4d258aec2548");

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
        public SAMAdjacencyClusterRevit()
          : base("SAMAdjacencyCluster.Revit", "SAMAdjacencyCluster.Revit",
              "SAM AdjacencyCluster to Revit",
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
                result.Add(ParamDefinition.FromParam(new GooAdjacencyClusterParam() { Name = "_adjacencyCluster", NickName = "_adjacencyCluster", Description = "SAM Analytical AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_convertSettings_", NickName = "_convertSettings_", Description = "SAM ConvertSettings", Optional = true, Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));

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
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.Element() { Name = "elements", NickName = "elements", Description = "Revit Elements", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            ConvertSettings convertSettings = null;
            index = Params.IndexOfInputParam("_convertSettings_");
            if (index != -1)
                dataAccess.GetData(index, ref convertSettings);

            convertSettings = this.UpdateSolutionEndEventHandler(convertSettings);

            AdjacencyCluster adjacencyCluster = null;
            index = Params.IndexOfInputParam("_adjacencyCluster");
            if (index == -1 || !dataAccess.GetData(index, ref adjacencyCluster))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            StartTransaction(document);

            convertSettings.AddParameter("AdjacencyCluster", adjacencyCluster);

            List<Element> elements = Analytical.Revit.Convert.ToRevit(adjacencyCluster, document, convertSettings);

            index = Params.IndexOfOutputParam("elements");
            if (index != -1)
                dataAccess.SetDataList(index, elements);
        }
    }
}