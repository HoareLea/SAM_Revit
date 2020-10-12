using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreElementsByScopeBox : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("172b87f6-8b7c-444b-be49-dc7b7057a77e");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreElementsByScopeBox()
          : base("SAMCore.ElementsByScopeBox", "SAMCore.ElementsByScopeBox",
              "Query Elements By ScopeBox",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            RhinoInside.Revit.GH.Parameters.Element element = new RhinoInside.Revit.GH.Parameters.Element();
            //element.Optional = true;

            inputParamManager.AddParameter(element, "_scopeBox", "_scopeBox", "Revit ScopeBox", GH_ParamAccess.item);
            //inputParamManager.AddGenericParameter("_elementIds", "_elementIds", "ElementIds", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.Element(), "Elements", "Elements", "Revit Elements", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            Element element = null;
            if (!dataAccess.GetData(0, ref element) || element == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (((BuiltInCategory)element.Category.Id.IntegerValue) != BuiltInCategory.OST_VolumeOfInterest)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            BoundingBoxXYZ boundingBoxXYZ = Core.Revit.Query.BoundingBoxXYZ(element);
            if(boundingBoxXYZ == null || boundingBoxXYZ.Min.DistanceTo(boundingBoxXYZ.Max) < Tolerance.Distance)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Outline outline = new Outline(boundingBoxXYZ.Transform.OfPoint(boundingBoxXYZ.Min), boundingBoxXYZ.Transform.OfPoint(boundingBoxXYZ.Max));

            List<Element> elements = new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(new BoundingBoxIsInsideFilter(outline, Tolerance.MacroDistance), new BoundingBoxIntersectsFilter(outline, Tolerance.MacroDistance))).ToElements()?.ToList();

            List<RhinoInside.Revit.GH.Types.Element> elements_Result = elements.ConvertAll(x => RhinoInside.Revit.GH.Types.Element.FromElement(x));
            elements_Result.RemoveAll(x => x == null || !x.IsValid);

            dataAccess.SetData(0, elements_Result);
        }
    }
}