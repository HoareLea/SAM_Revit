// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitSAMAnalyticalByElement : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9b809657-8de3-466e-b814-973b0677a37a");

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
        public RevitSAMAnalyticalByElement()
          : base("Revit.SAMAnalyticalByElement", "Revit.SAMAnalyticalByElement",
              "Convert Revit To SAM Analytical Object ie. Panel, Space",
              "SAM", "Revit")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_revitElement", NickName = "_revitElement", Description = "Revit Element instance", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_UseProjectLocation = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_useProjectLocation_", NickName = "_useProjectLocation_", Description = "Transform geometry using Revit Project Location", Access = GH_ParamAccess.item };
                param_UseProjectLocation.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_UseProjectLocation, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Run = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Run.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Run, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "analyticalObject", NickName = "analyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "report", NickName = "report", Description = "Report", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = Params.IndexOfInputParam("_run");
            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            index = Params.IndexOfInputParam("_revitElement");
            GH_ObjectWrapper objectWrapper = null;

            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_useProjectLocation_");
            bool useProjectLocation = false;
            if (index == -1 || !dataAccess.GetData(index, ref useProjectLocation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int index_AnalyticalObject = Params.IndexOfOutputParam("analyticalObject");
            int index_Report = Params.IndexOfOutputParam("report");

            ConvertSettings convertSettings = new ConvertSettings(true, true, true, useProjectLocation);
            IEnumerable<Core.ISAMObject> sAMObjects = null;
            string message = null;

            dynamic obj = objectWrapper.Value;
            if(obj is RhinoInside.Revit.GH.Types.ProjectDocument)
            {
                Document document = ((RhinoInside.Revit.GH.Types.ProjectDocument)obj).Value;
                List<Panel> panels = Analytical.Revit.Convert.ToSAM_Panels(document, convertSettings);
                if (panels != null)
                    sAMObjects = panels.Cast<Core.ISAMObject>();

                if (sAMObjects == null || sAMObjects.Count() == 0)
                {
                    message = string.Format("Cannot convert Document.");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                    if (index_Report != -1)
                        dataAccess.SetData(index_Report, message);

                    return;
                }

                if (index_AnalyticalObject != -1)
                    dataAccess.SetDataList(index_AnalyticalObject, sAMObjects);

                message = string.Format("Document converted");
                if (index_Report != -1)
                    dataAccess.SetData(index_Report, message);

                return;
            }

            ElementId aId = obj.Id as ElementId;

            Element element = (obj.Document as Document).GetElement(aId);
            if (element == null)
            {
                message = "Invalid Element";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                if (index_Report != -1)
                    dataAccess.SetData(index_Report, message);

                return;
            }

            if (element is FamilyInstance && ((FamilyInstance)element).Symbol.Family.IsInPlace)
            {
                message = string.Format("Cannot convert In-Place family. ElementId: {0} ", element.Id.Value);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                if (index_Report != -1)
                    dataAccess.SetData(index_Report, message);

                return;
            }


            if (element is RevitLinkInstance)
            {
                List<Panel> panels = Analytical.Revit.Convert.ToSAM_Panels((RevitLinkInstance)element, convertSettings);
                if (panels != null)
                    sAMObjects = panels.Cast<Core.ISAMObject>();
            }
            else
            {
                if(element is Level)
                {
                    sAMObjects = new List<Core.ISAMObject>() { Architectural.Revit.Convert.ToSAM((Level)element, convertSettings) };
                }
                else
                {
                    try
                    {
                        sAMObjects = Analytical.Revit.Convert.ToSAM(element, convertSettings);
                    }
                    catch (Exception exception)
                    {
                        message = string.Format("Cannot convert Element. ElementId: {0} Category: {1} Exception: {2}", element.Id.Value, element.Category.Name, exception.Message);
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                        if (index_Report != -1)
                            dataAccess.SetData(index_Report, message);
                    }
                }
            }

            if (sAMObjects == null || sAMObjects.Count() == 0)
            {
                message = string.Format("Cannot convert Element. ElementId: {0} Category: {1}", element.Id.Value, element.Category.Name);

                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                if (index_Report != -1)
                    dataAccess.SetData(index_Report, message);

                return;
            }

            if (index_AnalyticalObject != -1)
                dataAccess.SetDataList(index_AnalyticalObject, sAMObjects);

            message = string.Format("Element converted. ElementId: {0} Category: {1}", element.Id.Value, element.Category.Name);

            if (index_Report != -1)
                dataAccess.SetData(index_Report, message);
        }
    }
}
