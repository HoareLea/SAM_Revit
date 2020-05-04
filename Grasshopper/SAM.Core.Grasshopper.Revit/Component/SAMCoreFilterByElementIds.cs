using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreFilterByElementIds : GH_Component
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2eda1e16-2640-4da1-8557-6a74975bdaa6");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreFilterByElementIds()
          : base("SAMCore.FilterByElementIds", "SAMCore.FilterByElementIds",
              "Filter SAM Objects By ElementIds",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_sAMObjects", "_sAMObjects", "SAM Objects", GH_ParamAccess.list);
            inputParamManager.AddGenericParameter("_elementIds", "_elementIds", "ElementIds", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("In", "In", "Objects In", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("Out", "Out", "Objects Out", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<GH_ObjectWrapper> objectWrapperList;

            objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(0, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrapperList)
            {
                if (objectWrapper == null || objectWrapper.Value == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null SAMObject");
                    continue;
                }

                SAMObject sAMObject = null;
                if (objectWrapper.Value is SAMObject)
                    sAMObject = objectWrapper.Value as SAMObject;
                else if (objectWrapper.Value is IGH_Goo)
                    sAMObject = (objectWrapper.Value as dynamic).Value as SAMObject;

                if (sAMObject == null)
                    continue;

                sAMObjects.Add(sAMObject);
            }

            objectWrapperList = new List<GH_ObjectWrapper>();

            if (!dataAccess.GetDataList(1, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                dataAccess.SetData(1, false);
                return;
            }

            HashSet<Autodesk.Revit.DB.ElementId> elementIds = new HashSet<Autodesk.Revit.DB.ElementId>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrapperList)
            {
                if (objectWrapper == null || objectWrapper.Value == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Null ElementId");
                    continue;
                }

                if (objectWrapper.Value is int)
                    elementIds.Add(new Autodesk.Revit.DB.ElementId((int)objectWrapper.Value));
                else if (objectWrapper.Value is GH_Integer)
                    elementIds.Add(new Autodesk.Revit.DB.ElementId(((GH_Integer)objectWrapper.Value).Value));
                else if (objectWrapper.Value is string)
                {
                    int value;
                    if (int.TryParse((string)objectWrapper.Value, out value))
                        elementIds.Add(new Autodesk.Revit.DB.ElementId(value));
                }
                else if (objectWrapper.Value is GH_Number)
                {
                    elementIds.Add(new Autodesk.Revit.DB.ElementId((int)((GH_Number)objectWrapper.Value).Value));
                }
                else if (objectWrapper.Value is GH_String)
                {
                    int value;
                    if (int.TryParse(((GH_String)objectWrapper.Value).Value, out value))
                        elementIds.Add(new Autodesk.Revit.DB.ElementId(value));
                }
            }

            List<SAMObject> result_in = new List<SAMObject>();
            List<SAMObject> result_out = new List<SAMObject>();
            foreach (SAMObject sAMObject in sAMObjects)
            {
                Autodesk.Revit.DB.ElementId elementId = Core.Revit.Query.ElementId(sAMObject);
                if (elementIds.Contains(elementId))
                    result_in.Add(sAMObject);
                else
                    result_out.Add(sAMObject);
            }

            dataAccess.SetDataList(0, result_in);
            dataAccess.SetDataList(1, result_out);
        }
    }
}