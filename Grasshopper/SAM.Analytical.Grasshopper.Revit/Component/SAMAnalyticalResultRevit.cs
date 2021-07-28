using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalResultRevit : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d2166aa7-0a8c-4270-b6f5-eb57e83fb356");

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
        public SAMAnalyticalResultRevit()
          : base("SAMAnalytical.ResultsRevit", "SAMAnalytical.ResultsRevit",
              "Convert SAM Analytical Object to Revit Object",
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
                result.Add(new ParamDefinition(new Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new Param_GenericObject() { Name = "_results_", NickName = "_results_", Description = "SAM Analytical Results", Optional = true, Access = GH_ParamAccess.list }, ParamRelevance.Occasional));

                Param_Boolean param_Boolean = new Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Element() { Name = "elements", NickName = "elements", Description = "Revit Elements", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_run");

            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AdjacencyCluster)
                adjacencyCluster = (AdjacencyCluster)sAMObject;
            else if (sAMObject is AnalyticalModel)
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;

            if(adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IResult> results = null;
            index = Params.IndexOfInputParam("_results_");
            if(index != -1)
            {
                List<SAMObject> sAMObjects = new List<SAMObject>();
                if (dataAccess.GetDataList(index, sAMObjects) && sAMObjects != null)
                    results = sAMObjects.FindAll(x => x is IResult).ConvertAll(x => (IResult)x);
            }

            if (results == null)
            {
                results = new List<IResult>();
                adjacencyCluster.GetObjects<SpaceSimulationResult>()?.ForEach(x => results.Add(x));
                adjacencyCluster.GetObjects<ZoneSimulationResult>()?.ForEach(x => results.Add(x));
                adjacencyCluster.GetObjects<AdjacencyClusterSimulationResult>()?.ForEach(x => results.Add(x));
            }
                


            if (results == null || results.Count == 0)
                return;

            ConvertSettings convertSettings = new ConvertSettings(false, true, false);

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            StartTransaction(document);

            Dictionary<ElementId, Element> dictionary = new Dictionary<ElementId, Element>();
            foreach (IResult result in results)
            {
                List<Element> elements = null;
                if (result is SpaceSimulationResult)
                    elements = Analytical.Revit.Convert.ToRevit(adjacencyCluster, (SpaceSimulationResult)result, document, convertSettings)?.Cast<Element>().ToList();
                if (result is ZoneSimulationResult)
                    elements = Analytical.Revit.Convert.ToRevit(adjacencyCluster, (ZoneSimulationResult)result, document, convertSettings)?.Cast<Element>().ToList();
                if (result is AdjacencyClusterSimulationResult)
                {
                    ProjectInfo projectInfo = Analytical.Revit.Convert.ToRevit((AdjacencyClusterSimulationResult)result, document, convertSettings);
                    if (projectInfo != null)
                        elements = new List<Element>() { projectInfo };
                }

                elements?.ForEach(x => dictionary[x.Id] = x);
            }

            index = Params.IndexOfOutputParam("elements");
            if (index != -1)
                dataAccess.SetDataList(index, dictionary.Values.ToList());
        }
    }
}