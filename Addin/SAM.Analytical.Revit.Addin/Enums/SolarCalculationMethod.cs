using System.ComponentModel;

namespace SAM.Analytical.Revit
{
    [Description("Solar Calculation Method")]
    public enum SolarCalculationMethod
    {
        [Description("Undefined")] Undefined,
        [Description("None")] None,
        [Description("SAM")] SAM,
        [Description("TAS")] TAS,
    }
}