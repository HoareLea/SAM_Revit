using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical.Revit
{
    [AssociatedTypes(typeof(Panel)), Description("Revit Panel Parameter")]
    public enum RevitPanelParameter
    {
        [ParameterProperties("Length", "Length [m]"), DoubleParameterValue()] Length,
    }
}
