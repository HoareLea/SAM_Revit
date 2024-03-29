﻿using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalCopyParameters : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("722a9210-b00c-405c-86c0-0c8047405bb7");

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
        public SAMAnalyticalCopyParameters()
          : base("SAMAnalytical.CopyParameters", "SAMAnalytical.CopyParameters",
              "Copy Parameters from spftware specific to generic \n *Is used when you store data from different sources like \n Tas, E+ etc...",
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
                result.Add(new ParamDefinition(new Param_GenericObject() { Name = "_tool", NickName = "_tool", Description = "Source Tool \n *Connect SAMAnalytical.Tool", Access = GH_ParamAccess.item }, ParamRelevance.Binding));

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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "spaces", NickName = "spaces", Description = "Revit Spaces", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            index = Params.IndexOfInputParam("_run");

            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
            {
                return;
            }

            DelimitedFileTable delimitedFileTable = Analytical.Revit.Query.DefaultToolsParameterMap();
            if (delimitedFileTable == null)
            {
                return;
            }

            Analytical.Revit.Tool tool = Analytical.Revit.Tool.Undefined;
            index = Params.IndexOfInputParam("_tool");
            if (index == -1 || !dataAccess.GetData(index, ref tool) || tool == Analytical.Revit.Tool.Undefined)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            StartTransaction(document);

            List<SpatialElement> spatialElements = null;
           if (Analytical.Revit.Modify.CopySpatialElementParameters(document, tool))
            {
                spatialElements = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<SpatialElement>().ToList();
            }

            index = Params.IndexOfOutputParam("elements");
            if (index != -1)
                dataAccess.SetDataList(index, spatialElements);
        }
    }
}