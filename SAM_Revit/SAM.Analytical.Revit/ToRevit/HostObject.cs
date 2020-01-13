using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using SAM.Geometry.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static HostObject ToRevit(this Document document, Panel panel)
        {
            HostObjAttributes aHostObjAttributes = document.ToRevit(panel.Construction, panel.PanelType);


            HostObject result = null; 
            if (aHostObjAttributes is WallType)
            {
                List<Curve> curveList = new List<Curve>();
                foreach(Geometry.Spatial.Segment3D segment3D in panel.ToPolycurveLoop().GetCurves())
                    curveList.Add(segment3D.ToRevit());

                result = Wall.Create(document, curveList, false);
                result.ChangeTypeId(aHostObjAttributes.Id);

                return result;
            }

            throw new NotImplementedException();
        }
    }
}
