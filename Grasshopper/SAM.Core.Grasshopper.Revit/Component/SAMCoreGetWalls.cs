// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreGetWalls : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8fc5308e-45c4-410c-ae04-5b3f2563ab9e");

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
        public SAMCoreGetWalls()
          : base("SAMCore.GetWalls", "SAMCore.GetWalls",
              "Query Walls from Revit Document by kinds: Basic Wall, Curtain Wall or Stacked Wall \n *Connect SAMCore.WallKind",
              "SAM", "Revit")
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

                Param_String param_String = new Param_String() { Name = "_wallKinds_", NickName = "_wallKinds_", Description = "_wallKinds_  \n *Connect SAMCore.WallKind", Access = GH_ParamAccess.list, Optional = true };
                param_String.SetPersistentData(new string[] { WallKind.Basic.ToString() });
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));

                Param_Boolean param_Boolean = new Param_Boolean() { Name = "_inverted_", NickName = "_inverted_", Description = "Inverted_", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new RhinoInside.Revit.GH.Parameters.Wall() { Name = "Walls", NickName = "Walls", Description = "Revit Walls", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            List<WallKind> wallKinds = null;

            int index = Params.IndexOfInputParam("_wallKinds_");
            List<string> wallKindNames = new List<string>();
            if (index != -1 && dataAccess.GetDataList(index, wallKindNames))
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

            index = Params.IndexOfInputParam("_inverted_");
            bool inverted = false;
            if (index != -1)
                dataAccess.GetData(index, ref inverted);

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            List<Wall> walls = new FilteredElementCollector(document).OfClass(typeof(Wall)).Cast<Wall>().ToList();
            if (wallKinds != null && wallKinds.Count != 0)
            {
                if (inverted)
                    walls = walls?.FindAll(x => !wallKinds.Contains((WallKind)(int)x.WallType.Kind));
                else
                    walls = walls?.FindAll(x => wallKinds.Contains((WallKind)(int)x.WallType.Kind));
            }

            index = Params.IndexOfOutputParam("Walls");
            if (index != -1)
                dataAccess.SetDataList(index, walls);
        }
    }
}
