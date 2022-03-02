using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public class DestinationDuplicateTypeNamesHandler : IDuplicateTypeNamesHandler
    {
        public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs duplicateTypeNamesHandlerArgs)
        {
            return DuplicateTypeAction.UseDestinationTypes;
        }
    }
}