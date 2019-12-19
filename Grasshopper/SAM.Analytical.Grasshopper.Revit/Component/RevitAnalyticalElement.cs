using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Autodesk.Revit.DB;

using SAM.Analytical.Grasshopper.Revit.Properties;

namespace SAM.Analytical.Grasshopper.Topologic
{
    public class RevitAnalyticalElement : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitAnalyticalElement()
          : base("RevitAnalyticalElement", "ToTopoFaces",
              "Convert Topologic CellComplex To Topologic Faces",
              "SAM", "Topologic")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_cellComplex", "_cellComplex", "Topologic CellComplex", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Panel", "panel", "SAM Analytical Panel", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dynamic obj = objectWrapper.Value;

            Document document = obj.Document as Document; 

            ElementId aId = obj.Id as ElementId;

            HostObject hostObject = document.GetElement(aId) as HostObject;
            if(hostObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Element");
                return;
            }

            dataAccess.SetDataList(0, Analytical.Revit.Convert.ToSAM(hostObject));
            return;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SAM_Small;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d5b1cfe2-5951-4a42-a121-a476436cd867"); }
        }
    }
}