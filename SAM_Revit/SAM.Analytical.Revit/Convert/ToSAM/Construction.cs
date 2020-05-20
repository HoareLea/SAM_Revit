using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Construction ToSAM(this HostObjAttributes hostObjAttributes)
        {
            Construction construction = new Construction(hostObjAttributes.Name);
            construction.Add(Core.Revit.Query.ParameterSet(hostObjAttributes));

            return construction;
        }
    }
}