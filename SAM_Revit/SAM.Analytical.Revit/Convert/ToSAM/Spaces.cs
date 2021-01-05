using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static IEnumerable<Space> ToSAM_Spaces(this RevitLinkInstance revitLinkInstance, Core.Revit.ConvertSettings convertSettings, bool fromRooms = true)
        {
            Document document_Source = revitLinkInstance.GetLinkDocument();

            BuiltInCategory builtInCategory;
            if (fromRooms)
                builtInCategory = BuiltInCategory.OST_Rooms;
            else
                builtInCategory = BuiltInCategory.OST_MEPSpaces;

            IEnumerable<SpatialElement> spatialElements = new FilteredElementCollector(document_Source).OfCategory(builtInCategory).Cast<SpatialElement>();

            List<Space> spaces = new List<Space>();
            foreach (SpatialElement spatialElement in spatialElements)
            {
                Space space = ToSAM(spatialElement, convertSettings);
                if (space != null)
                    spaces.Add(space);
            }
            return spaces;
        }
    }
}