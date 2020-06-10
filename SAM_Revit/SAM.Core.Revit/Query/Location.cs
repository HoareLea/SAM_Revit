using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Location Location(this Document document)
        {
            return Location(document?.SiteLocation);
        }

        public static Location Location(this SiteLocation siteLocation)
        {
            if (siteLocation == null)
                return null;

            return new Location(siteLocation.PlaceName, siteLocation.Longitude, siteLocation.Latitude, siteLocation.Elevation);
        }
    }
}