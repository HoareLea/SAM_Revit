using Autodesk.Revit.DB;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static CurtainCell CurtainCell(this CurtainSystem curtainSystem, ElementId uGridLineElementId, ElementId vGridLineElementId)
        {
            if(uGridLineElementId == null || uGridLineElementId == ElementId.InvalidElementId || vGridLineElementId == null || vGridLineElementId == ElementId.InvalidElementId)
            {
                return null;
            }

            CurtainGridSet curtainGridSet = curtainSystem?.CurtainGrids;
            if(curtainGridSet == null)
            {
                return null;
            }

            foreach(CurtainGrid curtainGrid in curtainGridSet)
            {
                CurtainCell result = curtainGrid.GetCell(uGridLineElementId, vGridLineElementId);
                if(result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}