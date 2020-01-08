using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Core.Grasshopper.Revit.Properties;

namespace SAM.Core.Grasshopper.Revit
{
    public class TopologicFaces : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public TopologicFaces()
          : base("TopologicFaces", "ToTopoFaces",
              "Convert Topologic CellComplex To Topologic Faces",
              "SAM", "Revit")
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
            outputParamManager.AddGenericParameter("faces", "faces", "Topologic Faces", GH_ParamAccess.list);
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

            //CellComplex cellComplex = objectWrapper.Value as CellComplex;
            //if(cellComplex == null)
            //{
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
            //    return;
            //}

            //dataAccess.SetDataList(0, cellComplex.Faces);
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
            get { return new Guid("ba787b22-5355-4037-9d36-31c1dc8b385f"); }
        }
    }
}