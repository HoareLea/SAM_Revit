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

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitSAMFloorsAndRoofsFromSpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("90e18255-7b5e-4154-8f8c-615b62be273b");

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
        public RevitSAMFloorsAndRoofsFromSpaces()
          : base("Revit.FloorsAndRoofsFromSpaces", "Revit.FloorsAndRoofsFromSpaces",
              "Query Floors and Roofs based on Space Planar Geometry",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_space", NickName = "_space", Description = "Revit Space Instance", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Merge = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_merge_", NickName = "_merge_", Description = "Merge Coplanar Panels", Access = GH_ParamAccess.item };
                param_Merge.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Merge, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Floors", NickName = "Floors", Description = "SAM Analytical Floor Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Roofs", NickName = "Roofs", Description = "SAM Analytical Roof Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Air", NickName = "Air", Description = "SAM Analytical Air Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "RedundantPanels", NickName = "RedundantPanels", Description = "RedundantPanels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_merge_");
            bool merge = true;
            if (index == -1 || !dataAccess.GetData(index, ref merge))
                return;

            index = Params.IndexOfInputParam("_space");
            GH_ObjectWrapper objectWrapper = null;

            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dynamic obj = objectWrapper.Value;

            ElementId aId = obj.Id as ElementId;

            string message = null;

            Document document = obj.Document as Document;

            Autodesk.Revit.DB.Mechanical.Space space = document.GetElement(aId) as Autodesk.Revit.DB.Mechanical.Space;
            if (space == null)
            {
                message = "Invalid Element";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                return;
            }

            if (space.Location == null)
            {
                message = string.Format("Cannot generate Panels. Space {0} [ElementId: {1}] not enclosed", space.Name, space.Id.Value);

                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                return;
            }

            if (space.Volume < Core.Tolerance.MacroDistance)
            {
                message = string.Format("Space cannot be converted because it has no volume. Space {0} [ElementId: {1}] not enclosed", space.Name, space.Id.Value);

                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                return;
            }

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            List<Panel> panels = Analytical.Revit.Create.Panels(space, convertSettings);
            if (panels == null || panels.Count == 0)
            {
                message = "Panels could not be generated";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                return;
            }

            panels.RemoveAll(x => x == null);

            List<Panel> panels_Temp = new List<Panel>();
            foreach (Panel panel in panels)
            {
                PanelType panelType = panel.PanelType;
                if(panelType == PanelType.Air)
                {
                    if (!panel.Normal.Collinear(Geometry.Spatial.Vector3D.WorldZ))
                        continue;
                }
                else
                {
                    PanelGroup panelGroup = Analytical.Query.PanelGroup(panel.PanelType);
                    if (panelGroup != PanelGroup.Floor && panelGroup != PanelGroup.Roof)
                        continue;
                }

                ElementId elementId = panel.ElementId();
                if (elementId == null || elementId == ElementId.InvalidElementId)
                {
                    panels_Temp.Add(panel);
                    continue;
                }

                HostObject hostObject = document.GetElement(elementId) as HostObject;
                if (hostObject == null)
                {
                    panels_Temp.Add(panel);
                    continue;
                }

                Geometry.Spatial.Plane plane = panel.PlanarBoundary3D.Plane;

                List<Geometry.Spatial.Face3D> face3Ds = Geometry.Revit.Query.Profiles(hostObject);
                if (face3Ds == null || face3Ds.Count == 0)
                {
                    panels_Temp.Add(panel);
                    continue;
                }

                Geometry.Spatial.Plane plane_Face3D = face3Ds.Find(x => plane.Coplanar(x.GetPlane()))?.GetPlane();
                if (plane_Face3D == null)
                {
                    panels_Temp.Add(panel);
                    continue;
                }

                Geometry.Spatial.Point3D point3D_Projected = Geometry.Spatial.Query.Project(plane_Face3D, plane.Origin);

                panel.Move(new Geometry.Spatial.Vector3D(plane.Origin, point3D_Projected));
                panels_Temp.Add(panel);
            }

            List<Panel> redundantPanels = new List<Panel>();
            if (merge)
                panels_Temp = Analytical.Query.MergeCoplanarPanels(panels_Temp, Core.Tolerance.MacroDistance, ref redundantPanels, true, Core.Tolerance.MacroDistance);

            index = Params.IndexOfOutputParam("Floors");
            if (index != -1)
                dataAccess.SetDataList(index, panels_Temp.FindAll(x => Analytical.Query.PanelGroup(x.PanelType) == PanelGroup.Floor));

            index = Params.IndexOfOutputParam("Roofs");
            if (index != -1)
                dataAccess.SetDataList(index, panels_Temp.FindAll(x => Analytical.Query.PanelGroup(x.PanelType) == PanelGroup.Roof));

            index = Params.IndexOfOutputParam("Air");
            if (index != -1)
                dataAccess.SetDataList(index, panels_Temp.FindAll(x => x.PanelType == PanelType.Air));

            index = Params.IndexOfOutputParam("RedundantPanels");
            if (index != -1)
                dataAccess.SetDataList(index, redundantPanels);
        }
    }
}
