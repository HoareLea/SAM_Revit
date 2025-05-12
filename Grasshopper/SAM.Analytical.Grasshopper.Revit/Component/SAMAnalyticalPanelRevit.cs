using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalPanelRevit : SAMReconstructElementComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("222e0a64-9514-47d3-98ac-72e95124841d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        private List<Autodesk.Revit.DB.Wall> walls = new List<Autodesk.Revit.DB.Wall>();
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
        public SAMAnalyticalPanelRevit()
          : base("SAMAnalytical.PanelRevit", "SAMAnalytical.PanelRevit",
              "Convert Revit HostObject from SAM Analytical Panel",
              "SAM", "Revit")
        {
        }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021|| Revit2022|| Revit2023
        public override void OnStarted(Document document)
#else
        protected override void OnStarted(Document document)
#endif
        {
            base.OnStarted(document);

            // Disable all previous walls joins
            //IEnumerable<Wall> walls = Params.TrackedElements<Wall>("Wall", document);
            //var pinnedWalls = walls.Where(x => x.Pinned).
            //                  Select
            //                  (
            //                    wall =>
            //                    (
            //                      wall,
            //                      (wall.Location as LocationCurve).get_JoinType(0),
            //                      (wall.Location as LocationCurve).get_JoinType(1)
            //                    )
            //                  );

            //foreach (var (wall, joinType0, joinType1) in pinnedWalls)
            //{
            //    var location = wall.Location as LocationCurve;

            //    if (WallUtils.IsWallJoinAllowedAtEnd(wall, 0))
            //    {
            //        WallUtils.DisallowWallJoinAtEnd(wall, 0);
            //        WallUtils.AllowWallJoinAtEnd(wall, 0);
            //        location.set_JoinType(0, joinType0);
            //    }

            //    if (WallUtils.IsWallJoinAllowedAtEnd(wall, 1))
            //    {
            //        WallUtils.DisallowWallJoinAtEnd(wall, 1);
            //        WallUtils.AllowWallJoinAtEnd(wall, 1);
            //        location.set_JoinType(1, joinType1);
            //    }
            //}
        }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021|| Revit2022|| Revit2023
        public override void OnPrepare(IReadOnlyCollection<Document> documents)
#else
        protected override void OnPrepare(IReadOnlyCollection<Document> documents)
#endif
        {
            if (!run)
                return;

            base.OnPrepare(documents);

            // Reenable new joined walls
            foreach (var wallToJoin in walls)
            {
                WallUtils.AllowWallJoinAtEnd(wallToJoin, 0);
                WallUtils.AllowWallJoinAtEnd(wallToJoin, 1);
            }

            walls = new List<Autodesk.Revit.DB.Wall>();
        }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021|| Revit2022|| Revit2023
        public override void OnDone(TransactionStatus status)
#else
        protected override void OnDone(TransactionStatus status)
#endif
        {
            if (!run)
                return;

            base.OnDone(status);
        }

        private void ReconstructSAMAnalyticalPanelRevit(Document document, ref HostObject hostObject, Panel panel, ConvertSettings convertSettings = null, bool _run = false)
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
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
                string message = string.Format("Revit Element Type is missing - Panel Guid: {0} Construction Name: {1}, Revit Element Id: {2}", panel.Guid, construction.Name, hostObject_New.Id.IntegerValue);
#else
                string message = string.Format("Revit Element Type is missing - Panel Guid: {0} Construction Name: {1}, Revit Element Id: {2}", panel.Guid, construction.Name, hostObject_New.Id.Value);
#endif

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

            if (hostObject_New is Autodesk.Revit.DB.Wall)
                walls.Add((Autodesk.Revit.DB.Wall)hostObject_New);

            hostObject = hostObject_New;
        }
    }
}