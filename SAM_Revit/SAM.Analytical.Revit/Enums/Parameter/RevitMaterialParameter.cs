using SAM.Core;
using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Analytical.Revit
{
    [AssociatedTypes(typeof(IMaterial)), Description("Revit Material Parameter")]
    public enum RevitMaterialParameter
    {
        [ParameterProperties("Type Name", "Type Name"), ParameterValue(ParameterType.String)] TypeName,
    }
}
