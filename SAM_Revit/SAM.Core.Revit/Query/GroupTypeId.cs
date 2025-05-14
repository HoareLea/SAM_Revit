using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023

#else
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
#endif


    }
}