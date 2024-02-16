using SAM.Core;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static string FullName(this ApertureConstruction apertureConstruction)
        {
            if(apertureConstruction == null)
            {
                return null;
            }


            if(string.IsNullOrWhiteSpace(apertureConstruction.Name))
            {
                return null;
            }

            return string.Format("{0}s: {1}", apertureConstruction.ApertureType.Description(), apertureConstruction.Name);

            
        }
    }
}