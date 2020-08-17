using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Architectural.Revit
{
    public static partial class Convert
    {
        public static Autodesk.Revit.DB.Level ToRevit(this Level level, Document document, ConvertSettings convertSettings)
        {
            if (level == null || document == null)
                return null;

            Autodesk.Revit.DB.Level result = convertSettings?.GetObject<Autodesk.Revit.DB.Level>(level.Guid);
            if (result != null)
                return result;

            if (!convertSettings.RemoveExisting)
                result = Query.Element<Autodesk.Revit.DB.Level>(document, level);

            double elevation = level.Elevation;
            if (double.IsNaN(elevation))
                return null;

            elevation = UnitUtils.ConvertToInternalUnits(elevation, DisplayUnitType.DUT_METERS);

            if (result == null)
            {
                List<Autodesk.Revit.DB.Level> levels = new FilteredElementCollector(document).OfClass(typeof(Autodesk.Revit.DB.Level)).Cast<Autodesk.Revit.DB.Level>().ToList();
                if (levels != null && levels.Count != 0)
                    result = levels.Find(x => x.Elevation.Equals(elevation));
            }

            if (convertSettings.RemoveExisting)
            {
                ElementId elementId = level.ElementId();
                if (elementId != null && elementId != ElementId.InvalidElementId)
                {
                    Element element = document.GetElement(elementId) as Autodesk.Revit.DB.Level;
                    if (element != null)
                        document.Delete(elementId);
                }
            }

            if (result == null)
                result = Autodesk.Revit.DB.Level.Create(document, elevation);

            if (result == null)
                return null;

            //TODO: Implement method to properly rename element 
            try
            {
                result.Name = level.Name;
            }
            catch(Exception exception)
            {

            }
            

            if (convertSettings.ConvertParameters)
            {
                Modify.Values(level, result);
                Modify.Values(ActiveSetting.Setting, level, result);

                Modify.Json(result, level.ToJObject()?.ToString());
            }

            convertSettings?.Add(level.Guid, result);

            return result;
        }
    }
}