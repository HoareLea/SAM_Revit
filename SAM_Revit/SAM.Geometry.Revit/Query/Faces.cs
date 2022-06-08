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

            List<Autodesk.Revit.DB.Face> faces = Faces(hostObject);
            if(faces == null)
            {
                return null;
            }

            List<Autodesk.Revit.DB.Face> result = new List<Autodesk.Revit.DB.Face>();
            foreach (Autodesk.Revit.DB.Face face in faces)
            {
                if (hostObject.GetGeneratingElementIds(face).Any(x => x == generatingElementId))
                {
                    result.Add(face);
                }
            }

            return result;
        }

        public static List<Autodesk.Revit.DB.Face> Faces(this Element element)
        {
            if (element == null)
            {
                return null;
            }

            Options options = element.Document.Application.Create.NewGeometryOptions();
            if (options == null)
            {
                return null;
            }

            GeometryElement geometryElement = element.get_Geometry(options);
            if (geometryElement == null)
            {
                return null;
            }

            List<Autodesk.Revit.DB.Face> result = new List<Autodesk.Revit.DB.Face>();
            foreach (GeometryObject geometryObject in geometryElement)
            {
                List<Autodesk.Revit.DB.Face> faces = new List<Autodesk.Revit.DB.Face>();
                if (geometryObject is Solid)
                {
                    foreach (Autodesk.Revit.DB.Face face in ((Solid)geometryObject).Faces)
                    {
                        result.Add(face);
                    }
                }
            }

            return result;
        }
    }
}