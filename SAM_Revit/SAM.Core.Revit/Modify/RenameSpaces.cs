using System.Collections.Generic;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Modify
    {
        public static List<Space> RenameSpaces(this IEnumerable<Space> spaces, TextMap textMap)
        {
            if (spaces == null || textMap == null)
                return null;

            List<Space> result = new List<Space>();
            foreach(Space space in spaces)
            {
                Parameter parameter = space?.get_Parameter(BuiltInParameter.ROOM_NAME);
                if (parameter == null)
                    continue;

                string name = parameter.AsString();
                if (name == null)
                    continue;

                string name_New = textMap.Replace(name);
                if (name_New == name)
                    continue;

                if (!parameter.Set(name_New))
                    continue;

                result.Add(space);

            }

            return result;
        }
    }
}