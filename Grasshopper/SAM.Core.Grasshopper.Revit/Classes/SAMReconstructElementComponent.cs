﻿using Grasshopper.Kernel;
using System.Windows.Forms;

namespace SAM.Core.Grasshopper.Revit
{
    public abstract class SAMReconstructElementComponent : RhinoInside.Revit.GH.Components.ReconstructElementComponent, IGH_SAMComponent
    {
        public SAMReconstructElementComponent(string name, string nickname, string description, string category, string subCategory) 
            : base(name, nickname, description, category, subCategory)
        {
            SetValue("SAM_SAMVersion", Core.Query.CurrentVersion());
            SetValue("SAM_ComponentVersion", LatestComponentVersion);
        }

        public override bool Obsolete
        {

            get
            {
                return Grasshopper.Query.Obsolete(this);
            }
        }

        public string ComponentVersion
        {
            get
            {
                return GetValue("SAM_ComponentVersion", null);
            }
        }

        public string SAMVersion
        {
            get
            {
                return GetValue("SAM_SAMVersion", null);
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);
            Menu_AppendSeparator(menu);

            this.AppendSourceCodeAdditionalMenuItem(menu);
            this.AppendNewComponentAdditionalMenuItem(menu);
        }

        public abstract string LatestComponentVersion { get; }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            Message = ComponentVersion;
        }
    }
}
