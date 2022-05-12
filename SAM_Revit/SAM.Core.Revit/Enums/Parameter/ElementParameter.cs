using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core.Revit
{
    [AssociatedTypes(typeof(SAMObject)), Description("Revit Element Parameter")]
    public enum ElementParameter
    {
        [ParameterProperties("Revit Id", "Revit Id"), SAMObjectParameterValue(typeof(IntegerId))] RevitId,
    }
}
