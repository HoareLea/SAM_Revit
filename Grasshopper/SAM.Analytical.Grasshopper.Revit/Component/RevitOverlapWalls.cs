﻿using Autodesk.Revit.DB;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Analytical.Revit;
using SAM.Core.Grasshopper;
using SAM.Core.Revit;
using SAM.Geometry.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitOverlapWalls : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("bd72b694-f2a5-4dd7-ba36-e37d619fe374");

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
        public RevitOverlapWalls()
          : base("Revit.OverlapWalls", "Revit.OverlapWalls",
              "Query Finds Overlap Revit Walls",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            RhinoInside.Revit.GH.Parameters.HostObject hostObject = new RhinoInside.Revit.GH.Parameters.HostObject();
            hostObject.Optional = true;

            inputParamManager.AddParameter(hostObject, "walls_", "walls_", "Revit Walls", GH_ParamAccess.list);
            inputParamManager.AddBooleanParameter("_run", "_run", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.HostObject(), "walls", "walls", "Revit Walls", GH_ParamAccess.list);
            outputParamManager.AddParameter(new RhinoInside.Revit.GH.Parameters.HostObject(), "OverlapWalls", "OverlapWalls", "Revit Walls", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(1, ref run) || !run)
                return;

            List<HostObject> hostObjects = new List<HostObject>();
            dataAccess.GetDataList(0, hostObjects);

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            if (hostObjects == null || hostObjects.Count == 0)
                hostObjects = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Wall)).Cast<HostObject>().ToList();

            if (hostObjects == null || hostObjects.Count == 0)
                return;

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            List<Panel> panels = new List<Panel>();
            foreach (HostObject hostObject in hostObjects)
            {
                if (!(hostObject is Autodesk.Revit.DB.Wall))
                    continue;

                List<Panel> panels_Temp = hostObject.ToSAM(convertSettings);
                if (panels_Temp != null)
                    panels.AddRange(panels_Temp);
            }

            List<List<Panel>> panelsList_Overlap = Analytical.Query.OverlapPanels(panels);
            if (panelsList_Overlap == null || panelsList_Overlap.Count == 0 || panels.Count != panelsList_Overlap.Count)
                return;

            Dictionary<ElementId, HashSet<ElementId>> dictionary = new Dictionary<ElementId, HashSet<ElementId>>();
            for (int i = 0; i < panels.Count; i++)
            {
                Panel panel = panels[i];
                List<Panel> panels_Overlap = panelsList_Overlap[i];

                ElementId elementId = panel.ElementId();
                if (elementId == null || elementId == ElementId.InvalidElementId)
                    continue;

                HashSet<ElementId> elementIds = null;
                if (!dictionary.TryGetValue(elementId, out elementIds))
                {
                    elementIds = new HashSet<ElementId>();
                    dictionary.Add(elementId, elementIds);
                }

                panels_Overlap.ForEach(x => elementIds.Add(x.ElementId()));
            }

            List<Autodesk.Revit.DB.Wall> walls = new List<Autodesk.Revit.DB.Wall>();
            DataTree<Autodesk.Revit.DB.Wall> dataTree_Walls = new DataTree<Autodesk.Revit.DB.Wall>();
            int count = 0;
            foreach (KeyValuePair<ElementId, HashSet<ElementId>> keyValuePair in dictionary)
            {
                walls.Add(document.GetElement(keyValuePair.Key) as Autodesk.Revit.DB.Wall);

                GH_Path path = new GH_Path(count);
                //if (keyValuePair.Value == null)
                //{
                //    dataTree_Walls.Add(null, path);
                //    continue;
                //}

                keyValuePair.Value.Remove(keyValuePair.Key);
                //if (keyValuePair.Value.Count == 0)
                //{
                //    dataTree_Walls.Add(null, path);
                //    continue;
                //}

                List<Autodesk.Revit.DB.Wall> walls_Overlap = keyValuePair.Value.ToList().ConvertAll(x => document.GetElement(x) as Autodesk.Revit.DB.Wall);
                walls_Overlap.Sort((x, y) => (y.Location as LocationCurve).Curve.Length.CompareTo((x.Location as LocationCurve).Curve.Length));
                walls_Overlap.ForEach(x => dataTree_Walls.Add(x, path));
                count++;
            }

            dataAccess.SetDataList(0, walls);
            dataAccess.SetDataTree(1, dataTree_Walls);
        }
    }
}