using SAM.Core;
using SAM.Core.Attributes;
using System.ComponentModel;

namespace SAM.Geometry.Revit
{
    [AssociatedTypes(typeof(LongId)), Description("Revit Id Parameter")]
    public enum RevitIdParameter
    {
        [ParameterProperties("Location", "Revit Location"), SAMObjectParameterValue(false, typeof(Spatial.Point3D), typeof(Spatial.ICurve3D))] Location,
    }
}
