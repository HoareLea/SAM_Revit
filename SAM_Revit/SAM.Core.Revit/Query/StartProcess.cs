using System.Diagnostics;

namespace SAM.Core.Revit
{
    public static partial class Query
    {
        public static Process StartProcess(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

#if Revit2017 || Revit2018 || Revit2019 || Revit2020 || Revit2021 || Revit2022 || Revit2023 || Revit2024
            return System.Diagnostics.Process.Start(path);
#else
            return Core.Query.StartProcess(path);
#endif
        }
    }
}