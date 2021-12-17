using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core.Grasshopper.Revit;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class SAMAnalyticalUpdateParameters : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("a4772fdd-828c-4d2e-8cc9-3b446eb17563");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalUpdateParameters()
          : base("SAMAnalytical.UpdateParameters", "SAMAnalytical.UpdateParameters",
              "Updates Revit Element Parameter based on Analytical Object \n *works on Spaces at the moment \n connect Spaces or Analytical Model ",
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
                result.Add(new ParamDefinition(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object \n connect Spaces or Analytical Model", Access = GH_ParamAccess.item }, ParamRelevance.Binding));

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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Element() { Name = "elements", NickName = "elements", Description = "Revit Elements", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Parameters Updated", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            int index_Successful = -1;

            index_Successful = Params.IndexOfOutputParam("successful");
            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, false);

            bool run = false;
            if (!dataAccess.GetData(1, ref run) || !run)
                return;

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            IAnalyticalObject analyticalObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Core.SAMObject sAMObject = analyticalObject as Core.SAMObject;
            if (sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Tuple<ElementId, Core.SAMObject>> tuples = new List<Tuple<ElementId, Core.SAMObject>>();
            if(sAMObject is Space)
            {
                Space space = (Space)sAMObject;

                ElementId elementId = space.ElementId();
                if (elementId == null || elementId == ElementId.InvalidElementId)
                {
                    List<Autodesk.Revit.DB.Mechanical.Space> spaces_Revit = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();
                    if(spaces_Revit != null)
                    {
                        Autodesk.Revit.DB.Mechanical.Space space_Revit = spaces_Revit.Find(x => x.Name != null && x.Name.Equals(space.Name));
                        if (space_Revit == null)
                        {
                            foreach (Autodesk.Revit.DB.Mechanical.Space space_Revit_Temp in spaces_Revit)
                            {
                                Parameter parameter = space_Revit_Temp?.get_Parameter(BuiltInParameter.ROOM_NAME);
                                if (parameter == null || !parameter.HasValue)
                                {
                                    continue;
                                }

                                string name = parameter.AsString();
                                if (string.IsNullOrEmpty(name))
                                {
                                    continue;
                                }

                                if (name.Equals(space.Name))
                                {
                                    space_Revit = space_Revit_Temp;
                                    break;
                                }
                            }
                        }

                        if(space_Revit != null)
                        {
                            elementId = space_Revit.Id;
                        }
                    }
                }

                tuples.Add(new Tuple<ElementId, Core.SAMObject> (elementId, space));
                if(elementId != null && elementId != ElementId.InvalidElementId)
                {
                    tuples.Add(new Tuple<ElementId, Core.SAMObject>(elementId, space.InternalCondition));
                }
            }
            else if(sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = (AnalyticalModel)sAMObject;
                List<Space> spaces = analyticalModel.GetSpaces();
                if(spaces != null)
                {
                    foreach (Space space in spaces)
                    {
                        ElementId elementId = space.ElementId();
                        if (elementId == null || elementId == ElementId.InvalidElementId)
                        {
                            List<Autodesk.Revit.DB.Mechanical.Space> spaces_Revit = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();
                            if (spaces_Revit != null)
                            {
                                Autodesk.Revit.DB.Mechanical.Space space_Revit = spaces_Revit.Find(x => x.Name != null && x.Name.Equals(space.Name));
                                if (space_Revit == null)
                                {
                                    foreach (Autodesk.Revit.DB.Mechanical.Space space_Revit_Temp in spaces_Revit)
                                    {
                                        Parameter parameter = space_Revit_Temp?.get_Parameter(BuiltInParameter.ROOM_NAME);
                                        if (parameter == null || !parameter.HasValue)
                                        {
                                            continue;
                                        }

                                        string name = parameter.AsString();
                                        if (string.IsNullOrEmpty(name))
                                        {
                                            continue;
                                        }

                                        if (name.Equals(space.Name))
                                        {
                                            space_Revit = space_Revit_Temp;
                                            break;
                                        }
                                    }
                                }

                                if (space_Revit != null)
                                {
                                    elementId = space_Revit.Id;
                                }
                            }
                        }

                        tuples.Add(new Tuple<ElementId, Core.SAMObject>(elementId, space));
                        if (elementId != null && elementId != ElementId.InvalidElementId)
                        {
                            tuples.Add(new Tuple<ElementId, Core.SAMObject>(elementId, space.InternalCondition));
                        }
                    }
                }
            }
            else
            {
                ElementId elementId = ((Core.SAMObject)analyticalObject).ElementId();
                if(elementId != null && elementId != ElementId.InvalidElementId)
                {
                    tuples.Add(new Tuple<ElementId, Core.SAMObject>(elementId, sAMObject));
                }
            }

            if (tuples == null || tuples.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Cound not find matching Revit Element");
                return;
            }

            StartTransaction(document);

            List<Element> elements = new List<Element>();
            foreach(Tuple<ElementId, Core.SAMObject> tuple in tuples)
            {
                if(tuple.Item2 == null)
                {
                    continue;
                }
                
                if(tuple.Item1 == null || tuple.Item1 == ElementId.InvalidElementId)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Cound not find matching Revit Element for SAM Object [Guid: {0}]", tuple.Item2.Guid));
                }

                Element element = document.GetElement(tuple.Item1);
                if(element == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, string.Format("Cound not find matching Revit Element for SAM Object [Guid: {0}]", tuple.Item2.Guid));
                }

                Core.Revit.Modify.SetValues(element, sAMObject);
                Core.Revit.Modify.SetValues(element, sAMObject, ActiveSetting.Setting);
                elements.Add(element);
            }

            index = Params.IndexOfOutputParam("elements");
            if (index != -1)
                dataAccess.SetDataList(0, elements);

            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, true);
        }
    }
}