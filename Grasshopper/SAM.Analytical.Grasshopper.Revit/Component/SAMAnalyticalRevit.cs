using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalRevit : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f60b6d3c-9af8-41a2-8c54-f26d82d55078");

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
        public SAMAnalyticalRevit()
          : base("SAMAnalytical.Revit", "SAMAnalytical.Revit",
              "Convert SAM Analytical Object to Revit Object",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Object", GH_ParamAccess.item);

            int index = inputParamManager.AddGenericParameter("_convertSettings_", "_convertSettings_", "SAM Convert Settings", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

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
            bool run = false;
            if (!dataAccess.GetData(2, ref run) || !run)
                return;

            ConvertSettings convertSettings = null;
            dataAccess.GetData(1, ref convertSettings);
            convertSettings = this.UpdateSolutionEndEventHandler(convertSettings);

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            if (sAMObject is AnalyticalModel)
                sAMObject = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            
            if(!(sAMObject is Panel) && !(sAMObject is Aperture) && !(sAMObject is Space) && !(sAMObject is AdjacencyCluster))
            {
                dataAccess.SetData(0, null);
                return;
            }

            if(sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = (AdjacencyCluster)sAMObject;
                adjacencyCluster.GetPanels()?.ForEach(x => Core.Revit.Modify.RemoveExisting(convertSettings, document, x));
                adjacencyCluster.GetSpaces()?.ForEach(x => Core.Revit.Modify.RemoveExisting(convertSettings, document, x));
            }
            else
            {
                Core.Revit.Modify.RemoveExisting(convertSettings, document, sAMObject);
            }

            object @object = Analytical.Revit.Convert.ToRevit(sAMObject as dynamic, document, convertSettings);

            IEnumerable<Element> elements = null;
            if (@object is IEnumerable)
                elements = ((IEnumerable)@object).Cast<Element>();
            else
                elements = new List<Element>() { @object as Element };

            dataAccess.SetDataList(0, elements);
        }
    }
}