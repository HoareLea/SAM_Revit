using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class RevitUpdateParameters : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f26231fc-2db8-4927-ad62-1a6a2bd813a7");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitUpdateParameters()
          : base("Revit.UpdateParameters", "Revit.UpdateParameters",
              "Updates Revit Element Parameter based on SAMObject Parameter",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_sAMObject", "_sAMObject", "SAM Object", GH_ParamAccess.item);
            inputParamManager.AddTextParameter("_names", "_names", "Names", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Element(), "Element", "Element", "Revit Element", GH_ParamAccess.item);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Parameters Updated", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(1, false);

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ElementId elementId = sAMObject.ElementId();
            if (elementId == null || elementId == ElementId.InvalidElementId)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cound not find matching Revit Element");
                return;
            }


            Element element = document.GetElement(elementId);
            if (element == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cound not find matching Revit Element");
                return;
            }


            List<string> names = new List<string>();
            if (!dataAccess.GetDataList(1, names) || names == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Core.Revit.Modify.SetValues(element, sAMObject, names, null);

            dataAccess.SetData(0, element);
            dataAccess.SetData(1, true);
        }
    }
}