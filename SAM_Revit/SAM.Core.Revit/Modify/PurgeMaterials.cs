using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static Dictionary<ElementId, string> PurgeMaterials(this Document document)
        {
            if(document == null)
            {
                return null;
            }

            return new PurgeMaterialsWrapper(document).Purge();
        }

        public static Dictionary<ElementId, string> PurgeMaterials(this Document document, params ElementId[] elementIds)
        {
            if (document == null || elementIds == null)
            {
                return null;
            }

            return new PurgeMaterialsWrapper(document).Purge(elementIds);
        }

        public static Dictionary<ElementId, string> PurgeMaterials(this Document document, params string[] names)
        {
            if (document == null || names == null)
            {
                return null;
            }

            return new PurgeMaterialsWrapper(document).Purge(names);
        }


    }
}