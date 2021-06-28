using Autodesk.Revit.DB;

namespace SAM.Architectural.Revit
{
    public static partial class Query
    {
        public static IArchitecturalObject Transform(this Transform transform, IArchitecturalObject architecturalObject)
        {
            if (transform == null || architecturalObject == null)
                return null;

            IArchitecturalObject result = Core.Query.Clone(architecturalObject);

            if (transform.IsIdentity)
                return result;

            if (result is BuildingElement)
            {
                BuildingElement buildingElement = (BuildingElement)result;

                buildingElement.Face3D = Geometry.Revit.Query.Transform(transform, buildingElement.Face3D);
                return buildingElement;
            }

            if (result is Room)
            {
                Room room = (Room)result;

                room.Location = Geometry.Revit.Query.Transform(transform, room.Location);
                return room;
            }

            return result;
        }
    }
}