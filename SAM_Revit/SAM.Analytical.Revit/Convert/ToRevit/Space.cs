﻿using Autodesk.Revit.DB;
using SAM.Core.Revit;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Autodesk.Revit.DB.Mechanical.Space ToRevit(this Space space, Document document, ConvertSettings convertSettings)
        {
            if (space == null)
                return null;

            Autodesk.Revit.DB.Mechanical.Space result = convertSettings?.GetObject<Autodesk.Revit.DB.Mechanical.Space>(space.Guid);
            if (result != null)
                return result;

            if (!convertSettings.RemoveExisting)
                result = Core.Revit.Query.Element<Autodesk.Revit.DB.Mechanical.Space>(document, space);

            if(result == null)
            {
                double lowElevation = Query.LowElevation(space);
                if (double.IsNaN(lowElevation))
                    return null;

                Level level = Geometry.Revit.Query.LowLevel(document, lowElevation);
                if (level == null)
                    return null;

#if Revit2017 || Revit2018 || Revit2019 || Revit2020
                result = document.Create.NewSpace(level, new UV(UnitUtils.ConvertToInternalUnits(space.Location.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(space.Location.Y, DisplayUnitType.DUT_METERS)));
#else
                result = document.Create.NewSpace(level, new UV(UnitUtils.ConvertToInternalUnits(space.Location.X, UnitTypeId.Meters), UnitUtils.ConvertToInternalUnits(space.Location.Y, UnitTypeId.Meters)));
#endif
            }

            if (result == null)
                return null;

            //result.get_Parameter(BuiltInParameter.ROOM_NAME).Set(string.Empty);
            //result.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(space.Name);

            result.get_Parameter(BuiltInParameter.ROOM_NAME).Set(space.Name);

            if (convertSettings.ConvertParameters)
            {
                Dictionary<string, object> parameters = convertSettings.GetParameters();

                InternalCondition internalCondition = space.InternalCondition;
                if(internalCondition != null)
                {
                    Core.Revit.Modify.SetValues(result, internalCondition);
                    Core.Revit.Modify.SetValues(result, internalCondition, ActiveSetting.Setting, parameters);
                }

                Core.Revit.Modify.SetValues(result, space);
                Core.Revit.Modify.SetValues(result, space, ActiveSetting.Setting, parameters);

                Core.Revit.Modify.SetJson(result, space.ToJObject()?.ToString());
            }

            convertSettings?.Add(space.Guid, result);

            return result;
        }
    }
}