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
    public class RevitSAMAnalyticalByType : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("983b8384-71c3-4243-b93b-e63400311864");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Revit;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public RevitSAMAnalyticalByType()
          : base("Revit.SAMAnalyticalByType", "Revit.SAMAnalyticalByType",
              "Convert Revit Link Instance To SAM Analytical Object ie. Panel, Construction, Aperture, ApertureConstruction, Space",
              "SAM", "Revit")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                global::Grasshopper.Kernel.Parameters.Param_String param_String = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_type_", NickName = "_type_", Description = "Type Name ie. Panel, Construction, Aperture, ApertureConstruction, Space", Access = GH_ParamAccess.item };
                param_String.SetPersistentData("Panel");
                result.Add(new GH_SAMParam(param_String, ParamVisibility.Binding));
                
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "revitLinkInstance_", NickName = "revitLinkInstance_", Description = "Revit Link Instance", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "phase_", NickName = "phase_", Description = "Revit Phase", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_useProjectLocation_", NickName = "_useProjectLocation_", Description = "Transform geometry using Revit Project Location", Access = GH_ParamAccess.item};
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Voluntary));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_run", NickName = "_run", Description = "Run", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_run");
            
            bool run = false;
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            string typeName = null;
            index = Params.IndexOfInputParam("_type_");
            if (index == -1 || !dataAccess.GetData(index, ref typeName) || string.IsNullOrWhiteSpace(typeName))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool useProjectLocation = false;
            index = Params.IndexOfInputParam("_useProjectLocation_");
            if (index == -1 || !dataAccess.GetData(index, ref useProjectLocation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            typeName = typeName.Trim();
            
            Type type = Type.GetType(string.Format("{0},{1}", "SAM.Analytical." + typeName, "SAM.Analytical"));
            if(type == null)
            {
                type = Type.GetType(string.Format("{0},{1}", "SAM.Geometry.Revit." + typeName, "SAM.Geometry.Revit"));
            }

            if (type == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (document == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Transform transform = Transform.Identity;

            GH_ObjectWrapper objectWrapper = null;

            index = Params.IndexOfInputParam("revitLinkInstance_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref objectWrapper);
                if (objectWrapper != null)
                {
                    dynamic obj = objectWrapper.Value;

                    ElementId aId = obj.Id as ElementId;

                    Element element = (obj.Document as Document).GetElement(aId);
                    if (element == null || !(element is RevitLinkInstance))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Element");
                        return;
                    }

                    RevitLinkInstance revitLinkInstance = element as RevitLinkInstance;

                    document = revitLinkInstance.GetLinkDocument();
                    transform = revitLinkInstance.GetTotalTransform();
                }
            }

            Phase phase = null;

            index = Params.IndexOfInputParam("phase_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref objectWrapper);
                if(objectWrapper != null && document != null)
                {
                    if(objectWrapper.Value is string)
                    {
                        string name = (string)objectWrapper.Value;
                        phase = document.Phases.Cast<Phase>().ToList().Find(x => x.Name == name);
                    }
                    else
                    {
                        Core.Grasshopper.Revit.Query.TryGetElement(objectWrapper, out phase, document);
                    }


                }
            }

            ConvertSettings convertSettings = new ConvertSettings(true, true, true, useProjectLocation);

            IEnumerable<Core.SAMObject> result = Analytical.Revit.Convert.ToSAM(document, type, convertSettings, transform, phase);


            index = Params.IndexOfOutputParam("analyticalObjects");
            if(index != -1)
            {
                dataAccess.SetDataList(index, result);
            }
        }
    }
}