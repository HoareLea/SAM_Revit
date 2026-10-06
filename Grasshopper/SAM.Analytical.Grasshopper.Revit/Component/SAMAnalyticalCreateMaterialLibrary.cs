// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalCreateMaterialLibrary : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f836f629-b8fb-41a8-9611-6783933ad6b2");

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
        public SAMAnalyticalCreateMaterialLibrary()
          : base("SAMAnalytical.CreateMaterialLibrary", "SAMAnalytical.CreateMaterialLibrary",
              "Create SAM Material Library",
              "SAM", "Analytical")
        {
            // GH_SAMVariableOutputParameterComponent.RegisterInputParams clones each Param via
            // IGH_Param.Clone(), which silently resets NickName to Name for stock Grasshopper
            // param types when the two differ. The original component registered "path_" with a
            // different NickName ("_path_") — restore it here after base registration completes.
            int index = Params.IndexOfInputParam("path_");
            if (index != -1)
                Params.Input[index].NickName = "_path_";
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "path_", NickName = "_path_", Description = "Path to csv file", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name_", NickName = "_name_", Description = "SAM Material Library Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new GooMaterialLibraryParam() { Name = "MaterialLibrary", NickName = "MaterialLibrary", Description = "SAM MaterialLibrary", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index = Params.IndexOfInputParam("path_");
            string path = null;
            if (index == -1 || !dataAccess.GetData(index, ref path) || string.IsNullOrWhiteSpace(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (!System.IO.File.Exists(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string[] lines = System.IO.File.ReadAllLines(path);
            if (lines == null || lines.Length == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int namesIndex = 0;
            if (lines[0] != null && lines[0].ToUpper().Contains("FILEPATH"))
                namesIndex = 1;


            index = Params.IndexOfInputParam("_name_");
            string name = null;
            if (index != -1)
                dataAccess.GetData(index, ref name);

            TypeMap typeMap;
            if (!ActiveSetting.Setting.TryGetValue(Core.Revit.ActiveSetting.Name.ParameterMap, out typeMap) || typeMap == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string parameterName_Type = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), Analytical.Revit.RevitMaterialParameter.TypeName);
            string parameterName_Name = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), "Name");
            string parameterName_MaterialDescription = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), "Description");
            string parameterName_DefaultThickness = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), Core.MaterialParameter.DefaultThickness);
            string parameterName_ThermalConductivity = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), "ThermalConductivity");
            string parameterName_SpecificHeatCapacity = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), "SpecificHeatCapacity");
            string parameterName_Density = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), "Density");
            string parameterName_VapourDiffusionFactor = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), MaterialParameter.VapourDiffusionFactor);
            string parameterName_ExternalSolarReflectance = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.ExternalSolarReflectance);
            string parameterName_InternalSolarReflectance = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.InternalSolarReflectance);
            string parameterName_ExternalLightReflectance = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.ExternalLightReflectance);
            string parameterName_InternalLightReflectance = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.InternalLightReflectance);
            string parameterName_ExternalEmissivity = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.ExternalEmissivity);
            string parameterName_InternalEmissivity = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.InternalEmissivity);
            string parameterName_IgnoreThermalTransmittanceCalculations = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), OpaqueMaterialParameter.IgnoreThermalTransmittanceCalculations);
            string parameterName_SolarTransmittance = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.SolarTransmittance);
            string parameterName_LightTransmittance = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.LightTransmittance);
            string parameterName_IsBlind = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.IsBlind);
            string parameterName_HeatTransferCoefficient = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), GasMaterialParameter.HeatTransferCoefficient);
            string parameterName_DynamicViscosity = typeMap.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), "DynamicViscosity");

            MaterialLibrary result = Create.MaterialLibrary(
                path,
                parameterName_Type,
                parameterName_Name,
                parameterName_MaterialDescription,
                parameterName_DefaultThickness,
                parameterName_ThermalConductivity,
                parameterName_SpecificHeatCapacity,
                parameterName_Density,
                parameterName_VapourDiffusionFactor,
                parameterName_ExternalSolarReflectance,
                parameterName_InternalSolarReflectance,
                parameterName_ExternalLightReflectance,
                parameterName_InternalLightReflectance,
                parameterName_ExternalEmissivity,
                parameterName_InternalEmissivity,
                parameterName_IgnoreThermalTransmittanceCalculations,
                parameterName_SolarTransmittance,
                parameterName_LightTransmittance,
                parameterName_IsBlind,
                parameterName_HeatTransferCoefficient,
                parameterName_DynamicViscosity,
                name,
                namesIndex);

            index = Params.IndexOfOutputParam("MaterialLibrary");
            if (index != -1)
                dataAccess.SetData(index, new GooMaterialLibrary(result));
        }
    }
}
