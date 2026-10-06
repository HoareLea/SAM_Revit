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
    public class RevitSAMPanelsByCurtainWall : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7a1e91e5-24f2-48f7-ae6c-b98438c6fbbd");

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
        public RevitSAMPanelsByCurtainWall()
          : base("Revit.PanelsByCurtainWall", "Revit.PanelsByCurtainWall",
              "Convert Revit Curtain Wall To SAM Analytical Panels \n*optional input ActiveDocument to get all curtain walls from project",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_curtainWall", NickName = "_curtainWall", Description = "Revit Curtain Wall\n*or ActiveDocument to get all curtain walls from project", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_IncludeNonVisibleObjects = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_includeNonVisibleObjects_", NickName = "_includeNonVisibleObjects_", Description = "Include Non Visible Objects", Access = GH_ParamAccess.item };
                param_IncludeNonVisibleObjects.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_IncludeNonVisibleObjects, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "panels", NickName = "panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_curtainWall");
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

            index = Params.IndexOfInputParam("_includeNonVisibleObjects_");
            bool includeNonVisibleObjects = false;
            if (index == -1 || !dataAccess.GetData(index, ref includeNonVisibleObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConvertSettings convertSettings = new ConvertSettings(true, true, true, useProjectLocation);

            List<Autodesk.Revit.DB.Wall> walls = new List<Autodesk.Revit.DB.Wall>();

            dynamic @object = objectWrapper.Value;
            if(@object is RhinoInside.Revit.GH.Types.ProjectDocument)
            {
                Document document = ((RhinoInside.Revit.GH.Types.ProjectDocument)@object).Value;

                walls = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Wall)).Cast<Autodesk.Revit.DB.Wall>().ToList();
            }
            else
            {
                ElementId aId = @object.Id as ElementId;

                Autodesk.Revit.DB.Wall wall = (@object.Document as Document).GetElement(aId) as Autodesk.Revit.DB.Wall;
                if(wall != null)
                {
                    walls.Add(wall);
                }
            }

            if (walls == null || walls.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Element");
                return;
            }


            List<Core.ISAMObject> sAMObjects = new List<Core.ISAMObject>();

            foreach(Autodesk.Revit.DB.Wall wall in walls)
            {
                IEnumerable<ElementId> elementIds = wall?.CurtainGrid?.GetPanelIds();
                if(elementIds == null || elementIds.Count() == 0)
                {
                    continue;
                }

                foreach(ElementId elementId in elementIds)
                {
                    Autodesk.Revit.DB.Panel panel = wall.Document.GetElement(elementId) as Autodesk.Revit.DB.Panel;
                    if(panel == null)
                    {
                        continue;
                    }

                    List<Panel> panels = Analytical.Revit.Convert.ToSAM(panel, includeNonVisibleObjects, convertSettings);
                    if(panels != null)
                    {
                        sAMObjects.AddRange(panels.Cast<Core.ISAMObject>());
                    }
                }
            }

            index = Params.IndexOfOutputParam("panels");
            if (index != -1)
                dataAccess.SetDataList(index, sAMObjects);
        }
    }
}
