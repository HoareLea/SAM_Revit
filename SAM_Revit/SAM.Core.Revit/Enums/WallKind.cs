using System.ComponentModel;

namespace SAM.Core.Revit
{
    [Description("Revit Wall Kind")]
    public enum WallKind
    {
        //
        // Summary:
        //     The basic type is not known.
        [Description("Unknown")] Unknown = -1,
        //
        // Summary:
        //     A standard wall.
        [Description("Basic")] Basic = 0,
        //
        // Summary:
        //     A curtain wall.
        [Description("Curtain")] Curtain = 1,
        //
        // Summary:
        //     A stacked wall of several wall types.
        [Description("Stacked")] Stacked = 2

    }
}