using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper.Kernel;

using Autodesk.Revit.DB;

using SAM.Analytical.Grasshopper.Revit.Properties;


namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalRevit : RhinoInside.Revit.GH.Components.ReconstructElementComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("222e0a64-9514-47d3-98ac-72e95124841d");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        private List<Wall> walls = new List<Wall>();

        private static readonly FailureDefinitionId[] failureDefinitionIdsToFix = new FailureDefinitionId[]
        {
            BuiltInFailures.CreationFailures.CannotDrawWallsError,
            BuiltInFailures.JoinElementsFailures.CannotJoinElementsError,
        };

        protected override IEnumerable<FailureDefinitionId> FailureDefinitionIdsToFix => failureDefinitionIdsToFix;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRevit()
          : base("SAMAnalytical.Revit", "SAManalytical.Revit",
              "Create Revit HostObject from SAM Analytical ie. Panel",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.HostObject(), "HostObjects", "hso" ,"HostObjects", GH_ParamAccess.item);
        }

        protected override void OnAfterStart(Document document, string strTransactionName)
        {
            base.OnAfterStart(document, strTransactionName);

            // Disable all previous walls joins
            if (PreviousStructure is object)
            {
                var unjoinedWalls = PreviousStructure.OfType<RhinoInside.Revit.GH.Types.Element>().
                                    Select(x => document.GetElement(x)).
                                    OfType<Wall>().
                                    Where(x => x.Pinned).
                                    Select
                                    (
                                      x => Tuple.Create
                                      (
                                        x,
                                        (x.Location as LocationCurve).get_JoinType(0),
                                        (x.Location as LocationCurve).get_JoinType(1)
                                      )
                                    ).
                                    ToArray();

                foreach (var unjoinedWall in unjoinedWalls)
                {
                    var location = unjoinedWall.Item1.Location as LocationCurve;
                    if (WallUtils.IsWallJoinAllowedAtEnd(unjoinedWall.Item1, 0))
                    {
                        WallUtils.DisallowWallJoinAtEnd(unjoinedWall.Item1, 0);
                        WallUtils.AllowWallJoinAtEnd(unjoinedWall.Item1, 0);
                        location.set_JoinType(0, unjoinedWall.Item2);
                    }

                    if (WallUtils.IsWallJoinAllowedAtEnd(unjoinedWall.Item1, 1))
                    {
                        WallUtils.DisallowWallJoinAtEnd(unjoinedWall.Item1, 1);
                        WallUtils.AllowWallJoinAtEnd(unjoinedWall.Item1, 1);
                        location.set_JoinType(1, unjoinedWall.Item3);
                    }
                }
            }
        }

        protected override void OnBeforeCommit(Document document, string strTransactionName)
        {
            base.OnBeforeCommit(document, strTransactionName);

            // Reenable new joined walls
            foreach (var wallToJoin in walls)
            {
                WallUtils.AllowWallJoinAtEnd(wallToJoin, 0);
                WallUtils.AllowWallJoinAtEnd(wallToJoin, 1);
            }

            walls = new List<Wall>();
        }

        public override void OnCommitted(Document document, string strTransactionName)
        {
            base.OnCommitted(document, strTransactionName);
        }

        private void ReconstructSAMAnalyticalRevit(Document document, ref HostObject hostObject, Panel panel, bool _run = false)
        {
            if (!_run)
                return;
            
            HostObject hostObject_New = Analytical.Revit.Convert.ToRevit(document, panel);

            if (hostObject != null)
            {
                BuiltInParameter[] builtInParameters = new BuiltInParameter[]
                {
                    BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
                    BuiltInParameter.ELEM_FAMILY_PARAM,
                    BuiltInParameter.ELEM_TYPE_PARAM,
                    BuiltInParameter.WALL_BASE_CONSTRAINT,
                    BuiltInParameter.WALL_USER_HEIGHT_PARAM,
                    BuiltInParameter.WALL_BASE_OFFSET,
                    BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT,
                    BuiltInParameter.WALL_KEY_REF_PARAM
                 };

                ReplaceElement(ref hostObject, hostObject_New, builtInParameters);
            }

            if (hostObject_New is Wall)
                walls.Add((Wall)hostObject_New);

            hostObject = hostObject_New;
        }
    }
}