using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static IEnumerable<Space> ToSAM_Spaces(this RevitLinkInstance revitLinkInstance, bool fromRooms = true)
        {
            Document document_Source = revitLinkInstance.GetLinkDocument();

            BuiltInCategory builtInCategory;
            if (fromRooms)
                builtInCategory = BuiltInCategory.OST_Rooms;
            else
                builtInCategory = BuiltInCategory.OST_MEPSpaces;

            IEnumerable<SpatialElement> spatialElements = new FilteredElementCollector(document_Source).OfCategory(builtInCategory).Cast<SpatialElement>();

            List<Space> spaces = new List<Space>();
            foreach(SpatialElement spatialElement in spatialElements)
            {
                Space space = ToSAM(spatialElement);
                if (space != null)
                    spaces.Add(space);
            }
            return spaces;
        }
    }
}
