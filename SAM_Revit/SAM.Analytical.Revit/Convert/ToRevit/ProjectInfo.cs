using Autodesk.Revit.DB;
using SAM.Core.Revit;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static ProjectInfo ToRevit(this AdjacencyClusterSimulationResult adjacencyClusterSimulationResult, Document document, ConvertSettings convertSettings)
        {
            if (adjacencyClusterSimulationResult == null || document == null)
                return null;

            ProjectInfo result = convertSettings?.GetObject<ProjectInfo>(adjacencyClusterSimulationResult.Guid);
            if (result != null)
                return result;

            result = document.ProjectInformation;

            if (convertSettings.ConvertParameters)
            {
                if(result != null)
                {
                    Core.Revit.Modify.SetValues(result, adjacencyClusterSimulationResult);
                    Core.Revit.Modify.SetValues(result, adjacencyClusterSimulationResult, ActiveSetting.Setting);
                    Modify.SetValues(result, adjacencyClusterSimulationResult, ActiveSetting.Setting, adjacencyClusterSimulationResult.LoadType());
                }
                
            }

            convertSettings?.Add(adjacencyClusterSimulationResult.Guid, result);

            return result;
        }
    }
}