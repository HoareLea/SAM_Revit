using System.Reflection;

namespace SAM.Core.Grasshopper.Revit
{
    public abstract class SAMReconstructElementComponent : RhinoInside.Revit.GH.Components.ReconstructElementComponent
    {
        public SAMReconstructElementComponent(string name, string nickname, string description, string category, string subCategory) 
            : base(name, nickname, description, category, subCategory)
        {
            Message = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
