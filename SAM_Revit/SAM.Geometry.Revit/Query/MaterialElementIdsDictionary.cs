using Autodesk.Revit.DB;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        public static Dictionary<ElementId, List<Shell>> MaterialElementIdsDictionary(this Element element)
        {
            Transform transform = null;
            if (element is FamilyInstance)
                transform = ((FamilyInstance)element).GetTotalTransform();

            Options options = new Options();
            options.ComputeReferences = false;
            options.DetailLevel = ViewDetailLevel.Fine;
            options.IncludeNonVisibleObjects = false;

            return MaterialElementIdsDictionary(element.get_Geometry(options), transform);
        }

        public static Dictionary<ElementId, List<Shell>> MaterialElementIdsDictionary(this GeometryElement geometryElement, Transform transform = null)
        {
            if (geometryElement == null)
            {
                return null;
            }

            Dictionary<ElementId, List<Shell>> result = new Dictionary<ElementId, List<Shell>>();
            foreach (GeometryObject geometryObject in geometryElement)
            {
                Dictionary<ElementId, List<Shell>> dictionary = null;
                if (geometryObject is GeometryInstance)
                {
                    GeometryInstance geometryInstance = (GeometryInstance)geometryObject;

                    GeometryElement geometryElement_Temp = null;
                    if (transform == null)
                    {
                        geometryElement_Temp = geometryInstance.SymbolGeometry;
                    }

                    if (geometryElement_Temp == null)
                    {
                        Transform geometryTransform = geometryInstance.Transform;
                        if (transform != null)
                            geometryTransform = geometryTransform.Multiply(transform.Inverse);

                        geometryElement_Temp = geometryInstance.GetInstanceGeometry(geometryTransform);
                    }


                    if (geometryElement_Temp == null)
                    {
                        continue;
                    }


                    dictionary = MaterialElementIdsDictionary(geometryElement_Temp);
                }
                else if (geometryObject is Solid)
                {
                    Dictionary<ElementId, List<Face3D>> dictionary_Face3D = MaterialElementIdsDictionary((Solid)geometryObject);
                    if (dictionary_Face3D == null)
                    {
                        continue;
                    }

                    ElementId elementId = null;
                    Shell shell = null;
                    if (dictionary_Face3D.Count == 1)
                    {
                        elementId = dictionary_Face3D.Keys.First();
                        shell = new Shell(dictionary_Face3D.Values.First());
                    }
                    else
                    {
                        List<Face3D> face3Ds = new List<Face3D>();
                        double maxArea = double.MinValue;
                        foreach (KeyValuePair<ElementId, List<Face3D>> keyValuePair in dictionary_Face3D)
                        {
                            face3Ds.AddRange(keyValuePair.Value);
                            double area = keyValuePair.Value.ConvertAll(x => x.GetArea()).Sum();
                            if (area > maxArea)
                            {
                                maxArea = area;
                                elementId = keyValuePair.Key;
                            }
                        }
                        shell = new Shell(face3Ds);
                    }

                    if (elementId == null || shell == null)
                    {
                        continue;
                    }

                    if(dictionary == null)
                    {
                        dictionary = new Dictionary<ElementId, List<Shell>>();
                    }

                    if (!dictionary.TryGetValue(elementId, out List<Shell> shells))
                    {
                        shells = new List<Shell>();
                        dictionary[elementId] = shells;
                    }

                    shells.Add(shell);
                }

                if (dictionary == null)
                {
                    continue;
                }

                foreach (KeyValuePair<ElementId, List<Shell>> keyValuePair in dictionary)
                {
                    if (!result.TryGetValue(keyValuePair.Key, out List<Shell> shells))
                    {
                        shells = new List<Shell>();
                        result[keyValuePair.Key] = shells;
                    }

                    shells.AddRange(keyValuePair.Value);
                }
            }
            return result;
        }

        public static Dictionary<ElementId, List<Face3D>> MaterialElementIdsDictionary(this Solid solid)
        {
            FaceArray faceArray = solid?.Faces;
            if(faceArray == null)
            {
                return null;
            }

            Dictionary<ElementId, List<Face3D>> result = new Dictionary<ElementId, List<Face3D>>();
            foreach (Autodesk.Revit.DB.Face face in faceArray)
            {
                List<Face3D> face3Ds = face?.ToSAM();
                if(face3Ds == null)
                {
                    continue;
                }

                ElementId elementId = face.MaterialElementId;
                if (elementId == null)
                {
                    elementId = ElementId.InvalidElementId;
                }

                if(!result.TryGetValue(elementId, out List<Face3D> face3Ds_ElementId))
                {
                    face3Ds_ElementId = new List<Face3D>();
                    result[elementId] = face3Ds_ElementId;
                }

                face3Ds_ElementId.AddRange(face3Ds);
            }

            return result;
        }
    }
}