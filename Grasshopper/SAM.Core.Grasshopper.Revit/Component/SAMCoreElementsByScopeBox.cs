// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper.Revit
{
    public class SAMCoreElementsByScopeBox : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("172b87f6-8b7c-444b-be49-dc7b7057a77e");

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
        public SAMCoreElementsByScopeBox()
          : base("SAMCore.ElementsByScopeBox", "SAMCore.ElementsByScopeBox",
              "Query Elements By ScopeBox",
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
                result.Add(new GH_SAMParam(new RhinoInside.Revit.GH.Parameters.Element() { Name = "_scopeBox", NickName = "_scopeBox", Description = "Revit ScopeBox", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
                result.Add(new GH_SAMParam(new RhinoInside.Revit.GH.Parameters.Element() { Name = "Elements", NickName = "Elements", Description = "Revit Elements", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            int index = Params.IndexOfInputParam("_scopeBox");
            Element element = null;
            if (index == -1 || !dataAccess.GetData(index, ref element) || element == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            if (((BuiltInCategory)element.Category.Id.Value) != BuiltInCategory.OST_VolumeOfInterest)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            BoundingBoxXYZ boundingBoxXYZ = Core.Revit.Query.BoundingBoxXYZ(element);
            if(boundingBoxXYZ == null || boundingBoxXYZ.Min.DistanceTo(boundingBoxXYZ.Max) < Tolerance.Distance)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            Outline outline = new Outline(boundingBoxXYZ.Transform.OfPoint(boundingBoxXYZ.Min), boundingBoxXYZ.Transform.OfPoint(boundingBoxXYZ.Max));

            List<Element> elements = new FilteredElementCollector(document).WherePasses(new LogicalOrFilter(new BoundingBoxIsInsideFilter(outline, Tolerance.MacroDistance), new BoundingBoxIntersectsFilter(outline, Tolerance.MacroDistance))).ToElements()?.ToList();

            List<RhinoInside.Revit.GH.Types.Element> elements_Result = elements.ConvertAll(x => RhinoInside.Revit.GH.Types.Element.FromElement(x));
            elements_Result.RemoveAll(x => x == null || !x.IsValid);

            index = Params.IndexOfOutputParam("Elements");
            if (index != -1)
                dataAccess.SetDataList(index, elements_Result);
        }
    }
}
