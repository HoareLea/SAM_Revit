using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAM.Geometry.Spatial;

using Autodesk.Revit.DB;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static string FullName(this Element element)
        {
            return FullName(((ElementType)element.Document.GetElement(element.GetTypeId())).FamilyName, element.Name);
        }

        public static string FullName(string familyName, string familyTypeName)
        {
            if (string.IsNullOrWhiteSpace(familyName) || string.IsNullOrWhiteSpace(familyTypeName))
                return null;

            return string.Format("{0}: {1}", familyName, familyTypeName);
        }
    }
}
