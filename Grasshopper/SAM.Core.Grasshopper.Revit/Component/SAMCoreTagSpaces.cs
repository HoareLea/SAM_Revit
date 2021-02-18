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
    public class SAMCoreTagSpaces : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("da328efa-7bbb-48b6-b12a-cd0a4f3c98bc");

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
        public SAMCoreTagSpaces()
          : base("SAMCore.TagSpaces", "SAMCore.TagSpaces",
              "Tag Spaces",
              "SAM", "Revit")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_templateNames", "_templateNames", "View Template Names", GH_ParamAccess.list);
            inputParamManager.AddGenericParameter("_spaceTagType", "_spaceTagType", "Revit SpaceTagType", GH_ParamAccess.item);
            
            global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_viewTypes_", NickName = "_viewTypes_", Description = "Revit View Types to be considered", Access = GH_ParamAccess.list };
            @string.SetPersistentData(new string[] { Core.ViewType.FloorPlan.ToString() });
            inputParamManager.AddParameter(@string);

            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Element(), "SpaceTags", "SpaceTags", "SpaceTags", GH_ParamAccess.list);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(3, ref run) || !run)
                return;

            List<string> templateNames = new List<string>();
            if (!dataAccess.GetDataList(0, templateNames))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper = null;
            if (!dataAccess.GetData(1, ref objectWrapper))
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
            if (dataAccess.GetDataList(2, viewTypeNames))
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
                    try
                    {
                        spaceTagType = document.GetElement(@string) as Autodesk.Revit.DB.Mechanical.SpaceTagType;
                    }
                    catch
                    {

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

            List<Autodesk.Revit.DB.Mechanical.SpaceTag> spaceTags = Core.Revit.Modify.TagSpaces(document, templateNames, elementId, viewTypes?.ConvertAll(x => (Autodesk.Revit.DB.ViewType)((int)x)));

            dataAccess.SetDataList(0, spaceTags);
        }
    }
}