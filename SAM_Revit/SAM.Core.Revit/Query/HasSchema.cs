using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static bool HasSchema(this Element element, SAMSchema sAMSchema)
        {
            if (element == null || sAMSchema == null || sAMSchema.Guid == Guid.Empty)
                return false;

            IList<Guid> guids = element.GetEntitySchemaGuids();
            if (guids == null || guids.Count == 0)
                return false;

            return guids.Contains(sAMSchema.Guid);
        }
    }
}