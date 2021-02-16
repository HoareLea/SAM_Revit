using System.ComponentModel;

namespace SAM.Core
{
    [Description("Revit View Type")]
    public enum ViewType
    {
        //
        // Summary:
        //     Undefined/unspecified type of view.
        [Description("Undefined")] Undefined = 0,
        //
        // Summary:
        //     Floor plan type of view.
        [Description("Floor Plan")] FloorPlan = 1,
        //
        // Summary:
        //     Reflected ceiling plan type of view.
        [Description("Ceiling Plan")] CeilingPlan = 2,
        //
        // Summary:
        //     Elevation type of view.
        [Description("Elevation")] Elevation = 3,
        //
        // Summary:
        //     3-D type of view.
        [Description("3D View")] ThreeD = 4,
        //
        // Summary:
        //     Schedule type of view.
        [Description("Schedule")] Schedule = 5,
        //
        // Summary:
        //     Drawing sheet type of view.
        [Description("Drawing Sheet")] DrawingSheet = 6,
        //
        // Summary:
        //     The project browser view.
        [Description("Project Broswer")] ProjectBrowser = 7,
        //
        // Summary:
        //     Report type of view.
        [Description("Report")] Report = 8,
        //
        // Summary:
        //     Drafting type of view.
        [Description("Drafting View")] DraftingView = 10,
        //
        // Summary:
        //     Legend type of view.
        [Description("Legend")] Legend = 11,
        //
        // Summary:
        //     The MEP system browser view.
        [Description("System Broswer")] SystemBrowser = 12,
        //
        // Summary:
        //     Structural plan or Engineering plan type of view.
        [Description("Engineering Plan")] EngineeringPlan = 115,
        //
        // Summary:
        //     Area plan type of view.
        [Description("Area Plan")] AreaPlan = 116,
        //
        // Summary:
        //     Cross section type of view.
        [Description("Section")] Section = 117,
        //
        // Summary:
        //     Detail type of view.
        [Description("Detail")] Detail = 118,
        //
        // Summary:
        //     Cost Report view.
        [Description("Cost Report")] CostReport = 119,
        //
        // Summary:
        //     Loads Report view.
        [Description("Loads Report")] LoadsReport = 120,
        //
        // Summary:
        //     Pressure Loss Report view.
        [Description("Pressure Loss Report")] PresureLossReport = 121,
        //
        // Summary:
        //     Column Schedule type of view.
        [Description("Column Schedule")] ColumnSchedule = 122,
        //
        // Summary:
        //     Panel Schedule Report view.
        [Description("Panel Schedule")] PanelSchedule = 123,
        //
        // Summary:
        //     Walk-Through type of 3D view.
        [Description("Walkthrough")] Walkthrough = 124,
        //
        // Summary:
        //     Rendering type of view.
        [Description("Rendering")] Rendering = 125,
        //
        // Summary:
        //     Systems analysis report view.
        [Description("Systems Analysis Report")] SystemsAnalysisReport = 126,
        //
        // Summary:
        //     Revit's internal type of view
        //
        // Remarks:
        //     Internal views are not available to API users
        [Description("Internal")] Internal = 214
    }
}