using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMPanelRevit : SAMReconstructElementComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("222e0a64-9514-47d3-98ac-72e95124841d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        private List<Wall> walls = new List<Wall>();
        private bool run = false;

        private static readonly FailureDefinitionId[] failureDefinitionIdsToFix = new FailureDefinitionId[]
        {
            BuiltInFailures.CreationFailures.CannotDrawWallsError,
            BuiltInFailures.JoinElementsFailures.CannotJoinElementsError,
        };

        protected override IEnumerable<FailureDefinitionId> FailureDefinitionIdsToFix => failureDefinitionIdsToFix;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMPanelRevit()
          : base("SAMPanel.Revit", "SAMPanel.Revit",
              "Convert Revit HostObject from SAM Analytical Panel",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.HostObject(), "HostObjects", "hso", "HostObjects", GH_ParamAccess.item);
        }

        protected override void OnAfterStart(Document document, string strTransactionName)
        {
            if (!run)
                return;

            base.OnAfterStart(document, strTransactionName);

            // Disable all previous walls joins
            if (PreviousStructure is object)
            {
                var unjoinedWalls = PreviousStructure.OfType<RhinoInside.Revit.GH.Types.Element>().
                                    Select(x => x.Value).
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
            if (!run)
                return;

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
            if (!run)
                return;

            base.OnCommitted(document, strTransactionName);
        }

        private void ReconstructSAMPanelRevit(Document document, ref HostObject hostObject, Panel panel, ConvertSettings convertSettings = null, bool _run = false)
        {
            run = _run;

            if (!run)
                return;

            if (panel == null)
                return;

            convertSettings = this.UpdateSolutionEndEventHandler(convertSettings);

            string name = panel.Name;
            Guid guid = panel.Guid;

            if (!convertSettings.ConvertParameters && !convertSettings.ConvertGeometry)
            {
                string message = string.Format("Invalid Convert Settings");
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
            }

            Core.Revit.Modify.RemoveExisting(convertSettings, document, panel);

            HostObject hostObject_New = null;

            try
            {
                hostObject_New = Analytical.Revit.Convert.ToRevit(panel, document, convertSettings);
            }
            catch (Exception exception)
            {
                string message = string.Format("Cannot convert element. Panel Name: {0}, Panel Guid: {1}, Exception Message: {2}", name, guid, exception.Message);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
            }

            if (hostObject_New == null)
                return;

            Construction construction = panel.Construction;
            if (construction != null && !string.IsNullOrWhiteSpace(construction.Name) && (!(construction.Name.Equals(hostObject_New.FullName()) || construction.Name.Equals(hostObject_New.Name))))
            {
                string message = string.Format("Revit Element Type is missing - Panel Guid: {0} Construction Name: {1}, Revit Element Id: {2}", panel.Guid, construction.Name, hostObject_New.Id.IntegerValue);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
            }

            //if (hostObject != null)
            //{
            //    BuiltInParameter[] builtInParameters = new BuiltInParameter[]
            //    {
            //        BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
            //        BuiltInParameter.ELEM_FAMILY_PARAM,
            //        BuiltInParameter.ELEM_TYPE_PARAM,
            //        BuiltInParameter.WALL_BASE_CONSTRAINT,
            //        BuiltInParameter.WALL_USER_HEIGHT_PARAM,
            //        BuiltInParameter.WALL_BASE_OFFSET,
            //        BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT,
            //        BuiltInParameter.WALL_KEY_REF_PARAM
            //     };

            // ReplaceElement(ref hostObject, hostObject_New, builtInParameters);

            //    if (Core.Revit.Query.FullName(hostObject).Equals(panel.Construction.Name))
            //        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Defult type used for panel {0}", panel.Guid));
            //}

            if (hostObject_New is Wall)
                walls.Add((Wall)hostObject_New);

            hostObject = hostObject_New;
        }
    }
}