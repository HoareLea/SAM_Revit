using Autodesk.Revit.DB;
using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Revit.Properties;
using SAM.Core;
using SAM.Core.Grasshopper.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper.Revit
{
    public class RevitRenumberSpaces : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("d7c1384b-486d-45b2-ace0-b09deca6d14e");

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
        public RevitRenumberSpaces()
          : base("Revit.RenumberSpaces", "Revit.RenumberSpaces",
              "Renumber Spaces",
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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "_spaces_", NickName = "_spaces_", Description = "Revit Spaces", Access = GH_ParamAccess.list, Optional = true }, ParamRelevance.Binding));

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
                result.Add(new ParamDefinition(new RhinoInside.Revit.GH.Parameters.SpatialElement() { Name = "spaces", NickName = "spaces", Description = "Revit Spaces", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "numbers", NickName = "numbers", Description = "Numbers", Access = GH_ParamAccess.list }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Parameters Updated", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                return result.ToArray();
            }
        }

        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;
            int index_Successful = -1;

            index_Successful = Params.IndexOfOutputParam("successful");
            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, false);

            bool run = false;
            index = Params.IndexOfInputParam("_run");
            if (index == -1 || !dataAccess.GetData(index, ref run) || !run)
                return;

            List<Autodesk.Revit.DB.Mechanical.Space> spaces = new List<Autodesk.Revit.DB.Mechanical.Space>();
            index = Params.IndexOfInputParam("_spaces_");
            if (index != -1)
            {
                dataAccess.GetDataList(index, spaces);
            }

            Document document = RhinoInside.Revit.Revit.ActiveDBDocument;
            if (spaces == null || spaces.Count == 0)
            {
                spaces = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_MEPSpaces).Cast<Autodesk.Revit.DB.Mechanical.Space>().ToList();
            }

            Dictionary<string, Autodesk.Revit.DB.Mechanical.Space> dictionary = null;
            if (spaces != null && spaces.Count != 0)
            {
                StartTransaction(document);
                dictionary = Analytical.Revit.Modify.UpdateNumbers(spaces);
            }

            index = Params.IndexOfOutputParam("spaces");
            if (index != -1)
                dataAccess.SetDataList(index, dictionary?.Values);

            index = Params.IndexOfOutputParam("numbers");
            if (index != -1)
                dataAccess.SetDataList(index, dictionary?.Keys);

            if (index_Successful != -1)
                dataAccess.SetData(index_Successful, dictionary != null && dictionary.Count != 0);

        }
    }
}