using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static List<Panel> ToSAM(this HostObject hostObject)
        {
            PanelType panelType = ((HostObjAttributes)hostObject.Document.GetElement(hostObject.GetTypeId())).ToSAM();

            List<Panel> result = new List<Panel>();
            foreach (IClosed3D profile in hostObject.Profiles())
                result.Add(new Panel(hostObject.Name, panelType, profile));

            return result;
        }
    }
}
