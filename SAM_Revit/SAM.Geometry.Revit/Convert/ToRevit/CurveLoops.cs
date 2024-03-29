﻿using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Geometry.Revit
{
    public static partial class Convert
    {
        public static List<CurveLoop> ToRevit(this Face3D face3D, double tolerance = Core.Tolerance.MacroDistance)
        {
            if (face3D == null)
                return null;

            List<CurveLoop> result = new List<CurveLoop>();
            foreach (IClosedPlanar3D closedPlanar3D in face3D.GetEdge3Ds())
            {
                
                List<Line> lines = closedPlanar3D.ToRevit(tolerance);
                if (lines == null)
                    continue;

                CurveLoop curveLoop = new CurveLoop();
                lines.ForEach(x  => curveLoop.Append(x));
                result.Add(curveLoop);
            }

            return result;

        }
    }
}