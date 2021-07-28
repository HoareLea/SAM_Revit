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
    public class SAMArchitecturalLevelDispatchExisting : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b083ef49-1dec-4497-ac65-c4f63659ed83");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

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
        protected override ParamDefinition[] Inputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new GooLevelParam() { Name = "_levels", NickName = "_levels", Description = "SAM Architectural Levels", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override ParamDefinition[] Outputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new GooLevelParam() { Name = "new", NickName = "new", Description = "New Levels that do not exisit in Revit model", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new GooLevelParam() { Name = "existing", NickName = "exisiting", Description = "Existing Levels in Revit model", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            
            List<Level> levels_SAM = new List<Level>();
            index = Params.IndexOfInputParam("_levels");
            if (index == -1 || !dataAccess.GetDataList(index, levels_SAM))
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            int index_New = Params.IndexOfOutputParam("new");
            int index_Existing = Params.IndexOfOutputParam("existing");

            List<Autodesk.Revit.DB.Level> levels_Revit = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Level)).Cast<Autodesk.Revit.DB.Level>().ToList();
            if (levels_Revit == null || levels_Revit.Count() == 0)
            {
                if (index_New != -1)
                    dataAccess.SetDataList(index_New, null);

                if (index_Existing != -1)
                    dataAccess.SetDataList(index_Existing, null);
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

            if (index_New != -1)
                dataAccess.SetDataList(index_New, gooLevels_New);

            if (index_Existing != -1)
                dataAccess.SetDataList(index_Existing, gooLevels_Existing);
        }
    }
}