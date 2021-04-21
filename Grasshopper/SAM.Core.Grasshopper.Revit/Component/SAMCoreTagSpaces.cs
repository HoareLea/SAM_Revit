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
    public class SAMCoreTagSpaces : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("da328efa-7bbb-48b6-b12a-cd0a4f3c98bc");

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
        public SAMCoreTagSpaces()
          : base("SAMCore.TagSpaces", "SAMCore.TagSpaces",
              "Tag Spaces",
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
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_templateNames", NickName = "_templateNames", Description = "View Template Names", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(ParamDefinition.FromParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_spaceTagType", NickName = "_spaceTagType", Description = "Revit SpaceTagType", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                
                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_viewTypes_", NickName = "_viewTypes_", Description = "Revit View Types to be considered", Access = GH_ParamAccess.list };
                param_String.SetPersistentData(new string[] { Core.ViewType.FloorPlan.ToString() });
                result.Add(ParamDefinition.FromParam(param_String, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
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
                result.Add(ParamDefinition.FromParam(new RhinoInside.Revit.GH.Parameters.Element() { Name = "spaceTags", NickName = "spaceTags", Description = "Revit SpaceTags", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            GH_ObjectWrapper objectWrapper = null;
            index = Params.IndexOfInputParam("_spaceTagType");
            if (index ==-1 || !dataAccess.GetData(index, ref objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object value = objectWrapper?.Value;
            if(value == null)
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

            else if (value is IGH_Goo)
                value = (value as dynamic).Value;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
                return;

            ElementId elementId = null;
            if (value is Element)
            {
                elementId = ((Element)value).Id;
            }
            else if(value is ElementId)
            {
                elementId = (ElementId)value;
            }
            else if(value is int)
            {
                elementId = new ElementId((int)value);
            }
            else if(value is string)
            {
                string @string = (string)value;

                Autodesk.Revit.DB.Mechanical.SpaceTagType spaceTagType = null;
                if (int.TryParse(@string, out int @int))
                {
                    elementId = new ElementId(@int);
                    spaceTagType = document.GetElement(elementId) as Autodesk.Revit.DB.Mechanical.SpaceTagType;
                }

                if (spaceTagType == null)
                {
                    if (RhinoInside.Revit.External.DB.FullUniqueId.TryParse(@string, out Guid documentGuid, out string uniqueId))
                    {
                        spaceTagType = document.GetElement(uniqueId) as Autodesk.Revit.DB.Mechanical.SpaceTagType;
                    }
                }

                if (spaceTagType == null)
                {
                    List<Autodesk.Revit.DB.Mechanical.SpaceTagType> spaceTagTypes = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Mechanical.SpaceTagType)).Cast<Autodesk.Revit.DB.Mechanical.SpaceTagType>().ToList();
                    spaceTagType = spaceTagTypes.Find(x => x.Name.Equals(@string));
                    if(spaceTagType == null)
                    {
                        spaceTagType = spaceTagTypes.Find(x => @string.Contains(x.Name) || x.Name.Contains(@string));
                    }
                }

                if (spaceTagType != null)
                    elementId = spaceTagType.Id;
            }
            else if(value is RhinoInside.Revit.GH.Types.Element)
            {
                elementId = ((RhinoInside.Revit.GH.Types.Element)value).Id;
            }

            if(elementId == null || elementId == ElementId.InvalidElementId)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            StartTransaction(document);

            List<Autodesk.Revit.DB.Mechanical.SpaceTag> spaceTags = Core.Revit.Modify.TagSpaces(document, templateNames, elementId, viewTypes?.ConvertAll(x => (Autodesk.Revit.DB.ViewType)((int)x)));

            index = Params.IndexOfOutputParam("spaceTags");
            if (index != -1)
                dataAccess.SetDataList(index, spaceTags);
        }
    }
}