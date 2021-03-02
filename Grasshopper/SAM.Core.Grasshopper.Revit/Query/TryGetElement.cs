using Autodesk.Revit.DB;
using Grasshopper.Kernel.Types;

namespace SAM.Core.Grasshopper.Revit
{
    public static partial class Query
    {
        public static bool TryGetElement<T>(this GH_ObjectWrapper objectWrapper, out T element) where T: Element
        {
            element = null;

            if (objectWrapper == null)
                return false;
            
            dynamic obj = objectWrapper.Value;
            if (obj is RhinoInside.Revit.GH.Types.ProjectDocument)
                return false;

            ElementId aId = obj.Id as ElementId;

            element = (obj.Document as Document).GetElement(aId) as T;
            return element != null;
        }
    }
}