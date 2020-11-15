using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using SAM.Geometry.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitLinkSAMAnalyticalByTypeName : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("983b8384-71c3-4243-b93b-e63400311864");

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
        public RevitLinkSAMAnalyticalByTypeName()
          : base("RevitLink.SAMAnalyticalByTypeName", "Revit.SAMAnalyticalByTypeName",
              "Convert Revit Link Instance To SAM Analytical Object ie. Panel, Construction, Aperture, ApertureConstruction, Space",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddTextParameter("_typeName_", "_typeName_", "Type Name ie. Panel, Construction, Aperture, ApertureConstruction, Space", GH_ParamAccess.item, "Panel");

            index = inputParamManager.AddGenericParameter("_revitLinkInstance", "_revitLinkInstance", "RevitLinkInstance", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("AnalyticalObject", "AnalyticalObject", "SAM Analytical Object", GH_ParamAccess.list);
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
            if (!dataAccess.GetData(2, ref run) || !run)
                return;

            string typeName = null;
            if (!dataAccess.GetData(0, ref typeName) || string.IsNullOrWhiteSpace(typeName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            typeName = typeName.Trim();
            
            Type type = Type.GetType(string.Format("{0},{1}", "SAM.Analytical." + typeName, "SAM.Analytical"));
            if (type == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string message = null;

            GH_ObjectWrapper objectWrapper = null;

            dataAccess.GetData(1, ref objectWrapper);
            if (objectWrapper != null)
            {
                dynamic obj = objectWrapper.Value;

                ElementId aId = obj.Id as ElementId;

                Element element = (obj.Document as Document).GetElement(aId);
                if (element == null || !(element is RevitLinkInstance))
                {
                    message = "Invalid Element";
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                    dataAccess.SetData(1, message);
                    return;
                }

                RevitLinkInstance revitLinkInstance = element as RevitLinkInstance;

                document = revitLinkInstance.GetLinkDocument();
            }

            FilteredElementCollector filteredElementCollector = Analytical.Revit.Query.FilteredElementCollector(document, type);
            if (filteredElementCollector == null)
            {
                message = "Could not create proper FilteredElementCollector";
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, message);
                dataAccess.SetData(1, message);
                return;
            }

            List<Core.SAMObject> sAMObjects = new List<Core.SAMObject>();

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            List<Element> elementList = filteredElementCollector.ToList();
            foreach (Element element in elementList)
            {
                IEnumerable<Core.SAMObject> sAMObjects_Temp = Analytical.Revit.Convert.ToSAM(element, convertSettings);
                if (sAMObjects_Temp != null && sAMObjects_Temp.Count() > 0)
                {
                    sAMObjects.AddRange(sAMObjects_Temp);
                    message += "\n" + string.Format("Element converted. ElementId: {0} Category: {1}", element.Id.IntegerValue, element.Category.Name);
                }
                else
                {
                    message += "\n" + string.Format("Element not converted. ElementId: {0} Category: {1}", element.Id.IntegerValue, element.Category.Name);
                }
            }

            dataAccess.SetDataList(0, sAMObjects);
            dataAccess.SetData(1, message);
        }
    }
}