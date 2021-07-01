using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class RevitSAMCoreDesignOption : GH_SAMVariableOutputParameterComponent
    {       
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5c4f8938-77c1-4d9c-8f19-d46ac1d6f70f");

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
        public RevitSAMCoreDesignOption()
          : base("Revit.SAMCoreDesignOption", "Revit.SAMCoreDesignOption",
              "Gets SAM Design Option from Revit Document",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "document_", NickName = "document_", Description = "Document", Access = GH_ParamAccess.item, Optional = true}, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_id", NickName = "_id", Description = "Id of Design Option (Name or ElementId)", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "designOption", NickName = "designOption", Description = "SAM DesignOption", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            GH_ObjectWrapper objectWrapper;

            Document document = null;

            objectWrapper = null;
            index = Params.IndexOfInputParam("document_");
            if (index != -1 && dataAccess.GetData(index, ref objectWrapper))
            {
                document = objectWrapper.Value as Document;
            }

            if (document == null)
            {
                document = RhinoInside.Revit.Revit.ActiveDBDocument;
            }

            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Autodesk.Revit.DB.DesignOption designOption = null;

            objectWrapper = null;
            index = Params.IndexOfInputParam("_id");
            if(index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfOutputParam("designOption");
            if(index != -1)
            {
                dataAccess.SetData(index, Core.Revit.Convert.ToSAM(designOption, new ConvertSettings(false, true, false)));
            }
        }
    }
}