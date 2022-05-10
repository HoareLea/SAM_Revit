using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Geometry.Revit
{
    [AssociatedTypes(typeof(Tag)), Description("Tag Parameter")]
    public enum TagParameter
    {
        [ParameterProperties("Leader", "Leader"), ParameterValue(Core.ParameterType.Boolean)] Leader,
        [ParameterProperties("Orientation", "Orientation"), ParameterValue(Core.ParameterType.String)] Orientation,
    }
}
