using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Architectural.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural.Grasshopper.Revit
{
    public class SAMArchitecturalLevelDispatchExisting : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b083ef49-1dec-4497-ac65-c4f63659ed83");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Architectural;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMArchitecturalLevelDispatchExisting()
          : base("SAMArchitectural.LevelDispatchExisting", "SAMArchitectural.LevelDispatchExisting",
              "Dispatch/Filters existing Levels in Revit model based on Level elevation, help you chose which Levels are need to be created and missing in Revit",
              "SAM", "Architectural")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooLevelParam(), "_levels", "_levels", "SAM Architectural Levels", GH_ParamAccess.list);
                          }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooLevelParam(), "New", "New", "New Levels that do not exisit in Revit model", GH_ParamAccess.list);
            outputParamManager.AddParameter(new GooLevelParam(), "Existing", "Existing", "Existing Levels in Revit model", GH_ParamAccess.list);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            List<Level> levels_SAM = new List<Level>();
            if (!dataAccess.GetDataList(0, levels_SAM))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            List<Autodesk.Revit.DB.Level> levels_Revit = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Level)).Cast<Autodesk.Revit.DB.Level>().ToList();
            if(levels_Revit == null || levels_Revit.Count() == 0)
            {                
                dataAccess.SetDataList(0, null);
                dataAccess.SetDataList(1, null);
                return;
            }

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            List<GooLevel> gooLevels_New = new List<GooLevel>();
            List<GooLevel> gooLevels_Existing = new List<GooLevel>();
            foreach (Level level_SAM in levels_SAM)
            {
                Autodesk.Revit.DB.Level level_Revit = levels_Revit.Find(x => Math.Abs(Architectural.Revit.Query.Elevation(x) - level_SAM.Elevation) < Core.Tolerance.MacroDistance);
                if (level_Revit == null)
                    gooLevels_New.Add(new GooLevel(level_SAM));
                else
                    gooLevels_Existing.Add(new GooLevel(level_SAM));
            }

            dataAccess.SetDataList(0, gooLevels_New);
            dataAccess.SetDataList(1, gooLevels_Existing);
        }
    }
}