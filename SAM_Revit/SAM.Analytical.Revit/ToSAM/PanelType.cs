using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static PanelType ToSAM(this HostObjAttributes hostObjAttributes)
        {
            return new PanelType(Query.FullName(hostObjAttributes));
        }
    }
}
