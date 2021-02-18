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
    public class SAMCoreTagElements : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a69e2526-e01f-4fd1-b517-bfc52f83db9d");

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
        public SAMCoreTagElements()
          : base("SAMCore.TagElements", "SAMCore.TagElements",
              "Tag Elements",
              "SAM", "Revit")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddTextParameter("_templateNames", "_templateNames", "View Template Names", GH_ParamAccess.list);
            
            inputParamManager.AddGenericParameter("_tagType", "_tagType", "Revit tagType", GH_ParamAccess.item);
            
            inputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Element(), "_elements", "_elements", "Revit Elements", GH_ParamAccess.list);

            inputParamManager.AddBooleanParameter("_addLeader_", "_addLeader_", "Add Leader", GH_ParamAccess.item, false);
            inputParamManager.AddBooleanParameter("_horizontal_", "_horizontal_", "Is Horizontal", GH_ParamAccess.item, true);

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
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Element(), "Tags", "Tags", "Tags", GH_ParamAccess.list);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(6, ref run) || !run)
                return;

            List<string> templateNames = new List<string>();
            if (!dataAccess.GetDataList(0, templateNames))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Element> elements = new List<Element>();
            if (!dataAccess.GetDataList(2, elements))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool addLeader = false;
            if (!dataAccess.GetData(3, ref addLeader))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool horizontal = true;
            if (!dataAccess.GetData(4, ref horizontal))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Core.ViewType> viewTypes = null;

            List<string> viewTypeNames = new List<string>();
            if (dataAccess.GetDataList(5, viewTypeNames))
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

                FamilySymbol familySymbol = null;
                if (int.TryParse(@string, out int @int))
                {
                    elementId = new ElementId(@int);
                    familySymbol = document.GetElement(elementId) as FamilySymbol;
                }

                if(familySymbol == null)
                {
                    try
                    {
                        familySymbol = document.GetElement(@string) as FamilySymbol;
                    }
                    catch
                    {

                    }
                }

                if(familySymbol == null)
                {
                    List<FamilySymbol> familySymbols = new FilteredElementCollector(document).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>().ToList();
                    familySymbols.RemoveAll(x => x.Category == null || !x.Category.IsTagCategory);

                    familySymbol = familySymbols.Find(x => x.Name.Equals(@string));
                    if(familySymbol == null)
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

            List<IndependentTag> independentTags = Core.Revit.Modify.TagElements(document, templateNames, elementId, elements.ConvertAll(x => x.Id), addLeader, horizontal ? TagOrientation.Horizontal : TagOrientation.Vertical, viewTypes?.ConvertAll(x => (Autodesk.Revit.DB.ViewType)(int)x));

            dataAccess.SetDataList(0, independentTags);
        }
    }
}