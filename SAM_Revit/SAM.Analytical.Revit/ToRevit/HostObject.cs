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
                foreach (Geometry.Spatial.Segment3D segment3D in panel.ToPolycurveLoop().GetCurves())
                    curveList.Add(segment3D.ToRevit());

                double lowElevation = panel.LowElevation();

                Level level = document.LowLevel(lowElevation);
                if (level == null)
                    return null;

                result = Wall.Create(document, curveList, aHostObjAttributes.Id, level.Id, false);

                Parameter parameter = null;

                parameter = result.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                parameter = result.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
                if (parameter != null)
                {
                    double height = UnitUtils.ConvertToInternalUnits((panel.HightElevation() - lowElevation), DisplayUnitType.DUT_METERS);
                    parameter.Set(height);
                }

                double levelElevation = UnitUtils.ConvertFromInternalUnits(level.Elevation, DisplayUnitType.DUT_METERS);
                if (System.Math.Abs(lowElevation - levelElevation) > SAM.Geometry.Tolerance.MacroDistance)
                {
                    parameter = result.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET);
                    if (parameter != null)
                        parameter.Set(UnitUtils.ConvertToInternalUnits(lowElevation - levelElevation, DisplayUnitType.DUT_METERS));
                }

                return result;
            }
            else if (aHostObjAttributes is FloorType)
            {
                CurveArray curveArray = new CurveArray();
                foreach (Geometry.Spatial.Segment3D segment3D in panel.ToPolycurveLoop().GetCurves())
                    curveArray.Append(segment3D.ToRevit());

                Level level = document.HighLevel(panel.LowElevation());

                result = document.Create.NewFloor(curveArray, aHostObjAttributes as FloorType, level, false);
                result.ChangeTypeId(aHostObjAttributes.Id);

                return result;

            }
            else if(aHostObjAttributes is RoofType)
            {
                List<Geometry.Spatial.ICurve3D> curve3Ds = panel.ToPolycurveLoop().GetCurves();

                CurveArray curveArray = new CurveArray();
                foreach (Geometry.Spatial.Segment3D segment3D in curve3Ds)
                    curveArray.Append(segment3D.ToRevit());

                double elevation = panel.LowElevation();
                Level level = document.HighLevel(elevation);

                ModelCurveArray modelCurveArray = new ModelCurveArray();
                RoofBase roofBase = document.Create.NewFootPrintRoof(curveArray, level, aHostObjAttributes as RoofType, out modelCurveArray);

                Parameter parameter = roofBase.get_Parameter(BuiltInParameter.ROOF_UPTO_LEVEL_PARAM);
                if (parameter != null)
                    parameter.Set(ElementId.InvalidElementId);

                SlabShapeEditor slabShapeEditor = roofBase.SlabShapeEditor;
                slabShapeEditor.ResetSlabShape();

                foreach (Geometry.Spatial.ICurve3D curve3D in curve3Ds)
                {
                    Geometry.Spatial.Point3D point3D = curve3D.GetStart();
                    if (Math.Abs(point3D.Z - elevation) > Geometry.Tolerance.MicroDistance)
                        slabShapeEditor.DrawPoint(point3D.ToRevit());
                }

                return roofBase;
            }

            return null;
        }
    }
}
