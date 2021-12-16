using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalUpdateParameters : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a4772fdd-828c-4d2e-8cc9-3b446eb17563");

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
        public SAMAnalyticalUpdateParameters()
          : base("SAMAnalytical.UpdateParameters", "SAMAnalytical.UpdateParameters",
              "Updates Revit Element Parameter based on Analytical Object",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override ParamDefinition[] Inputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override ParamDefinition[] Outputs
        {
            get
            {
                List<ParamDefinition> result = new List<ParamDefinition>();
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Element() { Name = "element", NickName = "element", Description = "Revit Element", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Parameters Updated", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            int index_Successful = -1;

            index_Successful = Params.IndexOfOutputParam("successful");
            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, false);

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            IAnalyticalObject analyticalObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Core.SAMObject sAMObject = analyticalObject as Core.SAMObject;
            if (sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ElementId elementId = ((Core.SAMObject)analyticalObject).ElementId();
            if (elementId == null || elementId == ElementId.InvalidElementId)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cound not find matching Revit Element");
                return;
            }

            if (elementId == null && analyticalObject is Space)
            {
                List<Autodesk.Revit.DB.Mechanical.Space> spaces = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();
                if (spaces != null)
                {
                    Autodesk.Revit.DB.Mechanical.Space space = spaces.Find(x => x.Name != null && x.Name.Equals(((Space)analyticalObject).Name));
                    if (space != null)
                    {
                        elementId = space.Id;
                    }
                }
            }

            Element element = document.GetElement(elementId);
            if (element == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cound not find matching Revit Element");
                return;
            }

            StartTransaction(document);

            Core.Revit.Modify.SetValues(element, sAMObject);

            Core.Revit.Modify.SetValues(element, sAMObject, ActiveSetting.Setting);

            index = Params.IndexOfOutputParam("element");
            if (index != -1)
                dataAccess.SetData(0, element);

            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, true);
        }
    }
}