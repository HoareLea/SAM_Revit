using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static IEnumerable<Core.SAMObject> ToSAM(this Element element)
        {
            IEnumerable<Core.SAMObject> result = null;
            if (element is Wall || element is Floor || element is RoofBase)
            {
                List<Panel> panels = ToSAM((HostObject)element);
                if (panels != null)
                    result = panels.Cast<Core.SAMObject>();
            }
            else if (element is HostObjAttributes)
            {
                Construction construction = ToSAM((HostObjAttributes)element);
                if (construction != null)
                    result = new List<Core.SAMObject>() { construction };
            }
            else if (element is SpatialElement)
            {
                Space space = ToSAM((SpatialElement)element);
                if (space != null)
                    result = new List<Core.SAMObject>() { space };
            }

            return result;
        }
    }
}