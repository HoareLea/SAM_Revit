using System;
using System.IO;

namespace SAM.Core.Revit
{
    public static partial class Create
    {
        public static RevitFilePreview RevitFilePreview(string path, bool supressExceptions = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                if (!supressExceptions)
                    throw new Exception(string.Format("File path empty or null", path));

                return null;
            }

            if (!File.Exists(path))
            {
                if (!supressExceptions)
                    throw new FileNotFoundException(string.Format("The file \"{0}\" was not found.", path));

                return null;
            }

            try
            {
                RevitFilePreview revitFilePreview = new RevitFilePreview(path);
                return revitFilePreview;
            }
            catch (Exception exception)
            {
                if (!supressExceptions)
                    throw exception;
            }

            return null;
        }
    }
}