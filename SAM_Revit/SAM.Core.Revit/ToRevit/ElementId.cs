using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;


namespace SAM.Core.Revit
{
    public static partial class Convert
    {
        public static ElementId ToRevit(this IntegerId integerId)
        {
            if (integerId == null)
                return null;

            return new ElementId(integerId.Id);
        }
    }
}
