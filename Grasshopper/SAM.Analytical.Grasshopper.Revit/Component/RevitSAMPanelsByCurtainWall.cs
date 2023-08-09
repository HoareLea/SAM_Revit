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
    public class RevitSAMPanelsByCurtainWall : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("7a1e91e5-24f2-48f7-ae6c-b98438c6fbbd");

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
        public RevitSAMPanelsByCurtainWall()
          : base("Revit.PanelsByCurtainWall", "Revit.PanelsByCurtainWall",
              "Convert Revit Curtain Wall To SAM Analytical Panels \n*optional input ActiveDocument to get all curtain walls from project",
              "SAM", "Revit")
        {
     
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_curtainWall", "_curtainWall", "Revit Curtain Wall\n*or ActiveDocument to get all curtain walls from project", GH_ParamAccess.item);
            inputParamManager.AddBooleanParameter("_includeNonVisibleObjects_", "_includeNonVisibleObjects_", "Include Non Visible Objects", GH_ParamAccess.item, false);
            inputParamManager.AddBooleanParameter("_useProjectLocation_", "_useProjectLocation_", "Transform geometry using Revit Project Location", GH_ParamAccess.item, false);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("panels", "panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(3, ref run) || !run)
                return;

            GH_ObjectWrapper objectWrapper = null;

            if (!dataAccess.GetData(0, ref objectWrapper) || objectWrapper.Value == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool useProjectLocation = false;
            if (!dataAccess.GetData(2, ref useProjectLocation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool includeNonVisibleObjects = false;
            if (!dataAccess.GetData(1, ref includeNonVisibleObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            ConvertSettings convertSettings = new ConvertSettings(true, true, true, useProjectLocation);

            List<Autodesk.Revit.DB.Wall> walls = new List<Autodesk.Revit.DB.Wall>();

            dynamic @object = objectWrapper.Value;
            if(@object is RhinoInside.Revit.GH.Types.ProjectDocument)
            {
                Document document = ((RhinoInside.Revit.GH.Types.ProjectDocument)@object).Value;

                walls = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Wall)).Cast<Autodesk.Revit.DB.Wall>().ToList();
            }
            else
            {
                ElementId aId = @object.Id as ElementId;

                Autodesk.Revit.DB.Wall wall = (@object.Document as Document).GetElement(aId) as Autodesk.Revit.DB.Wall;
                if(wall != null)
                {
                    walls.Add(wall);
                }
            }

            if (walls == null || walls.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid Element");
                return;
            }


            List<Core.ISAMObject> sAMObjects = new List<Core.ISAMObject>();

            foreach(Autodesk.Revit.DB.Wall wall in walls)
            {
                IEnumerable<ElementId> elementIds = wall?.CurtainGrid?.GetPanelIds();
                if(elementIds == null || elementIds.Count() == 0)
                {
                    continue;
                }

                foreach(ElementId elementId in elementIds)
                {
                    Autodesk.Revit.DB.Panel panel = wall.Document.GetElement(elementId) as Autodesk.Revit.DB.Panel;
                    if(panel == null)
                    {
                        continue;
                    }

                    List<Panel> panels = Analytical.Revit.Convert.ToSAM(panel, includeNonVisibleObjects, convertSettings);
                    if(panels != null)
                    {
                        sAMObjects.AddRange(panels.Cast<Core.ISAMObject>());
                    }
                }
            }
                
            dataAccess.SetDataList(0, sAMObjects);
        }
    }
}