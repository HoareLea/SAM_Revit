using Autodesk.Revit.DB;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreDuplicatedElementIds : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1c0aa12e-ffbc-434b-833a-dc157c589b4d");

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
        public SAMCoreDuplicatedElementIds()
          : base("SAMCore.DuplicatedElementIds", "SAMCore.DuplicatedElementIds",
              "Query Filter SAM Objects By ElementIds",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddGenericParameter("_sAMObjects", "_sAMObjects", "SAM Objects", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("In", "In", "Objects In", GH_ParamAccess.list);
            outputParamManager.AddGenericParameter("Out", "Out", "Objects Out", GH_ParamAccess.tree);
            outputParamManager.AddGenericParameter("ElementIds", "ElementIds", "Objects ElementIds", GH_ParamAccess.list);
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

            Dictionary<ElementId, List<SAMObject>> dictionary = new Dictionary<ElementId, List<SAMObject>>();
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

                ElementId elementId = Core.Revit.Query.ElementId(sAMObject);
                if (elementId == null)
                    elementId = ElementId.InvalidElementId;

                if (!dictionary.TryGetValue(elementId, out List<SAMObject> sAMObjects_Temp))
                {
                    sAMObjects_Temp = new List<SAMObject>();
                    dictionary[elementId] = sAMObjects_Temp;
                }

                sAMObjects_Temp.Add(sAMObject);
            }

            List<SAMObject> sAMObjects_In = new List<SAMObject>();
            DataTree<SAMObject> sAMObjects_Out = new DataTree<SAMObject>();
            List<ElementId> elementIds = new List<ElementId>();

            int count = 0;
            foreach(KeyValuePair<ElementId, List<SAMObject>> keyValuePair in dictionary)
            {
                if(keyValuePair.Key == ElementId.InvalidElementId)
                {
                    sAMObjects_In.AddRange(keyValuePair.Value);
                }
                else
                {
                    if(keyValuePair.Value.Count == 1)
                    {
                        sAMObjects_In.AddRange(keyValuePair.Value);
                    }
                    else
                    {
                        GH_Path path = new GH_Path(count);
                        keyValuePair.Value.ForEach(x => sAMObjects_Out.Add(x, path));
                        elementIds.Add(keyValuePair.Key);
                        count++;
                    }
                }
            }


            dataAccess.SetDataList(0, sAMObjects_In);
            dataAccess.SetDataTree(1, sAMObjects_Out);
            dataAccess.SetDataList(2, elementIds);
        }
    }
}