using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Transform ProjectTransform(this Document document, bool inverse = true)
        {
            Transform result= document?.ActiveProjectLocation?.GetTotalTransform();
            if (inverse)
            {
                result = result?.Inverse;
            }

            return result;
        }
    }
}