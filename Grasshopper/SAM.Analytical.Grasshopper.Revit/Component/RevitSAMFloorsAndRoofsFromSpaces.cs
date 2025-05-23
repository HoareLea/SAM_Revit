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
    public class RevitSAMFloorsAndRoofsFromSpaces : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("90e18255-7b5e-4154-8f8c-615b62be273b");

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
        public RevitSAMFloorsAndRoofsFromSpaces()
          : base("Revit.FloorsAndRoofsFromSpaces", "Revit.FloorsAndRoofsFromSpaces",
              "Query Floors and Roofs based on Space Planar Geometry",
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
            outputParamManager.AddParameter(new GooPanelParam(), "Floors", "Floors", "SAM Analytical Floor Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "Roofs", "Roofs", "SAM Analytical Roof Panels", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooPanelParam(), "Air", "Air", "SAM Analytical Air Panels", GH_ParamAccess.list);
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

            dataAccess.SetDataList(0, panels_Temp.FindAll(x => Analytical.Query.PanelGroup(x.PanelType) == PanelGroup.Floor));
            dataAccess.SetDataList(1, panels_Temp.FindAll(x => Analytical.Query.PanelGroup(x.PanelType) == PanelGroup.Roof));
            dataAccess.SetDataList(2, panels_Temp.FindAll(x => x.PanelType == PanelType.Air));
            dataAccess.SetDataList(3, redundantPanels);
        }
    }
}