using System.Reflection;

namespace SAM.Core.Grasshopper.Revit
{
    public abstract class SAMTransactionalComponent : RhinoInside.Revit.GH.Components.TransactionalComponent
    {
        public SAMTransactionalComponent(string name, string nickname, string description, string category, string subCategory) 
            : base(name, nickname, description, category, subCategory)
        {
            Message = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
