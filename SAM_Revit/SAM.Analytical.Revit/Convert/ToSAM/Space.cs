using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Analysis;
using SAM.Geometry.Revit;
using SAM.Geometry.Spatial;

namespace SAM.Analytical.Revit
{
    public static partial class Convert
    {
        public static Space ToSAM(this SpatialElement spatialElement)
        {
            Point3D point3D = null;

            if (spatialElement.Location != null)
                point3D = ((LocationPoint)spatialElement.Location).Point.ToSAM();

            string name = Core.Revit.Query.Name(spatialElement);
            if (string.IsNullOrWhiteSpace(name))
                name = spatialElement.Name;

            Space space = new Space(name, point3D);
            Core.ParameterSet parameterSet = Core.Revit.Query.ParameterSet(spatialElement);
            parameterSet.Add(Analytical.Query.ParameterName_SpaceName(), name);
            space.Add(parameterSet);

            return space;
        }

        public static Space ToSAM(this EnergyAnalysisSpace energyAnalysisSpace)
        {
            if (energyAnalysisSpace == null)
                return null;

            Document document = energyAnalysisSpace.Document;

            SpatialElement spatialElement = Core.Revit.Query.Element(document, energyAnalysisSpace.CADObjectUniqueId) as SpatialElement;
            if (spatialElement == null)
                return null;

            return ToSAM(spatialElement);

        }
    }
}