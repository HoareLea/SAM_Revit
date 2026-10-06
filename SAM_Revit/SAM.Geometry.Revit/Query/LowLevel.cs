// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Revit
{
    public static partial class Query
    {
        /// <summary>
        /// Level equal or below given elevation
        /// </summary>
        /// <param name="document">Revit document</param>
        /// <param name="elevation">Elevation in meters [m]</param>
        public static Level LowLevel(this Document document, double elevation)
        {
            List<Level> levels = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().ToList();
            if (levels == null || levels.Count == 0)
                return null;

            levels.Sort((x, y) => x.Elevation.CompareTo(y.Elevation));

            double levelElevation = UnitUtils.ConvertFromInternalUnits(levels.First().Elevation, UnitTypeId.Meters);

            if (elevation - Core.Tolerance.MacroDistance < levelElevation)
                return levels.First();


            List<Level> levels_Temp = [];
            for (int i = 0; i < levels.Count; i++)
            {

                levelElevation = UnitUtils.ConvertFromInternalUnits(levels[i].Elevation, UnitTypeId.Meters);
                if (System.Math.Round(elevation, 3, MidpointRounding.AwayFromZero) >= System.Math.Round(levelElevation, 3, MidpointRounding.AwayFromZero))
                {
                    levels_Temp.Add(levels[i]);
                }
            }

            return levels_Temp.Last();
        }
    }
}