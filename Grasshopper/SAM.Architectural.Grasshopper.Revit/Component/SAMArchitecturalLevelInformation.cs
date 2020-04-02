using System;

using Autodesk.Revit.DB;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Core.Grasshopper.Revit.Properties;


namespace SAM.Core.Grasshopper.Revit
{
    public class SAMArchitecturalLevelInformation : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b72abb0f-6672-41e8-987b-d63fe9eaf7b9");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMArchitecturalLevelInformation()
          : base("SAMArchitectural.LevelInformation", "SAMCore.LevelInformation",
              "Level Information",
              "SAM", "Architectural")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Level(), "Level", "L", "New Level", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Level(), "HighLevel", "High Level", "Revit High Level", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("HighElevation", "HighElevation", "High Elevation", GH_ParamAccess.item);

            outputParamManager.AddNumberParameter("Elevation", "Elevation", "SAM Architectural Level Elevation", GH_ParamAccess.item);

            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Level(), "LowLevel", "LowLevel", "Revit Low Level", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("LowElevation", "LowElevation", "Low Elevation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dynamic obj = objectWrapper.Value;

            ElementId aId = obj.Id as ElementId;
            Document document = obj.Document as Document;

            Level level = document.GetElement(aId) as Level;

            if(level == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Level level_High = Core.Revit.Query.HighLevel(level);
            double elevation_High = double.NaN;
            if (level_High != null)
                elevation_High = UnitUtils.ConvertFromInternalUnits(level_High.Elevation, DisplayUnitType.DUT_METERS);


            Level level_Low = Core.Revit.Query.LowLevel(level);
            double elevation_Low = double.NaN;
            if (level_Low != null)
                elevation_Low = UnitUtils.ConvertFromInternalUnits(level_Low.Elevation, DisplayUnitType.DUT_METERS);
            
            dataAccess.SetData(0, level_High);
            dataAccess.SetData(1, elevation_High);
            dataAccess.SetData(2, UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS));
            dataAccess.SetData(3, level_Low);
            dataAccess.SetData(4, elevation_Low);
        }
    }
}