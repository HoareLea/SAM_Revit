using SAM.Core.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreWallKind : GH_SAMEnumComponent<WallKind>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("53ad4c77-28be-47fa-8c9c-c2246ff05eef");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMCoreWallKind()
          : base("SAMCore.WallKind", "SAM.Core.WallKind",
              "Revit Wall Kind",
              "SAM", "Revit")
        {
        }
    }
}
