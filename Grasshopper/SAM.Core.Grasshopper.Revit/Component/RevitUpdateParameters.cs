using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class RevitUpdateParameters : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f26231fc-2db8-4927-ad62-1a6a2bd813a7");

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
        public RevitUpdateParameters()
          : base("Revit.UpdateParameters", "Revit.UpdateParameters",
              "Updates Revit Element Parameter based on SAMObject Parameter",
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
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_sAMObject", NickName = "_sAMObject", Description = "SAM Object", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_names", NickName = "_names", Description = "Names", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
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

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_sAMObject");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
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
            index = Params.IndexOfInputParam("_names");
            if (index == -1 || !dataAccess.GetDataList(index, names) || names == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            StartTransaction(document);

            Core.Revit.Modify.SetValues(element, sAMObject, names, null);

            index = Params.IndexOfOutputParam("element");
            if (index != -1)
                dataAccess.SetData(0, element);

            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, true);
        }
    }
}