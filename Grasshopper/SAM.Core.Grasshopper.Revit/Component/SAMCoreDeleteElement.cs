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
    public class SAMCoreDeleteElement : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6578f2cc-e0b5-4e79-bc27-631e7a9e7a00");

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
        public SAMCoreDeleteElement()
          : base("SAMCore.DeleteElement", "SAMCore.DeleteElement",
              "Modify Deletes Revit Element",
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.Element() { Name = "_elements", NickName = "_elements", Description = "Revit Elements", Access = GH_ParamAccess.list }, ParamRelevance.Binding));

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
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_Integer() { Name = "ids", NickName = "ids", Description = "Revit ELement Ids", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
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

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            index = Params.IndexOfInputParam("_elements");
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ElementId> elementIds = new List<ElementId>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                if (objectWrapper == null)
                    continue;

                object value = null;

                System.Reflection.PropertyInfo propertyInfo = objectWrapper.Value.GetType().GetProperty("Id");
                if (propertyInfo != null)
                    value = propertyInfo.GetValue(objectWrapper.Value);
                else if (objectWrapper.Value is IGH_Goo)
                    value = (objectWrapper.Value as dynamic).Value;
                else
                    value = objectWrapper.Value;

                if (value is int)
                {
                    elementIds.Add(new ElementId((int)value));
                }
                else if (value is double)
                {
                    elementIds.Add(new ElementId(System.Convert.ToInt32(value)));
                }
                else if (value is string)
                {
                    int @int;
                    if (!int.TryParse((string)value, out @int))
                        continue;

                    elementIds.Add(new ElementId(@int));
                }
                else if (value is SAMObject)
                {
                    elementIds.Add(Core.Revit.Query.ElementId((SAMObject)value));
                }
                else if (value is ElementId)
                {
                    elementIds.Add((ElementId)value);
                }
            }

            elementIds?.RemoveAll(x => x == null || x == ElementId.InvalidElementId);

            if (elementIds == null || elementIds.Count == 0)
                return;

            Document document = RhinoInside.Revit.Revit.ActiveUIDocument.Document;

            StartTransaction(document);

            IEnumerable<ElementId> result = document.Delete(elementIds);

            index = Params.IndexOfOutputParam("ids");
            if(index != -1)
                dataAccess.SetDataList(index, result?.ToList().ConvertAll(x => x.IntegerValue));
        }
    }
}