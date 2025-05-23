﻿using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalUpdatePanelType : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1a975cf1-1df0-4e14-9d52-c900e8d569f8");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdatePanelType()
          : base("SAMAnalytical.UpdatePanelType", "SAManalytical.UpdatePanelType",
              "Revit got limitation and in order to create some element change in type is required\nExample..tilted wall or floor need to be change to Roof or Fllor etc\nThis node just recognize by normal orientation and modifies PanelType but keeps original construction.\nHosted windows on tilted wall if normal up will become skylight in roof.\nTilted wall normal down will become Floor",
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
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.ElementType() { Name = "elementType", NickName = "elementType", Description = "Revit ElementType that already exist in Revit and match by name", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            Document document = RhinoInside.Revit. Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = null;
            if(sAMObject is Panel)
            {
                panels = new List<Panel>() { (Panel)sAMObject };
            }
            else if(sAMObject is AdjacencyCluster)
            {
                panels = ((AdjacencyCluster)sAMObject).GetPanels();
            }
            else if(sAMObject is AnalyticalModel)
            {
                panels = ((AnalyticalModel)sAMObject).GetPanels();
            }

            if(panels == null || panels.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            StartTransaction(document);

            List<ElementType> elementTypes = Analytical.Revit.Modify.UpdatePanelTypes(document, panels);

            int index_Analytical = Params.IndexOfOutputParam("analytical");
            if(index_Analytical != -1)
            {
                if(sAMObject is Panel)
                {
                    dataAccess.SetData(index_Analytical, panels?.FirstOrDefault());
                }
                else if(sAMObject is AnalyticalModel)
                {
                    AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);
                    panels.ForEach(x => analyticalModel.AddPanel(x));
                    dataAccess.SetData(index_Analytical, analyticalModel);
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                    panels.ForEach(x => adjacencyCluster.AddObject(x));
                    dataAccess.SetData(index_Analytical, adjacencyCluster);
                }
            }

            int index_ElementType = Params.IndexOfOutputParam("elementType");
            if(index_ElementType != -1)
            {
                dataAccess.SetDataList(index_ElementType, elementTypes);
            }
        }
    }
}