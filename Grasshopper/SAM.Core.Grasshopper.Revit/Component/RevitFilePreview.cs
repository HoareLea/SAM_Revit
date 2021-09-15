using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Revit.Properties;
using System;
using System.Collections.Generic;

namespace SAM.Core.Grasshopper.Revit
{
    public class RevitFilePreview : SAMTransactionalChainComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1bddea97-6b79-4048-83a2-c614da53478c");

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
        public RevitFilePreview()
          : base("Revit.FilePreview", "Revit.FilePreview",
              "Preview of Revit File",
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
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_path", NickName = "_path", Description = "Revit file path", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
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
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "versionInfo", NickName = "versionInfo", Description = "Version Info", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "versionBuild", NickName = "versionBuild", Description = "Version Build", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "versionNumber", NickName = "versionNumber", Description = "Version Number", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "versionName", NickName = "versionName", Description = "Version Name", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "gUID", NickName = "gUID", Description = "Unique Document GUID", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "increments", NickName = "increments", Description = "Unique Document Increments", Access = GH_ParamAccess.item }, ParamRelevance.Binding));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "modelIdentity", NickName = "modelIdentity", Description = "Model Identity", Access = GH_ParamAccess.item }, ParamRelevance.Occasional));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "familyCategory", NickName = "familyCategory", Description = "Family Category Name", Access = GH_ParamAccess.item }, ParamRelevance.Occasional));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "familyTypes", NickName = "familyTypes", Description = "Family Type Names", Access = GH_ParamAccess.list }, ParamRelevance.Occasional));
                result.Add(new ParamDefinition(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "omniClass", NickName = "omniClass", Description = "OmniClass", Access = GH_ParamAccess.item }, ParamRelevance.Occasional));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void TrySolveInstance(IGH_DataAccess dataAccess)
        {
            int index = -1;

            index = Params.IndexOfInputParam("_path");
            string path = null;
            if(index == -1 || !dataAccess.GetData(index, ref path) || path == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(!System.IO.File.Exists(path))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "File Does not exists");
                return;
            }

            Core.Revit.RevitFilePreview revitFilePreview = new Core.Revit.RevitFilePreview(path);
            if(!revitFilePreview.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid File");
                return;
            }

            index = Params.IndexOfOutputParam("versionInfo");
            if (index != -1)
            {
                dataAccess.SetData(index, revitFilePreview.GetVersionInfo());
            }

            index = Params.IndexOfOutputParam("versionBuild");
            if (index != -1)
            {
                dataAccess.SetData(index, revitFilePreview.GetVersionBuild());
            }

            index = Params.IndexOfOutputParam("versionNumber");
            if (index != -1)
            {
                dataAccess.SetData(index, revitFilePreview.GetVersionNumber());
            }

            index = Params.IndexOfOutputParam("versionName");
            if (index != -1)
            {
                dataAccess.SetData(index, revitFilePreview.GetVersionName());
            }

            index = Params.IndexOfOutputParam("gUID");
            if (index != -1)
            {
                dataAccess.SetData(index, revitFilePreview.GetUniqueDocumentGUID());
            }

            index = Params.IndexOfOutputParam("increments");
            if (index != -1)
            {
                dataAccess.SetData(index, revitFilePreview.GetUniqueDocumentIncrements());
            }

            index = Params.IndexOfOutputParam("modelIdentity");
            if (index != -1)
            {
                dataAccess.SetData(index, revitFilePreview.GetModelIdentity());
            }

            index = Params.IndexOfOutputParam("familyCategory");
            if (index != -1)
            {
                dataAccess.SetData(index, revitFilePreview.GetFamilyCategoryName());
            }

            index = Params.IndexOfOutputParam("familyTypes");
            if (index != -1)
            {
                dataAccess.SetDataList(index, revitFilePreview.GetFamilyTypeNames());
            }

            index = Params.IndexOfOutputParam("omniClass");
            if (index != -1)
            {
                dataAccess.SetData(index, revitFilePreview.GetOmniClass());
            }
        }
    }
}