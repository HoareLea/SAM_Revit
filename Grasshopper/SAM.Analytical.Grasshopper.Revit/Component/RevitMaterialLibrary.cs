using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitMaterialLibrary : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6c4f99f6-5f52-491d-823e-171a66c0d40c");

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
        public RevitMaterialLibrary()
          : base("Revit.MaterialLibrary", "Revit.MaterialLibrary",
              "Gets SAM Material Library",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Document(), "_document_", "_document_", "Document", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooMaterialLibraryParam(), "MaterialLibrary", "MaterialLibrary", "SAM MaterialLibrary", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Document document = null;
            dataAccess.GetData(0, ref document);

            if(document == null)
                document = RhinoInside.Revit.Revit.ActiveDBDocument;

            IEnumerable<Autodesk.Revit.DB.Material> materials = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Material)).Cast<Autodesk.Revit.DB.Material>();

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            MaterialLibrary result = new MaterialLibrary(document.PathName);
            foreach (Autodesk.Revit.DB.Material material in materials)
                result.Add(Analytical.Revit.Convert.ToSAM(material, convertSettings));


            dataAccess.SetData(0, new GooMaterialLibrary(result));
        }
    }
}