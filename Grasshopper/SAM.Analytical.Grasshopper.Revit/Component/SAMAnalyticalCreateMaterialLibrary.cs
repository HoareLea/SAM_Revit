using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalCreateMaterialLibrary : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f836f629-b8fb-41a8-9611-6783933ad6b2");

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
        public SAMAnalyticalCreateMaterialLibrary()
          : base("SAMAnalytical.CreateMaterialLibrary", "SAMAnalytical.CreateMaterialLibrary",
              "Create SAM Material Library",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddTextParameter("path_", "_path_", "Path to csv file", GH_ParamAccess.item);
            index = inputParamManager.AddTextParameter("_name_", "_name_", "SAM Material Library Name", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooMaterialLibraryParam(), "MaterialLibrary", "MaterialLibrary", "SAM MaterialLibrary", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string path = null;
            if (!dataAccess.GetData(0, ref path) || string.IsNullOrWhiteSpace(path))
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


            string name = null;
            dataAccess.GetData(1, ref name);

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
                name,
                namesIndex);

            dataAccess.SetData(0, new GooMaterialLibrary(result));
        }
    }
}