using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Core.Revit.Enums.Parameter
{
    [AssociatedTypes(typeof(SAMObject)), Description("Revit Element Parameter")]
    public enum ElementParameter
    {
        [ParameterProperties("Revit Element Id", "Integer value from ElementId of Revit Element"), ParameterValue(ParameterType.Integer)] ElementId,
        [ParameterProperties("Revit Unique Id", "UniqueId property of Revit Element"), ParameterValue(ParameterType.String)] UniqueId,
        [ParameterProperties("Revit Category Name", "Category Name of Revit Element"), ParameterValue(ParameterType.String)] CategoryName,
    }
}
