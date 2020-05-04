using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static ElementId ElementId(this SAMObject sAMObject)
        {
            if (sAMObject == null)
                return null;

            ParameterSet parameterSet = sAMObject.GetParameterSet(typeof(Element).Assembly);
            if (parameterSet == null)
                return null;

            int id = parameterSet.ToInt("ElementId");
            if (id == -1)
                return null;

            return new ElementId(id);
        }
    }
}