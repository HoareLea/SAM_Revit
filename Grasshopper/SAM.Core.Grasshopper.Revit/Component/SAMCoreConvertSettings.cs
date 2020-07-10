using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using System;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreCreateConvertSettings : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("da750879-e8e0-446d-ab7d-a705358ce304");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreCreateConvertSettings()
          : base("SAMCore.CreateConvertSettings", "SAMCore.CreateConvertSettings",
              "Create SAM Core ConvertSettings",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddBooleanParameter("_convertGeometry_", "_convertGeometry_", "Convert Geometry", GH_ParamAccess.item, true);
            inputParamManager.AddBooleanParameter("_convertParameters_", "_convertParameters_", "Convert Parameters", GH_ParamAccess.item, true);
            inputParamManager.AddBooleanParameter("_removeExisting_", "_removeExisting_", "Remove existing Revit element if exists before conversion ", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooConvertSettingsParam(), "ConvertSettings", "ConvertSettings", "SAM Core Convert Settings", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool convertGeometry = true;
            if (!dataAccess.GetData(0, ref convertGeometry))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool convertParameters = true;
            if (!dataAccess.GetData(1, ref convertParameters))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool removeExisting = true;
            if (!dataAccess.GetData(2, ref removeExisting))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, new GooConvertSettings(new Core.Revit.ConvertSettings(convertGeometry, convertParameters, removeExisting)));
        }
    }
}