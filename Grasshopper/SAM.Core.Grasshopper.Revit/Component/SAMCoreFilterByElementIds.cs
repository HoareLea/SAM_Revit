// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreFilterByElementIds : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2eda1e16-2640-4da1-8557-6a74975bdaa6");

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
        public SAMCoreFilterByElementIds()
          : base("SAMCore.FilterByElementIds", "SAMCore.FilterByElementIds",
              "Query Filter SAM Objects By ElementIds",
              "SAM", "Revit")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_sAMObjects", NickName = "_sAMObjects", Description = "SAM Objects", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_elementIds", NickName = "_elementIds", Description = "ElementIds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "In", NickName = "In", Description = "Objects In", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Out", NickName = "Out", Description = "Objects Out", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index_Out = Params.IndexOfOutputParam("Out");

            int index = Params.IndexOfInputParam("_sAMObjects");
            List<GH_ObjectWrapper> objectWrapperList;

            objectWrapperList = new List<GH_ObjectWrapper>();

            if (index == -1 || !dataAccess.GetDataList(index, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                if (index_Out != -1)
                    dataAccess.SetData(index_Out, false);
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

            index = Params.IndexOfInputParam("_elementIds");
            objectWrapperList = new List<GH_ObjectWrapper>();

            if (index == -1 || !dataAccess.GetDataList(index, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                if (index_Out != -1)
                    dataAccess.SetData(index_Out, false);
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

            index = Params.IndexOfOutputParam("In");
            if (index != -1)
                dataAccess.SetDataList(index, result_in);

            if (index_Out != -1)
                dataAccess.SetDataList(index_Out, result_out);
        }
    }
}
