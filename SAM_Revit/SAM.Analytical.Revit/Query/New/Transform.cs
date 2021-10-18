using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Analytical.Revit
{
    public static partial class Query
    {
        public static IAnalyticalObject Transform(this Transform transform, IAnalyticalObject analyticalObject)
        {
            if (transform == null || analyticalObject == null)
                return null;

            IAnalyticalObject result = Core.Query.Clone(analyticalObject);

            if (transform.IsIdentity)
                return result;

            if (result is AirPartition)
            {
                AirPartition airPartition = (AirPartition)result;
                return new AirPartition(airPartition.Guid, Geometry.Revit.Query.Transform(transform, airPartition.Face3D));
            }

            if (result is IHostPartition)
            {
                return Transform(transform, (IHostPartition)result);
            }

            if (result is Room)
            {
                Room room = (Room)result;

                room.Location = Geometry.Revit.Query.Transform(transform, room.Location);
                return room;
            }

            return result;
        }

        public static IHostPartition Transform(Transform transform, IHostPartition hostPartition)
        {
            if (transform == null || hostPartition == null)
                return null;

            IHostPartition result = Analytical.Create.HostPartition(hostPartition.Guid, Geometry.Revit.Query.Transform(transform, hostPartition.Face3D), hostPartition.Type());

            List<IOpening> openings = hostPartition.Openings;
            if(openings != null)
            {
                foreach(IOpening opening in openings)
                {
                    result.AddOpening(Transform(transform, opening));
                }
            }

            return result;
        }

        public static IOpening Transform(Transform transform, IOpening opening)
        {
            if (transform == null || opening == null)
                return null;

            return Analytical.Create.Opening(opening.Guid, opening.Type(), Geometry.Revit.Query.Transform(transform, opening.Face3D));
        }
    }
}