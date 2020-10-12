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
    public class SAMCoreDeleteElement : SAMTransactionComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6578f2cc-e0b5-4e79-bc27-631e7a9e7a00");

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
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_elements", "_elements", "Elements to be deleted", GH_ParamAccess.list);
            inputParamManager.AddBooleanParameter("_run_", "_run_", "Run", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddIntegerParameter("Ids", "Ids", "Ids", GH_ParamAccess.list);
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            bool run = false;
            if (!dataAccess.GetData(1, ref run) || !run)
                return;

            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (!dataAccess.GetDataList(0, objectWrappers))
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

            IEnumerable<ElementId> result = RhinoInside.Revit.Revit.ActiveUIDocument.Document.Delete(elementIds);

            dataAccess.SetDataList(0, result?.ToList().ConvertAll(x => x.IntegerValue));
        }
    }
}