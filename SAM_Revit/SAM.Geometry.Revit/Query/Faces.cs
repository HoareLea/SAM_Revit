using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static List<Autodesk.Revit.DB.Face> Faces(this HostObject hostObject, ElementId generatingElementId)
        {
            if(hostObject == null || generatingElementId == null)
            {
                return null;
            }

            Options options = hostObject.Document.Application.Create.NewGeometryOptions();
            if(options == null)
            {
                return null;
            }

            GeometryElement geometryElement = hostObject.get_Geometry(options);
            if(geometryElement == null)
            {
                return null;
            }

            List<Autodesk.Revit.DB.Face> result = new List<Autodesk.Revit.DB.Face>();

            List<Solid> solids = new List<Solid>();
            foreach(GeometryObject geometryObject in geometryElement)
            {
                List<Autodesk.Revit.DB.Face> faces = new List<Autodesk.Revit.DB.Face>();
                if (geometryObject is Solid)
                {
                    foreach(Autodesk.Revit.DB.Face face in ((Solid)geometryObject).Faces)
                    {
                        if (hostObject.GetGeneratingElementIds(face).Any(x => x == generatingElementId))
                        {
                            result.Add(face);
                        }
                    }
                }
            }

            return result;
        }
    }
}