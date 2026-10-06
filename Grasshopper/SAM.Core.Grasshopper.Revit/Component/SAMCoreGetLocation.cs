// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreGetLocation : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("348836ad-6a12-48fc-91ad-f939a8ebda6a");

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
        public SAMCoreGetLocation()
          : base("SAMCore.GetLocation", "SAMCore.GetLocation",
              "Query Gets Location from Revit Document",
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
                result.Add(new GH_SAMParam(new GooLocationParam() { Name = "Location", NickName = "Location", Description = "SAM Location", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;

            int index = Params.IndexOfOutputParam("Location");
            if (index != -1)
                dataAccess.SetData(index, new GooLocation(document.Location()));
        }
    }
}
