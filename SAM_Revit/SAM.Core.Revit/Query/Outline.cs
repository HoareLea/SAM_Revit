using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Outline Outline(this ViewPlan viewPlan, Transform transform = null)
        {
            if (viewPlan == null)
                return null;

            PlanViewRange planViewRange = viewPlan.GetViewRange();
            if (planViewRange == null)
                return null;

            Document document = viewPlan.Document;

            ElementId elementId_Level = null;

            elementId_Level = planViewRange.GetLevelId(PlanViewPlane.TopClipPlane);
            Level level_TopClipPlane = document.GetElement(elementId_Level) as Level;
            double offset_TopClipPlane = planViewRange.GetOffset(PlanViewPlane.TopClipPlane);

            elementId_Level = planViewRange.GetLevelId(PlanViewPlane.BottomClipPlane);
            Level level_BottomClipPlane = document.GetElement(elementId_Level) as Level;
            double offset_BottomClipPlane = planViewRange.GetOffset(PlanViewPlane.BottomClipPlane);

            BoundingBoxXYZ boundingBoxXYZ = viewPlan.CropBox;

            boundingBoxXYZ.Min = new XYZ(boundingBoxXYZ.Min.X, boundingBoxXYZ.Min.Y, level_BottomClipPlane.Elevation + offset_BottomClipPlane);
            boundingBoxXYZ.Max = new XYZ(boundingBoxXYZ.Max.X, boundingBoxXYZ.Max.Y, level_TopClipPlane.Elevation + offset_TopClipPlane);
            
            if (transform != null)
                boundingBoxXYZ.Transform = transform;

            return new Outline(boundingBoxXYZ.Min, boundingBoxXYZ.Max);
        }
    }
}