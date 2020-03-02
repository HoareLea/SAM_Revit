using Autodesk.Revit.DB;


namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Construction ToSAM(this HostObjAttributes hostObjAttributes)
        {
            Construction construction = new Construction(SAM.Core.Revit.Query.FullName(hostObjAttributes));
            construction.Add(Core.Revit.Query.ParameterSet(hostObjAttributes));

            return construction;

        }
    }
}
