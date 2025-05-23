﻿using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitSAMPanelsFromSpaces : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("ed3d4ef2-833d-40ce-9ad4-f20011ffac7e");

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
        public RevitSAMPanelsFromSpaces()
          : base("Revit.PanelsFromSpaces", "Revit.PanelsFromSpaces",
              "Query Panels based on Space Geometry",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_space", "_space", "Revit Space Instance", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_merge_", "_merge_", "Merge Coplanar Panels", GH_ParamAccess.item, true);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Walls", "Walls", "SAM Analytical Wall Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "Floors", "Floors", "SAM Analytical Floor Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "Roofs", "Roofs", "SAM Analytical Roof Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "RedundantPanels", "RedundantPanels", "RedundantPanels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(2, ref run) || !run)
                return;

            bool merge = true;
            if (!dataAccess.GetData(1, ref merge))
                return;

            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dynamic obj = objectWrapper.Value;

            ElementId aId = obj.Id as ElementId;

            string message = null;

            Autodesk.Revit.DB.Mechanical.Space space = (obj.Document as Document).GetElement(aId) as Autodesk.Revit.DB.Mechanical.Space;
            if (space == null)
            {
                message = "Invalid Element";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                return;
            }

            if (space.Location == null)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                message = string.Format("Cannot generate Panels. Space {0} [ElementId: {1}] not enclosed", space.Name, space.Id.IntegerValue);
#else
                message = string.Format("Cannot generate Panels. Space {0} [ElementId: {1}] not enclosed", space.Name, space.Id.Value);
#endif

                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                return;
            }

            if (space.Volume < Core.Tolerance.MacroDistance)
            {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                message = string.Format("Space cannot be converted because it has no volume. Space {0} [ElementId: {1}] not enclosed", space.Name, space.Id.IntegerValue);
#else
                message = string.Format("Space cannot be converted because it has no volume. Space {0} [ElementId: {1}] not enclosed", space.Name, space.Id.Value);
#endif

                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                return;
            }

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            List<Panel> panels = Analytical.Revit.Create.Panels(space, convertSettings);
            if (panels == null || panels.Count == 0)
            {
                message = "Panels ould not be generated";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                return;
            }

            panels.RemoveAll(x => x == null);

            List<Panel> redundantPanels = new List<Panel>();
            if (merge)
                panels = Analytical.Query.MergeCoplanarPanels(panels, Core.Tolerance.MacroDistance, ref redundantPanels);

            dataAccess.SetDataList(0, panels.FindAll(x => Analytical.Query.PanelGroup(x.PanelType) == PanelGroup.Wall));
            dataAccess.SetDataList(1, panels.FindAll(x => Analytical.Query.PanelGroup(x.PanelType) == PanelGroup.Floor));
            dataAccess.SetDataList(2, panels.FindAll(x => Analytical.Query.PanelGroup(x.PanelType) == PanelGroup.Roof));
            dataAccess.SetDataList(3, redundantPanels);
        }
    }
}