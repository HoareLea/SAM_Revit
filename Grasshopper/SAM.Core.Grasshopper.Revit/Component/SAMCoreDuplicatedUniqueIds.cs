// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreDuplicatedUniqueIds : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("8e7b22ef-9341-41ab-8165-16ce485b7f63");

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
        public SAMCoreDuplicatedUniqueIds()
          : base("SAMCore.DuplicatedUniqueIds", "SAMCore.DuplicatedUniqueIds",
              "Query Filter SAM Objects By UniqueIds",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Out", NickName = "Out", Description = "Objects Out", Access = GH_ParamAccess.tree }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "UniqueIds", NickName = "UniqueIds", Description = "Objects UniqueIds", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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
            int index = Params.IndexOfInputParam("_sAMObjects");

            List<GH_ObjectWrapper> objectWrapperList;

            objectWrapperList = new List<GH_ObjectWrapper>();

            int index_Out = Params.IndexOfOutputParam("Out");

            if (index == -1 || !dataAccess.GetDataList(index, objectWrapperList) || objectWrapperList == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                if (index_Out != -1)
                    dataAccess.SetData(index_Out, false);
                return;
            }

            Dictionary<string, List<SAMObject>> dictionary = new Dictionary<string, List<SAMObject>>();
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

                string uniqueId = Core.Revit.Query.UniqueId(sAMObject);
                if (uniqueId == null)
                    uniqueId = string.Empty;

                if (!dictionary.TryGetValue(uniqueId, out List<SAMObject> sAMObjects_Temp))
                {
                    sAMObjects_Temp = new List<SAMObject>();
                    dictionary[uniqueId] = sAMObjects_Temp;
                }

                sAMObjects_Temp.Add(sAMObject);
            }

            List<SAMObject> sAMObjects_In = new List<SAMObject>();
            DataTree<SAMObject> sAMObjects_Out = new DataTree<SAMObject>();
            List<string> uniqueIds = new List<string>();

            int count = 0;
            foreach(KeyValuePair<string, List<SAMObject>> keyValuePair in dictionary)
            {
                if(keyValuePair.Key == string.Empty)
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
                        uniqueIds.Add(keyValuePair.Key);
                        count++;
                    }
                }
            }

            index = Params.IndexOfOutputParam("In");
            if (index != -1)
                dataAccess.SetDataList(index, sAMObjects_In);

            if (index_Out != -1)
                dataAccess.SetDataTree(index_Out, sAMObjects_Out);

            index = Params.IndexOfOutputParam("UniqueIds");
            if (index != -1)
                dataAccess.SetDataList(index, uniqueIds);
        }
    }
}
