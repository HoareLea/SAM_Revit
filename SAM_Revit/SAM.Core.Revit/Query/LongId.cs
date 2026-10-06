// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020-2026 Michal Dengusiak & Jakub Ziolkowski and contributors
using Autodesk.Revit.DB;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static LongId LongId(this Element element)
        {
            if (element == null)
                return null;

            LongId result = Convert.ToSAM(element.Id);
            if (result == null)
                return null;

            string fullName = FullName(element);
            if(!string.IsNullOrEmpty(fullName))
            {
                result.SetValue(RevitIdParameter.FullName, fullName);
            }

            Autodesk.Revit.DB.Category category = element is Family ? ((Family)element).FamilyCategory : element.Category;
            if(category != null)
            {
                result.SetValue(RevitIdParameter.CategoryName, category.Name);
                result.SetValue(RevitIdParameter.CategoryId, category.Id.Value);
            }

            result.SetValue(RevitIdParameter.UniqueId, element.UniqueId);

            return result;
        }
    }
}