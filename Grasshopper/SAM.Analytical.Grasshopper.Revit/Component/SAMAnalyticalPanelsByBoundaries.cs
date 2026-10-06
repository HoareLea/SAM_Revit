// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalPanelsByBoundaries : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fcd537c9-0a80-472c-bc64-07f11c9aa879");

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
        public SAMAnalyticalPanelsByBoundaries()
          : base("SAMAnalytical.PanelsByBoundaries", "SAMAnalytical.PanelsByBoundaries",
              "Creates Wall Panels based on Space Boundaries",
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
                result.Add(new GH_SAMParam(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "_space", NickName = "_space", Description = "Revit Space or Wall", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new RhinoInside.Revit.GH.Parameters.Level() { Name = "level_Lower_", NickName = "level_Lower_", Description = "Revit Lower Level", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new RhinoInside.Revit.GH.Parameters.Level() { Name = "level_Upper_", NickName = "level_Upper_", Description = "Revit Upper Level", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "Panels", NickName = "Panels", Description = "SAM Analytical Panels", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = Params.IndexOfInputParam("_run");
            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            index = Params.IndexOfInputParam("_space");
            SpatialElement spatialElement = null;
            if(index == -1 || !dataAccess.GetData(index, ref spatialElement) || spatialElement == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Document document = spatialElement.Document;

            index = Params.IndexOfInputParam("level_Lower_");
            Level level_Low = null;
            if (index != -1)
                dataAccess.GetData(index, ref level_Low);

            index = Params.IndexOfInputParam("level_Upper_");
            Level level_High = null;
            if (index != -1)
                dataAccess.GetData(index, ref level_High);

            if(level_Low == null || level_High == null)
            {
                BoundingBoxXYZ boundingBoxXYZ = spatialElement.get_BoundingBox(null);
                if(boundingBoxXYZ != null)
                {
                    if (level_Low == null)
                        level_Low = Core.Revit.Query.LowLevel(document, boundingBoxXYZ.Min.Z);

                    if (level_High == null)
                        level_High = Core.Revit.Query.HighLevel(document, boundingBoxXYZ.Max.Z);
                }
            }

            if (level_Low == null || level_High == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation_Low = Units.Revit.Convert.ToSI(level_Low.Elevation, SpecTypeId.Length);
            double elevation_High = Units.Revit.Convert.ToSI(level_High.Elevation, SpecTypeId.Length);

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            List<Panel> result = Analytical.Revit.Create.Panels(spatialElement, elevation_Low, elevation_High, convertSettings);

            index = Params.IndexOfOutputParam("Panels");
            if (index != -1)
                dataAccess.SetDataList(index, result.ConvertAll(x => new GooPanel(x)));
        }
    }
}
