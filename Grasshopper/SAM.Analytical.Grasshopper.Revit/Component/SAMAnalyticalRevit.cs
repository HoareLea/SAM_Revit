using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalRevit : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f60b6d3c-9af8-41a2-8c54-f26d82d55078");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        //private HashSet<ElementId> elementIds = new HashSet<ElementId>();

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
        protected override ParamDefinition[] Inputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_convertSettings_", NickName = "_convertSettings_", Description = "SAM ConvertSettings", Optional = true, Access = GH_ParamAccess.item }, ParamRelevance.Occasional));

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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Element() { Name = "elements", NickName = "elements", Description = "Revit Elements", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Parameters Updated", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            int index_Elements = -1;
            int index_Successful = -1;

            index_Successful = Params.IndexOfOutputParam("successful");
            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, false);

            index_Elements = Params.IndexOfOutputParam("elements");
            if(index_Elements != -1)
                dataAccess.SetData(index_Elements, null);

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            ConvertSettings convertSettings = null;
            index = Params.IndexOfInputParam("_convertSettings_");
            if (index != -1)
                dataAccess.GetData(index, ref convertSettings);

            convertSettings = this.UpdateSolutionEndEventHandler(convertSettings);

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            StartTransaction(document);

            if (!(sAMObject is Panel) && !(sAMObject is Aperture) && !(sAMObject is Space) && !(sAMObject is AdjacencyCluster) && !(sAMObject is AnalyticalModel))
                return;

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)sAMObject;
                convertSettings.AddParameter("AdjacencyCluster", adjacencyCluster);
            }
            else if (sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                convertSettings.AddParameter("AnalyticalModel", (AnalyticalModel)sAMObject);
            }
                

            if(adjacencyCluster != null)
            {
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

            //if(elements != null)
            //{
            //    foreach(Element element in elements)
            //    {
            //        ElementId elementId = element.Id;
            //        if (elementId == null)
            //            continue;

            //        elementIds.Add(elementId);
            //    }
            //}

            if (index_Elements != -1)
                dataAccess.SetDataList(index_Elements, elements);

            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, elements != null && elements.Count() > 0);
        }

        //protected override void OnAfterStart(Document document, string strTransactionName)
        //{
        //    base.OnAfterStart(document, strTransactionName);

        //    elementIds = new HashSet<ElementId>();
        //}

        //protected override void OnBeforeCommit(Document document, string strTransactionName)
        //{
        //    base.OnBeforeCommit(document, strTransactionName);

        //    if(elementIds != null && elementIds.Count != 0)
        //    {
        //        List<Wall> walls = new FilteredElementCollector(document, elementIds).OfClass(typeof(Wall)).Cast<Wall>().ToList();
        //        if(walls != null && walls.Count != 0)
        //        {
        //            Dictionary<Wall, int> dictionary = new Dictionary<Wall, int>();
        //            walls.ForEach(x => dictionary[x] = x.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).AsInteger());

        //            using (SubTransaction subTransaction = new SubTransaction(document))
        //            {
        //                subTransaction.Start();
                        
        //                foreach (Wall wall in dictionary.Keys)
        //                    wall.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(0);

        //                document.Regenerate();

        //                subTransaction.Commit();

        //                subTransaction.Start();

        //                foreach (KeyValuePair<Wall, int> keyValuePair in dictionary)
        //                    keyValuePair.Key.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).Set(keyValuePair.Value);

        //                subTransaction.Commit();
        //            }
        //        }
        //    }
        //}
    }
}