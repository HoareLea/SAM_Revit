using System.ComponentModel;

namespace SAM.Analytical.Revit
{
    [Description("Analytical Tool.")]
    public enum Tool
    {
        [Description("Undefined")] Undefined,
        [Description("EnergyPlus")] EnergyPlus,
        [Description("IES")] IES,
        [Description("TAS")] TAS,
        [Description("Other")] Other,
    }
}