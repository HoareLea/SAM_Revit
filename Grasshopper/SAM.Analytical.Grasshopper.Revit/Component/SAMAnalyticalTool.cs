using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using System;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalTool : GH_SAMEnumComponent<Analytical.Revit.Tool>
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("fe588099-76ef-411d-8b49-3a0392bd074f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Panel Type Component
        /// </summary>
        public SAMAnalyticalTool()
          : base("SAMAnalytical.Tool", "SAMAnalytical.Tool",
              "Analytical Tool",
              "SAM", "Analytical")
        {
        }
    }
}