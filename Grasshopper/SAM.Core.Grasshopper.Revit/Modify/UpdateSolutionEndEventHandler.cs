using Grasshopper.Kernel;
using SAM.Core.Revit;

namespace SAM.Core.Grasshopper.Revit
{
    public static partial class Modify
    {
        public static ConvertSettings UpdateSolutionEndEventHandler(this GH_Component gHComponent, ConvertSettings convertSettings = null)
        {
            if (gHComponent == null)
                return null;
            
            if (convertSettings == null)
                return Core.Revit.Query.ConvertSettings();

            GH_Document.SolutionEndEventHandler solutionEndEventHandler = null;

            gHComponent.OnPingDocument().SolutionEnd += solutionEndEventHandler = (sender, args) =>
            {
                (sender as GH_Document).SolutionEnd -= solutionEndEventHandler;
                convertSettings.ClearObjects();
                //gHComponent.Phase = GH_SolutionPhase.Blank;
            };

            return convertSettings;
        }
    }
}