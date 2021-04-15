using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreIsPlaced : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("b6256752-1d68-42ef-b6db-ca4802471ef6");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreIsPlaced()
          : base("SAMCore.IsPlaced", "SAMCore.IsPlaced",
              "Check if given Type is placed",
              "SAM", "Revit")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "type_", NickName = "type_", Description = "Typet", Access = GH_ParamAccess.item, Optional = false }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new RhinoInside.Revit.GH.Parameters.Element() { Name = "_revitLinkInstance_", NickName = "_revitLinkInstance_", Description = "Revit Link Instance", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "isPlaced", NickName = "isPlaced", Description = "IsPlaced", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "count", NickName = "count", Description = "Count", Access = GH_ParamAccess.item }, ParamVisibility.Voluntary));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("type_");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_revitLinkInstance_");
            if (index != -1)
                dataAccess.GetData(index, ref objectWrapper);

            Document document = null;
            if(objectWrapper != null && objectWrapper.TryGetElement(out RevitLinkInstance revitLinkInstance) && revitLinkInstance != null)
            {
                document = revitLinkInstance.GetLinkDocument();
            }

            if(document == null)
            {
                document = RhinoInside.Revit.Revit.ActiveDBDocument;
            }

            objectWrapper = null;
            index = Params.IndexOfInputParam("type_");
            if (!dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(!objectWrapper.TryGetElement(out ElementType elementType, document) || elementType == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            int count = elementType.InstancesCount();

            index = Params.IndexOfOutputParam("isPlaced");
            if(index != -1)
            {
                dataAccess.SetData(index, count > 0);
            }

            index = Params.IndexOfOutputParam("count");
            if (index != -1)
            {
                dataAccess.SetData(index, count);
            }
        }
    }
}