using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
	public static partial class Convert
	{
		public static Wall ToRevit_Wall(this Geometry.Spatial.Face3D face3D, Document document, WallType wallType, Level level)
        {
			if (face3D == null || document == null)
				return null;

			Geometry.Spatial.Vector3D normal = face3D.GetPlane().Normal;
			if (normal == null)
				return null;

			if(wallType == null)
            {
				ElementId elementId = document.Settings.Categories.get_Item(BuiltInCategory.OST_Walls).Id;
				if(elementId != null && elementId != ElementId.InvalidElementId)
                {
					elementId = document.GetDefaultFamilyTypeId(elementId);
					if (elementId != null && elementId != ElementId.InvalidElementId)
                    {
						wallType = document.GetElement(elementId) as WallType;

					}
				}
			}

			if (wallType == null)
				return null;

			if(level == null)
            {
				double elevation = Geometry.Revit.Convert.ToRevit(face3D.GetBoundingBox().Min).Z;
				level = Core.Revit.Query.LowLevel(document, elevation);
            }

			if (level == null)
				return null;

			List<CurveLoop> curveLoops = Geometry.Revit.Convert.ToRevit(face3D);
			curveLoops?.RemoveAll(x => x == null);
			if (curveLoops == null || curveLoops.Count == 0)
				return null;

			if(curveLoops.Count == 1)
				return Wall.Create(document, curveLoops[0].ToList(), wallType.Id, level.Id, false, Geometry.Revit.Convert.ToRevit(normal, false));

			List<Curve> curves = new List<Curve>();

			foreach (CurveLoop curveLoop in curveLoops)
			{
				List<Curve> curves_Temp = curveLoop?.ToList();
				if (curveLoop == null)
					continue;

				List<Curve> curves_Postponed = new List<Curve>();

				int startindex = 0;
				while (startindex < curves_Temp.Count)
				{
					Curve curve = curves_Temp[startindex];
					if (!(curve is Line))
					{
						curves_Postponed.Add(curve);
						startindex++;
						continue;
					}
					XYZ dir = curve.GetEndPoint(1).Subtract(curve.GetEndPoint(0)).Normalize();
					if (!dir.IsAlmostEqualTo(XYZ.BasisZ) && !dir.IsAlmostEqualTo(XYZ.BasisZ.Negate()))
					{
						curves_Postponed.Add(curve);
						startindex++;
					}
					else break;
				}

				for (int i = startindex; i < curves_Temp.Count; i++)
				{
					Curve curve = curves_Temp[i];
					curves.Add(curve);
				}

				if (curves_Postponed.Count > 0)
					curves.AddRange(curves_Postponed);
			}

			if (curves == null || curves.Count == 0)
				return null;

			return Wall.Create(document, curves, wallType.Id, level.Id, false, Geometry.Revit.Convert.ToRevit(normal, false));

		}
	}
}