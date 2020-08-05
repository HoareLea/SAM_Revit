using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Construction ToSAM(this HostObjAttributes hostObjAttributes, Core.Revit.ConvertSettings convertSettings)
        {
            if (hostObjAttributes == null)
                return null;

            Construction result = convertSettings?.GetObject<Construction>(hostObjAttributes.Id);
            if (result != null)
                return result;

            result = new Construction(hostObjAttributes.Name);
            result.Add(Core.Revit.Query.ParameterSet(hostObjAttributes));

            convertSettings?.Add(hostObjAttributes.Id, result);

            return result;
        }
    }
}