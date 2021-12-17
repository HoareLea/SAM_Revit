using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreGetWalls : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8fc5308e-45c4-410c-ae04-5b3f2563ab9e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreGetWalls()
          : base("SAMCore.GetWalls", "SAMCore.GetWalls",
              "Query Walls from Revit Document by kinds: Basic Wall, Curtain Wall or Stacked Wall \n *Connect SAMCore.WallKind",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            Param_String param_String = new Param_String() { Optional = true};
            param_String.SetPersistentData(new string[] { WallKind.Basic.ToString() });
            inputParamManager.AddParameter(param_String, "_wallKinds_", "_wallKinds_", "_wallKinds_  \n *Connect SAMCore.WallKind", GH_ParamAccess.list);


            Param_Boolean param_Boolean = new Param_Boolean() { Optional = true};
            param_Boolean.SetPersistentData(false);
            inputParamManager.AddParameter(param_Boolean, "_inverted_", "_inverted_", "Inverted_", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Wall(), "Walls", "Walls", "Revit Walls", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<WallKind> wallKinds = null;

            List<string> wallKindNames = new List<string>();
            if (dataAccess.GetDataList(0, wallKindNames))
            {
                if (wallKindNames != null && wallKindNames.Count != 0)
                {
                    wallKinds = new List<WallKind>();
                    foreach (string viewTypeName in wallKindNames)
                        if (Enum.TryParse(viewTypeName, true, out WallKind wallKind))
                            wallKinds.Add(wallKind);
                }
            }
            else
            {
                wallKinds = new List<WallKind>() { WallKind.Basic };
            }

            bool inverted = false;
            dataAccess.GetData(1, ref inverted);

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            List<Wall> walls = new FilteredElementCollector(document).OfClass(typeof(Wall)).Cast<Wall>().ToList();
            if (wallKinds != null && wallKinds.Count != 0)
            {
                if (inverted)
                    walls = walls?.FindAll(x => !wallKinds.Contains((WallKind)(int)x.WallType.Kind));
                else
                    walls = walls?.FindAll(x => wallKinds.Contains((WallKind)(int)x.WallType.Kind));
            }

            dataAccess.SetDataList(0, walls);
        }
    }
}