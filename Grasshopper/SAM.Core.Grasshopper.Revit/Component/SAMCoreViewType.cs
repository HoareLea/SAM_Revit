using SAM.Core.Grasshopper.Revit.Properties;
using System;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreViewType : GH_SAMEnumComponent<ViewType>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("c54e1b39-0282-4865-a0c5-bdd88f600303");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Panel Type
        /// </summary>
        public SAMCoreViewType()
          : base("SAMCore.ViewType", "SAM.Core.ViewType",
              "Revit View Type",
              "SAM", "Revit")
        {
        }
    }
}
