using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Architectural.Grasshopper.Revit
{
    public class SAMArchitecturalLevelInformation : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b72abb0f-6672-41e8-987b-d63fe9eaf7b9");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Architectural;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMArchitecturalLevelInformation()
          : base("SAMArchitectural.LevelInformation", "SAMCore.LevelInformation",
              "Query Level Information",
              "SAM", "Architectural")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("Level", "L", "New Level", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("HighLevel", "High Level", "Revit High Level", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("HighElevation", "HighElevation", "High Elevation", GH_ParamAccess.item);

            outputParamManager.AddNumberParameter("Elevation", "Elevation", "SAM Architectural Level Elevation", GH_ParamAccess.item);

            outputParamManager.AddGenericParameter("LowLevel", "LowLevel", "Revit Low Level", GH_ParamAccess.item);
            outputParamManager.AddNumberParameter("LowElevation", "LowElevation", "Low Elevation", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Autodesk.Revit.DB.Level level = null;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot access Revit Document");
                return;
            }

            dynamic obj = objectWrapper.Value;
            if (obj is GH_Integer)
            {
                ElementId elementId = new ElementId(((GH_Integer)obj).Value);
                if (elementId == null || elementId == ElementId.InvalidElementId)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot access Element");
                    return;
                }
                level = document.GetElement(elementId) as Autodesk.Revit.DB.Level;
            }
            else if (obj.GetType().GetProperty("Id") != null)
            {
                ElementId aId = obj.Id as ElementId;
                level = document.GetElement(aId) as Autodesk.Revit.DB.Level;
            }

            if (level == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot access Level");
                return;
            }

            if (level == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Autodesk.Revit.DB.Level level_High = Core.Revit.Query.HighLevel(level);
            double elevation_High = double.NaN;
            if (level_High != null)
                elevation_High = UnitUtils.ConvertFromInternalUnits(level_High.Elevation, DisplayUnitType.DUT_METERS);

            Autodesk.Revit.DB.Level level_Low = Core.Revit.Query.LowLevel(level);
            double elevation_Low = double.NaN;
            if (level_Low != null)
                elevation_Low = UnitUtils.ConvertFromInternalUnits(level_Low.Elevation, DisplayUnitType.DUT_METERS);

            dataAccess.SetData(0, level_High);
            dataAccess.SetData(1, new GH_Number(elevation_High));
            dataAccess.SetData(2, new GH_Number(UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS)));
            dataAccess.SetData(3, level_Low);
            dataAccess.SetData(4, new GH_Number(elevation_Low));
        }
    }
}