using Autodesk.Revit.DB;
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
    public class SAMAnalyticalResultRevit : SAMTransactionComponent
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
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new Param_GenericObject(), "_analytical", "_analytical", "SAM Analytical Object", GH_ParamAccess.item);
            inputParamManager.AddParameter(new Param_GenericObject() { Optional = true}, "_results_", "_results_", "SAM Analytical Results", GH_ParamAccess.list);

            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Element(), "Elements", "Element", "Revit Elements", GH_ParamAccess.list);
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

            dataAccess.SetDataList(0, dictionary.Values.ToList());
        }
    }
}