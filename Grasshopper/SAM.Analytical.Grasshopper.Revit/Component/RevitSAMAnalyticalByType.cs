using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitLinkSAMAnalyticalByType : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("983b8384-71c3-4243-b93b-e63400311864");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitLinkSAMAnalyticalByType()
          : base("RevitLink.SAMAnalyticalByType", "Revit.SAMAnalyticalByType",
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

            inputParamManager.AddTextParameter("_type_", "_type_", "Type Name ie. Panel, Construction, Aperture, ApertureConstruction, Space", GH_ParamAccess.item, "Panel");

            index = inputParamManager.AddGenericParameter("_revitLinkInstance", "_revitLinkInstance", "RevitLinkInstance", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("AnalyticalObjects", "AnalyticalObjects", "SAM Analytical Objects", GH_ParamAccess.list);
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

            Transform transform = Transform.Identity;

            GH_ObjectWrapper objectWrapper = null;

            dataAccess.GetData(1, ref objectWrapper);
            if (objectWrapper != null)
            {
                dynamic obj = objectWrapper.Value;

                ElementId aId = obj.Id as ElementId;

                Element element = (obj.Document as Document).GetElement(aId);
                if (element == null || !(element is RevitLinkInstance))
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Element");
                    return;
                }

                RevitLinkInstance revitLinkInstance = element as RevitLinkInstance;

                document = revitLinkInstance.GetLinkDocument();
                transform = revitLinkInstance.GetTotalTransform();
            }

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            IEnumerable<Core.SAMObject> result = Analytical.Revit.Convert.ToSAM(document, type, convertSettings, transform);

            dataAccess.SetDataList(0, result);
        }
    }
}