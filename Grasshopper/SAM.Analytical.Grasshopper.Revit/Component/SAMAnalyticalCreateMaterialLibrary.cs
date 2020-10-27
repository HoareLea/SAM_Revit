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

            MapCluster mapCluster;
            if (!ActiveSetting.Setting.TryGetValue(Core.Revit.ActiveSetting.Name.ParameterMap, out mapCluster) || mapCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string parameterName_MaterialDescription = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), MaterialParameter.Description.Name(), 2);
            string parameterName_DefaultThickness = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), MaterialParameter.DefaultThickness.Name(), 2);
            string parameterName_ThermalConductivity = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), "ThermalConductivity", 2);
            string parameterName_SpecificHeatCapacity = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), "SpecificHeatCapacity", 2);
            string parameterName_Density = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), "Density", 2);
            string parameterName_VapourDiffusionFactor = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), MaterialParameter.VapourDiffusionFactor.Name(), 2);
            string parameterName_ExternalSolarReflectance = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.ExternalSolarReflectance.Name(), 2);
            string parameterName_InternalSolarReflectance = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.InternalSolarReflectance.Name(), 2);
            string parameterName_ExternalLightReflectance = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.ExternalLightReflectance.Name(), 2);
            string parameterName_InternalLightReflectance = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.InternalLightReflectance.Name(), 2);
            string parameterName_ExternalEmissivity = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.ExternalEmissivity.Name(), 2);
            string parameterName_InternalEmissivity = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.InternalEmissivity.Name(), 2);
            string parameterName_IgnoreThermalTransmittanceCalculations = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), OpaqueMaterialParameter.IgnoreThermalTransmittanceCalculations.Name(), 2);
            string parameterName_SolarTransmittance = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.SolarTransmittance.Name(), 2);
            string parameterName_LightTransmittance = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.LightTransmittance.Name(), 2);
            string parameterName_IsBlind = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), TransparentMaterialParameter.IsBlind.Name(), 2);
            string parameterName_HeatTransferCoefficient = mapCluster.GetName(typeof(Material), typeof(Autodesk.Revit.DB.FamilyInstance), GasMaterialParameter.HeatTransferCoefficient.Name(), 2);
            
            MaterialLibrary result = Create.MaterialLibrary(
                path,
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