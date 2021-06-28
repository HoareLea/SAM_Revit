using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural.Grasshopper.Revit
{
    public class RevitSAMArchitecturalByElement : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("bf6da822-29c6-4948-8844-949f960d2490");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Architectural;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitSAMArchitecturalByElement()
          : base("Revit.SAMArchitecturalByElement", "Revit.SAMArchitecturalByElement",
              "Convert Revit To SAM Architectural Object ie. BuildingElement (Wall, Floor etc.)",
              "SAM", "Revit")
        {
     
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_revitElement", "_revitElement", "Revit Element instance", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("ArchitecturalObject", "ArchitecturalObject", "SAM Architectural Object", GH_ParamAccess.list);
            outputParamManager.AddTextParameter("Report", "Report", "Report", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(1, ref run) || !run)
                return;

            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);
            IEnumerable<Core.ISAMObject> sAMObjects = null;
            string message = null;

            dynamic obj = objectWrapper.Value;
            if(obj is RhinoInside.Revit.GH.Types.ProjectDocument)
            {
                Document document = ((RhinoInside.Revit.GH.Types.ProjectDocument)obj).Value;
                List<IArchitecturalObject> architecturalObjects = Architectural.Revit.Convert.ToSAM(document, convertSettings);
                if (architecturalObjects != null)
                    sAMObjects = architecturalObjects.Cast<Core.ISAMObject>();

                if (sAMObjects == null || sAMObjects.Count() == 0)
                {
                    message = string.Format("Cannot convert Document.");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                    dataAccess.SetData(1, message);

                    return;
                }

                dataAccess.SetDataList(0, sAMObjects);

                message = string.Format("Document converted");
                dataAccess.SetData(1, message);

                return;
            }

            ElementId aId = obj.Id as ElementId;

            Element element = (obj.Document as Document).GetElement(aId);
            if (element == null)
            {
                message = "Invalid Element";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                dataAccess.SetData(1, message);

                return;
            }

            if (element is FamilyInstance && ((FamilyInstance)element).Symbol.Family.IsInPlace)
            {
                message = string.Format("Cannot convert In-Place family. ElementId: {0} ", element.Id.IntegerValue);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                dataAccess.SetData(1, message);

                return;
            }

            
            if (element is RevitLinkInstance)
            {
                List<IArchitecturalObject> architecturalObjects = Architectural.Revit.Convert.ToSAM((RevitLinkInstance)element, convertSettings);
                if (architecturalObjects != null)
                    sAMObjects = architecturalObjects.Cast<Core.ISAMObject>();
            }
            else
            {
                try
                {
                    sAMObjects = Architectural.Revit.Convert.ToSAM(element, convertSettings).Cast<Core.ISAMObject>();
                }
                catch (Exception exception)
                {
                    message = string.Format("Cannot convert Element. ElementId: {0} Category: {1} Exception: {2}", element.Id.IntegerValue, element.Category.Name, exception.Message);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                    dataAccess.SetData(1, message);
                }
            }

            if (sAMObjects == null || sAMObjects.Count() == 0)
            {
                message = string.Format("Cannot convert Element. ElementId: {0} Category: {1}", element.Id.IntegerValue, element.Category.Name);
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
                dataAccess.SetData(1, message);

                return;
            }

            dataAccess.SetDataList(0, sAMObjects);

            message = string.Format("Element converted. ElementId: {0} Category: {1}", element.Id.IntegerValue, element.Category.Name);
            dataAccess.SetData(1, message);
        }
    }
}