using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitSAMAnalyticalByView : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e577a396-9908-412a-9a36-a02a1d8a3f04");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        public override GH_Exposure Exposure => GH_Exposure.secondary;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_view", NickName = "_view", Description = "Revit View", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "revitLinkInstance_", NickName = "revitLinkInstance_", Description = "Revit Link Instance", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "analyticalObjects", NickName = "analyticalObjects", Description = "SAM Analytical Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitSAMAnalyticalByView()
          : base("Revit.SAMAnalyticalByView", "Revit.SAMAnalyticalByView",
              "Convert Revit To SAM Analytical Object ie. Panel, Space by View",
              "SAM", "Revit")
        {
     
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
            
            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if(index != -1)
            {
                if (!dataAccess.GetData(index, ref run) || !run)
                    return;
            }

            GH_ObjectWrapper objectWrapper = null;


            index = Params.IndexOfInputParam("revitLinkInstance_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref objectWrapper);
            }

            Transform tranform = null;
            Document document = null;
            if(objectWrapper != null && Core.Grasshopper.Revit.Query.TryGetElement(objectWrapper, out RevitLinkInstance revitLinkInstance) && revitLinkInstance != null)
            {
                document = revitLinkInstance.GetLinkDocument();
                tranform = revitLinkInstance.GetTotalTransform();
            }
            else if (objectWrapper?.Value is RhinoInside.Revit.GH.Types.ProjectDocument)
            {
                document = ((RhinoInside.Revit.GH.Types.ProjectDocument)objectWrapper.Value).Value;
            }

            if (document == null)
            {
                document = RhinoInside.Revit.Revit.ActiveDBDocument;
            }

            if(document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(tranform == null)
            {
                tranform = Transform.Identity;
            }

            index = Params.IndexOfInputParam("_view");
            if(index == -1 || !dataAccess.GetData(index, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            IEnumerable<ElementId> elementIds = null;
            if (Core.Grasshopper.Revit.Query.TryGetElement(objectWrapper, out ViewPlan viewPlan))
            {
                Outline outline = Core.Revit.Query.Outline(viewPlan, tranform);
                if (outline != null)
                {
                    LogicalOrFilter logicalOrFilter = new LogicalOrFilter(new BoundingBoxIsInsideFilter(outline, Core.Tolerance.MacroDistance), new BoundingBoxIntersectsFilter(outline, Core.Tolerance.MacroDistance));
                    elementIds = new FilteredElementCollector(document).WherePasses(logicalOrFilter)?.ToElementIds();


                    //BoundingBoxIntersectsFilter boundingBoxIntersectsFilter = new BoundingBoxIntersectsFilter(outline, Core.Tolerance.MacroDistance);
                    //elementIds = new FilteredElementCollector(document).WherePasses(boundingBoxIntersectsFilter).ToElementIds();
                }
            }

            if (elementIds == null || elementIds.Count() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }    

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);
            IEnumerable<Core.ISAMObject> sAMObjects = null;

            List<Panel> panels = Analytical.Revit.Convert.ToSAM_Panels(document, elementIds, convertSettings);
            if (panels != null)
                sAMObjects = panels.Cast<Core.ISAMObject>();

            dataAccess.SetDataList(0, sAMObjects);
        }
    }
}