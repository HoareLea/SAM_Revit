// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreCreateConvertSettings : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("da750879-e8e0-446d-ab7d-a705358ce304");

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
        public SAMCoreCreateConvertSettings()
          : base("SAMCore.CreateConvertSettings", "SAMCore.CreateConvertSettings",
              "Create SAM Core ConvertSettings",
              "SAM WIP", "Revit")
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

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_ConvertGeometry = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_convertGeometry_", NickName = "_convertGeometry_", Description = "Convert Geometry", Access = GH_ParamAccess.item };
                param_ConvertGeometry.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_ConvertGeometry, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_ConvertParameters = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_convertParameters_", NickName = "_convertParameters_", Description = "Convert Parameters", Access = GH_ParamAccess.item };
                param_ConvertParameters.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_ConvertParameters, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_RemoveExisting = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_removeExisting_", NickName = "_removeExisting_", Description = "Remove existing Revit element if exists before conversion ", Access = GH_ParamAccess.item };
                param_RemoveExisting.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_RemoveExisting, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooConvertSettingsParam() { Name = "ConvertSettings", NickName = "ConvertSettings", Description = "SAM Core Convert Settings", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index = Params.IndexOfInputParam("_convertGeometry_");
            bool convertGeometry = true;
            if (index == -1 || !dataAccess.GetData(index, ref convertGeometry))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_convertParameters_");
            bool convertParameters = true;
            if (index == -1 || !dataAccess.GetData(index, ref convertParameters))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_removeExisting_");
            bool removeExisting = true;
            if (index == -1 || !dataAccess.GetData(index, ref removeExisting))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("ConvertSettings");
            if (index != -1)
                dataAccess.SetData(index, new GooConvertSettings(new Core.Revit.ConvertSettings(convertGeometry, convertParameters, removeExisting)));
        }
    }
}
