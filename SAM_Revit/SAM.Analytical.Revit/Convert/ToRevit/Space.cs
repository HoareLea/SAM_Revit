using Autodesk.Revit.DB;


namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Autodesk.Revit.DB.Mechanical.Space ToRevit(this Document document, Space space)
        {
            double lowElevation = Query.LowElevation(space);
            if (double.IsNaN(lowElevation))
                return null;

            Level level = Geometry.Revit.Query.LowLevel(document, lowElevation);
            if (level == null)
                return null;

            Autodesk.Revit.DB.Mechanical.Space result = document.Create.NewSpace(level, new UV(UnitUtils.ConvertToInternalUnits(space.Location.X, DisplayUnitType.DUT_METERS), UnitUtils.ConvertToInternalUnits(space.Location.Y, DisplayUnitType.DUT_METERS)));

            result.get_Parameter(BuiltInParameter.ROOM_NAME).Set(space.Name);

            return result;
        }
    }
}
