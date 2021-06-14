using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalDuplicateConstruction : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7af06904-abe4-4b64-957a-28adfad8a124");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.4";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        private bool run = false;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalDuplicateConstruction()
          : base("SAMAnalytical.DuplicateConstruction", "SAManalytical.DuplicateConstruction",
              "Modify Duplicate Analytical Object",
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
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_name", NickName = "_name", Description = "Name", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "inverted_", NickName = "inverted_", Description = "If inverted then name is source type and panel construction is destination type", Optional = true, Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(true);
                result.Add(ParamDefinition.FromParam(param_Boolean, ParamVisibility.Binding));

                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_parameterNames_", NickName = "_parameterNames_", Description = "Parameter Names", Optional = true, Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(ParamDefinition.FromParam(param_Boolean, ParamVisibility.Binding));
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
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.ElementType() { Name = "elemenType", NickName = "elemenType", Description = "Revit ElemenType", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            
            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string name = null;
            index = Params.IndexOfInputParam("_name");
            if (index == -1 || !dataAccess.GetData(index, ref name) || string.IsNullOrWhiteSpace(name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool inverted = false;
            index = Params.IndexOfInputParam("inverted_");
            if (index == -1 || !dataAccess.GetData(index, ref inverted))
                inverted = false;

            List<string> parameterNames = new List<string>();
            index = Params.IndexOfInputParam("_parameterNames_");
            if (index == -1 || !dataAccess.GetDataList(index, parameterNames) || parameterNames.Count == 0)
                parameterNames = null;

            Core.SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            StartTransaction(document);

            ElementType elementType = null;

            if(sAMObject is Panel)
            {
                Panel panel = (Panel)sAMObject;

                if (inverted)
                    elementType = Analytical.Revit.Modify.DuplicateByName(document, name, panel.PanelType, panel.Construction, parameterNames);
                else
                    elementType = Analytical.Revit.Modify.DuplicateByName(document, panel.Construction, panel.PanelType, name, parameterNames);
            }
            else if(sAMObject is ApertureConstruction)
            {
                ApertureConstruction apertureConstruction = (ApertureConstruction)sAMObject;

                if (inverted)
                    elementType = Analytical.Revit.Modify.DuplicateByName(document, name, apertureConstruction, parameterNames);
                else
                    elementType = Analytical.Revit.Modify.DuplicateByName(document, apertureConstruction, name, parameterNames);
            }
            else if (sAMObject is Construction)
            {
                Construction construction = (Construction)sAMObject;

                if(construction.TryGetValue(ConstructionParameter.DefaultPanelType, out string panelTypeString))
                {
                    PanelType panelType = Analytical.Query.PanelType(panelTypeString);
                    if(panelType != PanelType.Undefined)
                    {
                        if (inverted)
                            elementType = Analytical.Revit.Modify.DuplicateByName(document, name, panelType, construction, parameterNames);
                        else
                            elementType = Analytical.Revit.Modify.DuplicateByName(document, construction, panelType, name, parameterNames);
                    }
                }
            }

            index = Params.IndexOfOutputParam("elemenType");
            if (index != -1)
                dataAccess.SetData(index, elementType);
        }
    }
}