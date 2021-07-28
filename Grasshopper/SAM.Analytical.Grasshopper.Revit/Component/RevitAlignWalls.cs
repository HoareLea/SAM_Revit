using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Analytical.Revit;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using SAM.Geometry.Planar;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitAlignWalls : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("40eea184-d5f1-45f3-a4cc-45e11ff510fb");

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
        public RevitAlignWalls()
          : base("Revit.AlignWalls", "Revit.AlignWalls",
              "Modify Align Revit Walls at _level ",
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Level() { Name = "_level", NickName = "_level", Description = "Level", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Level() { Name = "_referenceLevel", NickName = "_referenceLevel", Description = "Reference Level", Access = GH_ParamAccess.item }, ParamRelevance.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_maxDistance", NickName = "_maxDistance", Description = "Max Distance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.2);
                result.Add(new ParamDefinition(param_Number, ParamRelevance.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new ParamDefinition(param_Boolean, ParamRelevance.Binding));
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.HostObject() { Name = "walls", NickName = "walls", Description = "Revit Walls", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
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

            double maxDistance = 0.2;
            index = Params.IndexOfInputParam("_maxDistance");
            if (index == -1 || !dataAccess.GetData(index, ref maxDistance))
                return;

            RhinoInside.Revit.GH.Types.Level level_GH = null;
            index = Params.IndexOfInputParam("_level");
            if (index == -1 || !dataAccess.GetData(index, ref level_GH))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            RhinoInside.Revit.GH.Types.Level referenceLevel_GH = null;
            index = Params.IndexOfInputParam("_referenceLevel");
            if (index == -1 || !dataAccess.GetData(index, ref referenceLevel_GH))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Level level = level_GH.Value;
            if (level == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Level referenceLevel = referenceLevel_GH.Value;
            if (level == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation = UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);
            double referenceElevation = UnitUtils.ConvertFromInternalUnits(referenceLevel.Elevation, DisplayUnitType.DUT_METERS);

            Document document = level.Document;

            IEnumerable<Wall> walls_All = new FilteredElementCollector(document).OfClass(typeof(Wall)).Cast<Wall>();
            if (walls_All == null || walls_All.Count() == 0)
                return;

            StartTransaction(document);

            ConvertSettings convertSettings = new ConvertSettings(true, true, true);

            List<Panel> panels = new List<Panel>();
            List<Panel> panels_Reference = new List<Panel>();
            foreach (Wall wall in walls_All)
            {
                List<Panel> panels_Temp = Analytical.Revit.Convert.ToSAM(wall, convertSettings);
                foreach (Panel panel in panels_Temp)
                {
                    double max = panel.MaxElevation();
                    double min = panel.MinElevation();

                    if (Math.Abs(min - elevation) < Core.Tolerance.Distance || (min - Core.Tolerance.Distance < elevation && max - Core.Tolerance.Distance > elevation))
                        panels.Add(panel);

                    if (Math.Abs(min - referenceElevation) < Core.Tolerance.Distance || (min - Core.Tolerance.Distance < referenceElevation && max - Core.Tolerance.Distance > referenceElevation))
                        panels_Reference.Add(panel);
                }
            }

            IEnumerable<ElementId> elementIds = panels.ConvertAll(x => x.ElementId()).Distinct();
            IEnumerable<ElementId> elementIds_Reference = panels_Reference.ConvertAll(x => x.ElementId()).Distinct();

            Geometry.Spatial.Plane plane = new Geometry.Spatial.Plane(new Point3D(0, 0, elevation), Vector3D.WorldZ);

            Dictionary<Segment2D, HostObjAttributes> dictionary_Reference = new Dictionary<Segment2D, HostObjAttributes>();
            foreach (ElementId elementId in elementIds_Reference)
            {
                Element element = document.GetElement(elementId);
                if (element == null)
                    continue;

                HostObjAttributes hostObjAttributes = document.GetElement(element.GetTypeId()) as HostObjAttributes;
                if (hostObjAttributes == null)
                    continue;

                LocationCurve locationCurve = element.Location as LocationCurve;
                ISegmentable3D segmentable3D = locationCurve.ToSAM() as ISegmentable3D;
                if (segmentable3D == null)
                    continue;

                List<Segment3D> segment3Ds = segmentable3D.GetSegments();
                if (segment3Ds == null || segment3Ds.Count == 0)
                    continue;

                segment3Ds.ForEach(x => dictionary_Reference[plane.Convert(x)] = hostObjAttributes);
            }

            Dictionary<Segment2D, ElementId> dictionary = new Dictionary<Segment2D, ElementId>();
            foreach (ElementId elementId in elementIds)
            {
                LocationCurve locationCurve = document.GetElement(elementId).Location as LocationCurve;
                Segment3D segment3D = locationCurve.ToSAM() as Segment3D;
                if (segment3D == null)
                    continue;

                dictionary[plane.Convert(plane.Project(segment3D))] = elementId;
            }

            Dictionary<Segment2D, ElementId> dictionary_Result = new Dictionary<Segment2D, ElementId>();
            foreach (KeyValuePair<Segment2D, ElementId> keyValuePair in dictionary)
            {
                Segment2D segment2D = keyValuePair.Key;

                List<Segment2D> segment2Ds_Temp = dictionary_Reference.Keys.ToList().FindAll(x => x.Collinear(segment2D) && x.Distance(segment2D) <= maxDistance + Core.Tolerance.MacroDistance && x.Distance(segment2D) > Core.Tolerance.MacroDistance);
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    continue;

                Element element = document.GetElement(keyValuePair.Value);
                if (element == null)
                    continue;

                HostObjAttributes hostObjAttributes = document.GetElement(element.GetTypeId()) as HostObjAttributes;
                if (hostObjAttributes == null)
                    continue;

                segment2Ds_Temp.Sort((x, y) => segment2D.Distance(x).CompareTo(segment2D.Distance(y)));

                Segment2D segment2D_Reference = null;

                foreach (Segment2D segment2D_Temp in segment2Ds_Temp)
                {
                    HostObjAttributes hostObjAttributes_Temp = dictionary_Reference[segment2D_Temp];
                    if (hostObjAttributes.Name.Equals(hostObjAttributes_Temp.Name))
                    {
                        segment2D_Reference = segment2D_Temp;
                        break;
                    }
                }

                if (segment2D_Reference == null)
                {
                    HashSet<PanelType> panelTypes = new HashSet<PanelType>();
                    panelTypes.Add(Analytical.Revit.Query.PanelType(hostObjAttributes));
                    switch (panelTypes.First())
                    {
                        case PanelType.CurtainWall:
                            panelTypes.Add(PanelType.WallExternal);
                            break;

                        case PanelType.UndergroundWall:
                            panelTypes.Add(PanelType.WallExternal);
                            break;

                        case PanelType.Undefined:
                            panelTypes.Add(PanelType.WallInternal);
                            break;
                    }

                    foreach (Segment2D segment2D_Temp in segment2Ds_Temp)
                    {
                        HostObjAttributes hostObjAttributes_Temp = dictionary_Reference[segment2D_Temp];
                        PanelType panelType_Temp = Analytical.Revit.Query.PanelType(hostObjAttributes_Temp);
                        if (panelTypes.Contains(panelType_Temp))
                        {
                            segment2D_Reference = segment2D_Temp;
                            break;
                        }
                    }
                }

                if (segment2D_Reference == null)
                    segment2D_Reference = segment2Ds_Temp.First();

                Segment2D segment2D_Project = segment2D_Reference.Project(segment2D);
                if (segment2D_Project == null)
                    continue;

                dictionary_Result[segment2D_Project] = dictionary[segment2D];
            }

            List<HostObject> result = new List<HostObject>();

            foreach (KeyValuePair<Segment2D, ElementId> keyValuePair in dictionary_Result)
            {
                Wall wall = document.GetElement(keyValuePair.Value) as Wall;

                if (wall == null || !wall.IsValidObject)
                    continue;

                Segment2D segment2D = keyValuePair.Key;

                bool pinned = wall.Pinned;

                if (wall.Pinned)
                {
                    using (SubTransaction subTransaction = new SubTransaction(document))
                    {
                        subTransaction.Start();
                        wall.Pinned = false;
                        subTransaction.Commit();
                    }
                }

                Segment3D segment3D = plane.Convert(segment2D);
                LocationCurve locationCurve = wall.Location as LocationCurve;

                using (SubTransaction subTransaction = new SubTransaction(document))
                {
                    subTransaction.Start();

                    document.Regenerate();
                    locationCurve.Curve = Geometry.Revit.Convert.ToRevit(segment3D);

                    subTransaction.Commit();
                }

                if (wall.Pinned != pinned)
                {
                    using (SubTransaction subTransaction = new SubTransaction(document))
                    {
                        subTransaction.Start();
                        wall.Pinned = pinned;
                        subTransaction.Commit();
                    }
                }
                result.Add(wall);
            }

            index = Params.IndexOfOutputParam("walls");
            if (index != -1)
                dataAccess.SetDataList(index, result);
        }
    }
}