using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static IntegerId ToSAM(this ElementId elementId)
        {
            if (elementId == null)
                return null;

            return new IntegerId(elementId.IntegerValue);
        }
    }
}