// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalShellsBySpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("352f8762-3aba-43b8-9bfc-d1024cedafd1");

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
        public SAMAnalyticalShellsBySpaces()
          : base("SAMAnalytical.ShellsBySpaces", "SAMAnalytical.ShellsBySpaces",
              "Gets Shells By Spaces",
              "SAM", "Revit")
        {
            // GH_SAMVariableOutputParameterComponent.RegisterInputParams clones each Param via
            // IGH_Param.Clone(), which silently resets NickName to Name for stock Grasshopper
            // param types when the two differ. The original component registered "spaces_" with a
            // different NickName ("_space_") — restore it here after base registration completes.
            int index = Params.IndexOfInputParam("spaces_");
            if (index != -1)
                Params.Input[index].NickName = "_space_";
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "spaces_", NickName = "_space_", Description = "Revit Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Offset = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_offset_", NickName = "_offset_", Description = "Offset from bottom of space", Access = GH_ParamAccess.item };
                param_Offset.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(param_Offset, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_SnapTolerance = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_snapTolerance_", NickName = "_snapTolerance_", Description = "Snap Tolerance", Access = GH_ParamAccess.item };
                param_SnapTolerance.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(param_SnapTolerance, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Tolerance = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                param_Tolerance.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(param_Tolerance, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new Geometry.Grasshopper.GooSAMGeometryParam() { Name = "Shells", NickName = "Shells", Description = "SAM Geometry Shells", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = Params.IndexOfInputParam("_run");
            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            index = Params.IndexOfInputParam("spaces_");
            List<SpatialElement> spatialElements = new List<SpatialElement>();
            if (index == -1 || !dataAccess.GetDataList(index, spatialElements) || spatialElements == null || spatialElements.Count == 0)
                spatialElements = null;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            index = Params.IndexOfInputParam("_offset_");
            double offset = 0.1;
            if (index == -1 || !dataAccess.GetData(index, ref offset))
                offset = 0.1;

            index = Params.IndexOfInputParam("_snapTolerance_");
            double snapTolerance = Core.Tolerance.MacroDistance;
            if (index == -1 || !dataAccess.GetData(index, ref snapTolerance))
                snapTolerance = Core.Tolerance.MacroDistance;

            index = Params.IndexOfInputParam("_tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
                tolerance = Core.Tolerance.Distance;

            List<Geometry.Spatial.Shell> result = Analytical.Revit.Create.Shells(document, spatialElements?.ConvertAll(x => x as Autodesk.Revit.DB.Mechanical.Space).FindAll(x => x != null), offset, snapTolerance, tolerance);

            index = Params.IndexOfOutputParam("Shells");
            if (index != -1)
                dataAccess.SetDataList(index, result);
        }
    }
}
