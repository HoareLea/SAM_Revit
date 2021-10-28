namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static bool IsTransparent(this Autodesk.Revit.DB.Material material)
        {
            if(material == null)
            {
                return false;
            }

            return material.Transparency >= 10;
        }
    }
}