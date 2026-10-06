// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static ForgeTypeId GroupTypeId(string groupName)
        {
            if(groupName == null)
            {
                return null;
            }


            IList<ForgeTypeId> groupTypeIds = ParameterUtils.GetAllBuiltInGroups();

            foreach (ForgeTypeId groupTypeId in groupTypeIds)
            {
                string groupName_Temp = LabelUtils.GetLabelForGroup(groupTypeId);

                if(groupName_Temp == groupName)
                {
                    return groupTypeId;
                }
            }

            return null;
        }


    }
}