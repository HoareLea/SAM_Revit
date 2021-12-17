using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Revit
{
    public static partial class Modify
    {
        public static Dictionary<string, Autodesk.Revit.DB.Mechanical.Space> UpdateNumbers(IEnumerable<Autodesk.Revit.DB.Mechanical.Space> spaces)
        {
            if(spaces == null || spaces.Count() == 0)
            {
                return null;
            }

            Dictionary<string, List<Autodesk.Revit.DB.Mechanical.Space>> dictionary = new Dictionary<string, List<Autodesk.Revit.DB.Mechanical.Space>>();
            foreach (Autodesk.Revit.DB.Mechanical.Space space in spaces)
            {
                

                string name = space?.Level?.Name;
                if (string.IsNullOrEmpty(name))
                    name = string.Empty;

                if (!dictionary.TryGetValue(name, out List<Autodesk.Revit.DB.Mechanical.Space> spaces_Level))
                {
                    spaces_Level = new List<Autodesk.Revit.DB.Mechanical.Space>();
                    dictionary[name] = spaces_Level;
                }

                spaces_Level.Add(space);
            }


            Dictionary<string, Autodesk.Revit.DB.Mechanical.Space> result = new Dictionary<string, Autodesk.Revit.DB.Mechanical.Space>();
            foreach (KeyValuePair<string, List<Autodesk.Revit.DB.Mechanical.Space>> keyValuePair in dictionary)
            {
                XYZ xyz_Min = new XYZ(double.MaxValue, double.MaxValue, double.MaxValue);
                foreach (Autodesk.Revit.DB.Mechanical.Space space_Temp in keyValuePair.Value)
                {
                    XYZ xyz = (space_Temp.Location as LocationPoint)?.Point;
                    if (xyz == null)
                        continue;

                    xyz_Min = new XYZ(Math.Min(xyz.X, xyz_Min.X), Math.Min(xyz.Y, xyz_Min.Y), Math.Min(xyz.Z, xyz_Min.Z));
                }

                List<Tuple<double, Autodesk.Revit.DB.Mechanical.Space>> tuples = new List<Tuple<double, Autodesk.Revit.DB.Mechanical.Space>>();
                foreach (Autodesk.Revit.DB.Mechanical.Space space_Temp in keyValuePair.Value)
                {
                    XYZ xyz = (space_Temp.Location as LocationPoint)?.Point;
                    if (xyz == null)
                        continue;

                    tuples.Add(new Tuple<double, Autodesk.Revit.DB.Mechanical.Space>(xyz.DistanceTo(xyz_Min), space_Temp));
                }

                tuples.Sort((x, y) => x.Item1.CompareTo(y.Item1));

                int count = 1;
                string levelName = keyValuePair.Key.Replace("Level", string.Empty).Trim();
                foreach (Autodesk.Revit.DB.Mechanical.Space space in tuples.ConvertAll(x => x.Item2))
                {
                    string number = count.ToString();
                    while (number.Length < 3)
                        number = "0" + number;

                    if (!string.IsNullOrWhiteSpace(levelName))
                        number = string.Format("{0}_{1}", levelName, number);


                    if(space.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(number))
                    {
                        result[number] = space;
                        count++;
                    }
                }
            }

            return result;
        }
    }
}