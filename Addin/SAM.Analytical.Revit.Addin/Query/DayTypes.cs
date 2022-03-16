using System.Collections.Generic;

namespace SAM.Analytical.Revit.Addin
{
    public static partial class Query
    {
        public static List<TBD.dayType> DayTypes(this TBD.Calendar calendar)
        {
            if (calendar == null)
                return null;

            List<TBD.dayType> result = new List<TBD.dayType>();

            for (int i = 1; i <= calendar.GetDayTypeCount(); i++)
            {
                TBD.dayType dayType = calendar.dayTypes(i);
                if (dayType != null)
                    result.Add(dayType);
            }

            return result;
        }
    }
}