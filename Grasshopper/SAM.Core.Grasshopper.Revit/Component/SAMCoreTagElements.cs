using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMCoreTagElements : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a69e2526-e01f-4fd1-b517-bfc52f83db9d");

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
        public SAMCoreTagElements()
          : base("SAMCore.TagElements", "SAMCore.TagElements",
              "Tag Elements",
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
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_templateNames", NickName = "_templateNames", Description = "Revit View Template Names", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_tagType", NickName = "_tagType", Description = "Revit Tag Type", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.Element() { Name = "_elements", NickName = "_elements", Description = "Revit Elements", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_addLeader_", NickName = "_addLeader_", Description = "Add Leader", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(ParamDefinition.FromParam(param_Boolean, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_horizontal_", NickName = "_horizontal_", Description = "Horizontal", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(true);
                result.Add(ParamDefinition.FromParam(param_Boolean, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_viewTypes_", NickName = "_viewTypes_", Description = "Revit View Types to be considered", Access = GH_ParamAccess.list };
                param_String.SetPersistentData(new string[] { Core.ViewType.FloorPlan.ToString() });
                result.Add(ParamDefinition.FromParam(param_String, ParamVisibility.Binding));

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
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.Element() { Name = "tags", NickName = "tags", Description = "Revit Tags", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            List<string> templateNames = new List<string>();
            index = Params.IndexOfInputParam("_templateNames");
            if (index == -1 || !dataAccess.GetDataList(index, templateNames))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Element> elements = new List<Element>();
            index = Params.IndexOfInputParam("_elements");
            if (index == -1 || !dataAccess.GetDataList(index, elements))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool addLeader = false;
            index = Params.IndexOfInputParam("_addLeader_");
            if (index == -1 || !dataAccess.GetData(index, ref addLeader))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool horizontal = true;
            index = Params.IndexOfInputParam("_horizontal_");
            if (index == -1 || !dataAccess.GetData(index, ref horizontal))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Core.ViewType> viewTypes = null;

            List<string> viewTypeNames = new List<string>();
            index = Params.IndexOfInputParam("_viewTypes_");
            if (index != -1 && dataAccess.GetDataList(index, viewTypeNames))
            {
                if (viewTypeNames != null && viewTypeNames.Count != 0)
                {
                    viewTypes = new List<Core.ViewType>();
                    foreach (string viewTypeName in viewTypeNames)
                        if (Enum.TryParse(viewTypeName, true, out Core.ViewType viewType))
                            viewTypes.Add(viewType);
                }
            }

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_tagType");
            if (index == -1 || !dataAccess.GetData(index, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object value = objectWrapper?.Value;
            if (value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (value is IGH_Goo)
                value = (value as dynamic).Value;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
                return;

            ElementId elementId = null;
            if (value is Element)
            {
                elementId = ((Element)value).Id;
            }
            else if (value is ElementId)
            {
                elementId = (ElementId)value;
            }
            else if (value is int)
            {
                elementId = new ElementId((int)value);
            }
            else if (value is string)
            {
                string @string = (string)value;

                FamilySymbol familySymbol = null;
                if (int.TryParse(@string, out int @int))
                {
                    elementId = new ElementId(@int);
                    familySymbol = document.GetElement(elementId) as FamilySymbol;
                }

                if (familySymbol == null)
                {
                    if (RhinoInside.Revit.External.DB.FullUniqueId.TryParse(@string, out Guid documentGuid, out string uniqueId))
                    {
                        familySymbol = document.GetElement(uniqueId) as FamilySymbol;
                    }
                }

                if (familySymbol == null)
                {
                    List<FamilySymbol> familySymbols = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList();
                    familySymbols.RemoveAll(x => x.Category == null || !x.Category.IsTagCategory);

                    familySymbol = familySymbols.Find(x => x.Name.Equals(@string));
                    if (familySymbol == null)
                    {
                        familySymbol = familySymbols.Find(x => @string.Contains(x.Name) || x.Name.Contains(@string));
                    }
                }

                if (familySymbol != null)
                    elementId = familySymbol.Id;
            }
            else if (value is RhinoInside.Revit.GH.Types.Element)
            {
                elementId = ((RhinoInside.Revit.GH.Types.Element)value).Id;
            }

            if (elementId == null || elementId == ElementId.InvalidElementId)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            StartTransaction(document);

            List<IndependentTag> independentTags = Core.Revit.Modify.TagElements(document, templateNames, elementId, elements.ConvertAll(x => x.Id), addLeader, horizontal ? TagOrientation.Horizontal : TagOrientation.Vertical, viewTypes?.ConvertAll(x => (Autodesk.Revit.DB.ViewType)(int)x));

            index = Params.IndexOfOutputParam("tags");
            if (index != -1)
                dataAccess.SetDataList(index, independentTags);
        }
    }
}