using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreGetLocation : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("348836ad-6a12-48fc-91ad-f939a8ebda6a");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreGetLocation()
          : base("SAMCore.GetLocation", "SAMCore.GetLocation",
              "Query Gets Location from Revit Document",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            //inputParamManager.AddGenericParameter("_sAMObjects", "_sAMObjects", "SAM Objects", GH_ParamAccess.list);
            //inputParamManager.AddGenericParameter("_elementIds", "_elementIds", "ElementIds", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooLocationParam(), "Location", "Location", "SAM Location", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            dataAccess.SetData(0, new GooLocation(document.Location()));
        }
    }
}