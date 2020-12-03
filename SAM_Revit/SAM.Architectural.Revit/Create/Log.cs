using Autodesk.Revit.DB;
using SAM.Core;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural.Revit
{
    public static partial class Create
    {
        public static Log Log(this IEnumerable<Level> levels, Document document)
        {
            if (levels == null || document == null)
                return null;

            Log result = new Log();
            foreach (Level level in levels)
                result.AddRange(Log(level, document));

            return result;
        }
        
        public static Log Log(this Level level, Document document)
        {
            if(level == null || document == null)
                return null;

            List<Autodesk.Revit.DB.Level> levels_Revit = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Level)).Cast<Autodesk.Revit.DB.Level>().ToList();

            Log result = new Log();

            Core.Revit.Create.Log(level, document);

            Autodesk.Revit.DB.Level level_Revit = levels_Revit.Find(x => System.Math.Abs(Query.Elevation(x) - level.Elevation) < Tolerance.MacroDistance);

            if (level_Revit == null)
                result.Add("Revit Level with elevation {0} is missing.", LogRecordType.Error, level.Elevation);
            else if(!level_Revit.Name.Equals(level.Name))
                result.Add("Revit Level {0} and SAM Level {1} names does not math.", LogRecordType.Warning, level_Revit.Name, level.Name);

            return result;
        }
    }
}