using Autodesk.Revit.DB;


namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Construction ToSAM(this HostObjAttributes hostObjAttributes)
        {
            return new Construction(SAM.Core.Revit.Query.FullName(hostObjAttributes));
        }
    }
}
