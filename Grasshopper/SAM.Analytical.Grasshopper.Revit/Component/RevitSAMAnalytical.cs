using System;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using Autodesk.Revit.DB;

using SAM.Analytical.Grasshopper.Revit.Properties;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitSAMAnalytical : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("9b809657-8de3-466e-b814-973b0677a37a");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitSAMAnalytical()
          : base("Revit.SAMAnalytical", "Revit.SAMAnalytical",
              "Convert Revit To SAM Analytical Object ie. Panel, Space",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_revitElement", "_revitElement", "Revit Element instance", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
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

            Element element = document.GetElement(aId);
            if(element == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Element");
                return;
            }

            if(element is FamilyInstance && ((FamilyInstance)element).Symbol.Family.IsInPlace)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Cannot convert In-Place family. ElementId: {0} ", element.Id.IntegerValue));
                return;
            }

            IEnumerable<Core.ISAMObject> sAMObjects = Analytical.Revit.Convert.ToSAM(element);
            if(sAMObjects == null || sAMObjects.Count() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Cannot convert Element. ElementId: {0} ", element.Id.IntegerValue));
                return;
            }

            dataAccess.SetDataList(0, sAMObjects);
        }
    }
}