using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace SAM.Core.Revit
{
    public class RevitFilePreview : IDisposable
    {
        private static string namespaceName = "http://www.w3.org/2005/Atom";
        
        private string path = string.Empty;
        private byte[] previewData = null;
        private byte[] fileInfoData = null;
        private XDocument xDocument = null;
        private StorageInfo storageInfo = null;
        private bool disposed;

        public string Path
        {
            get
            {
                return path;
            }

            protected set
            {
                path = value;
            }
        }

        public RevitFilePreview(string path)
        {
            if (!File.Exists(path))
                return;

            this.path = path;

            storageInfo = (StorageInfo)InvokeStorageRootMethod(null, "Open", path, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (storageInfo != null)
            {
                ReadStructuredStorageFile(storageInfo);
                CloseStorageInfo(storageInfo);
            }
        }

        public void Dispose()
        {
            if (disposed == false)
            {
                if (storageInfo != null)
                    CloseStorageInfo(storageInfo);

                disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public Image GetImage()
        {
            if ((previewData == null) || (previewData.Length <= 0))
            {
                using (Bitmap bitmap = new Bitmap(128, 128))
                {
                    return bitmap.Clone() as Bitmap;
                }
            }

            int startingOffset = GetPngStartingOffset();
            if (startingOffset == 0)
            {
                using (Bitmap bitmap = new Bitmap(128, 128))
                {
                    return bitmap.Clone() as Bitmap;
                }
            }

            try
            {
                byte[] pngDataBuffer = new byte[previewData.GetUpperBound(0) - startingOffset + 1];
                using (MemoryStream memoryStream = new MemoryStream(previewData))
                {
                    memoryStream.Position = startingOffset;
                    memoryStream.Read(pngDataBuffer, 0, pngDataBuffer.Length);
                }

                byte[] decoderData = null;

                if (pngDataBuffer != null)
                {
                    using (MemoryStream memoryStream = new MemoryStream(pngDataBuffer))
                    {
                        PngBitmapDecoder pngBitmapDecoder = new PngBitmapDecoder(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                        decoderData = BitmapSourceToArray(pngBitmapDecoder.Frames[0]);
                    }
                }

                if ((decoderData != null) && (decoderData.Length > 0))
                {
                    using (MemoryStream memoryStream = new MemoryStream(decoderData))
                    {
                        using (Bitmap bitmap = new Bitmap((memoryStream)))
                        {
                            using (Image image = bitmap)
                            {
                                return image.Clone() as Image;
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                string messsage = exception.Message;
            }

            using (Bitmap bitmap = new Bitmap(128, 128))
            {
                return bitmap.Clone() as Bitmap;
            }
        }

        public string GetVersionInfo()
        {
            Dictionary<string, string> dictionary = GetParameterDictionary("Revit Build", "Author", "Build", "Format");
            //Revit 2016, 2017, 2018
            //Revit Build: Autodesk Revit 2016 (Build: 20150110_1515(x64))
            if (dictionary.ContainsKey("Revit Build"))
                return dictionary["Revit Build"];

            string author = null;
            string build = null;
            string format = null;

            //Revit 2019
            //Author: Autodesk Revit
            if (!dictionary.ContainsKey("Author"))
                author = dictionary["Author"];
            if (string.IsNullOrEmpty(author))
                return null;

            //Revit 2019
            //Build: 20180112_1515(x64)
            if (!dictionary.ContainsKey("Build"))
                build = dictionary["Build"];
            if (string.IsNullOrEmpty(build))
                return null;

            //Revit 2019
            //Format: 2019
            if (!dictionary.ContainsKey("Format"))
                format = dictionary["Format"];
            if (string.IsNullOrEmpty(format))
                return null;

            if (!string.IsNullOrEmpty(author) && !string.IsNullOrEmpty(format) && !string.IsNullOrEmpty(build))
                return string.Format("{0} {1} (Build: {2})", author, format, build);

            return null;
        }

        public string GetVersionBuild()
        {
            Dictionary<string, string> dictionary = GetParameterDictionary("Revit Build", "Build");
            //Revit 2019
            //Build: 20180112_1515(x64)
            if (dictionary.ContainsKey("Build"))
                return dictionary["Build"];

            string build = null;

            //Revit 2016, 2017, 2018
            //Revit Build: Autodesk Revit 2016 (Build: 20150110_1515(x64))
            if (!dictionary.ContainsKey("Revit Build"))
                return null;

            build = dictionary["Revit Build"];
            if (string.IsNullOrEmpty(build))
                return null;

            int index;

            index = build.IndexOf('(');
            if (index < 1)
                return null;

            string buildInfo = build.Substring(index + 1, build.Length - index - 2);
            if (string.IsNullOrEmpty(buildInfo))
                return null;

            index = buildInfo.IndexOf(':');
            if (index < 1 || index + 2 >= buildInfo.Length)
                return null;

            return buildInfo.Substring(index + 2);
        }

        public string GetVersionNumber()
        {
            Dictionary<string, string> dictionary = GetParameterDictionary("Revit Build", "Format");
            //Revit 2019
            //Format: 2019
            if (dictionary.ContainsKey("Format"))
                return dictionary["Format"];

            string build = null;

            //Revit 2016, 2017, 2018
            //Revit Build: Autodesk Revit 2016 (Build: 20150110_1515(x64))
            if (!dictionary.ContainsKey("Revit Build"))
                return null;

            build = dictionary["Revit Build"];
            if (string.IsNullOrEmpty(build))
                return null;

            int index = build.IndexOf('(');
            if (index <= 1)
                return null;

            build = build.Substring(0, index - 1);
            build = build.Trim();
            index = build.LastIndexOf(' ');
            if (index <= 1)
                return null;

            index++;

            build = build.Substring(index).Trim();
            int versionNumber;
            if (int.TryParse(build, out versionNumber))
                return build;

            return null;
        }

        public string GetVersionName()
        {
            Dictionary<string, string> dictionary = GetParameterDictionary("Revit Build", "Author");
            //Revit 2019
            //Author: Autodesk Revit
            if (dictionary.ContainsKey("Author"))
                return dictionary["Author"];

            string build = null;

            //Revit 2016, 2017, 2018
            //Revit Build: Autodesk Revit 2016 (Build: 20150110_1515(x64))
            if (!dictionary.ContainsKey("Revit Build"))
                return null;

            build = dictionary["Revit Build"];
            if (string.IsNullOrEmpty(build))
                return null;

            int index = build.IndexOf('(');
            if (index <= 1)
                return null;

            build = build.Substring(0, index - 1);
            build = build.Trim();

            index = build.LastIndexOf(' ');
            if (index <= 1)
                return build;

            string versionNumberString = build.Substring(index).Trim();
            int versionNumber;

            if (int.TryParse(versionNumberString, out versionNumber))
                return build.Substring(0, index).Trim();
            else
                return build.Trim();
        }

        public string GetUniqueDocumentGUID()
        {
            Dictionary<string, string> dictionary = GetParameterDictionary("Unique Document GUID");
            //Revit 2016, 2017, 2018, 2019
            //Unique Document GUID: c1366468-e878-4bd6-81e7-2b6c81b4b263
            if (dictionary.ContainsKey("Unique Document GUID"))
                return dictionary["Unique Document GUID"];

            return null;
        }

        public int? GetUniqueDocumentIncrements()
        {
            Dictionary<string, string> dictionary = GetParameterDictionary("Unique Document Increments");
            //Revit 2016, 2017, 2018, 2019
            //Unique Document Increments: 83
            if (dictionary.ContainsKey("Unique Document Increments"))
            {
                string @string = dictionary["Unique Document Increments"];
                int increments;
                if (int.TryParse(@string, out increments))
                    return increments;
            }

            return null;
        }

        public string ModelIdentity()
        {
            Dictionary<string, string> dictionary = GetParameterDictionary("Model Identity");
            //Revit 2016, 2017, 2018, 2019
            //Model Identity: 00000000-0000-0000-0000-000000000000
            if (dictionary.ContainsKey("Model Identity"))
                return dictionary["Model Identity"];

            return null;
        }

        public string GetFamilyCategoryName()
        {
            if (xDocument == null || xDocument.Root == null || xDocument.Root.Attributes() == null)
                return null;

            List<XAttribute> xAttributes = xDocument.Root.Attributes().ToList();

            XAttribute xAttribute = xAttributes.Find(x => x.Name.LocalName == "xmlns");
            if (xAttribute != null)
            {
                XName xName = XName.Get("category", xAttribute.Value);
                List<XElement> xElements = xDocument.Root.Elements(xName).ToList();
                if (xElements != null)
                {
                    xName = XName.Get("scheme", xAttribute.Value);
                    foreach (XElement xElement in xElements)
                    {
                        XElement xElement_Scheme = xElement.Element(xName);
                        if (xElement_Scheme != null && xElement_Scheme.Value == "adsk:revit:grouping")
                        {
                            xName = XName.Get("term", xAttribute.Value);
                            XElement xElement_Term = xElement.Element(xName);
                            if (xElement_Term != null)
                                return xElement_Term.Value;
                        }
                    }
                }
            }

            return null;
        }

        public bool IsCompatibleVersionNumber(string versionNumber)
        {
            if (string.IsNullOrEmpty(versionNumber))
                return false;

            string versionNumberString_File = GetVersionNumber();
            if (string.IsNullOrEmpty(versionNumberString_File))
                return false;

            int versionNumber_File;
            if (!int.TryParse(versionNumberString_File, out versionNumber_File))
                return false;

            int versionNumber_Revit;
            if (!int.TryParse(versionNumber, out versionNumber_Revit))
                return false;

            return versionNumber_Revit >= versionNumber_File;
        }

        public List<string> GetFamilyTypeNames()
        {
            if (xDocument == null || xDocument.Root == null || xDocument.Root.Attributes() == null)
                return null;

            List<XAttribute> xAttributes = xDocument.Root.Attributes().ToList();
            XAttribute xAttribute_First = xAttributes.Find(x => x.Name.LocalName == "A");
            XAttribute xAttribute_Second = xAttributes.Find(x => x.Name.LocalName == "xmlns");

            if (xAttribute_First != null && xAttribute_Second != null)
            {
                XName xName = XName.Get("family", xAttribute_First.Value);
                XElement xElement = xDocument.Root.Element(xName);
                if (xElement != null)
                {
                    xName = XName.Get("part", xAttribute_First.Value);
                    List<XElement> xElements = xElement.Elements(xName).ToList();
                    if (xElements != null)
                    {
                        List<string> result = new List<string>();
                        xName = XName.Get("title", xAttribute_Second.Value);
                        foreach (XElement xElement_Type in xElements)
                        {
                            XElement xElement_Title = xElement_Type.Element(xName);
                            if (xElement_Title != null && !string.IsNullOrEmpty(xElement_Title.Value))
                                result.Add(xElement_Title.Value);
                        }
                        return result;
                    }
                }
            }
            return null;
        }

        public List<RevitFilePreviewParameter> GetRevitFilePreviewParameters(string familyTypeName)
        {
            if (familyTypeName == null)
                return null;

            if (xDocument == null || xDocument.Root == null)
                return null;

            List<XAttribute> xAttributes = xDocument.Root.Attributes()?.ToList();
            if (xAttributes == null || xAttributes.Count == 0)
                return null;

            XAttribute xAttribute_First = xAttributes.Find(x => x.Name.LocalName == "A");
            if (xAttribute_First == null)
                return null;

            XAttribute xAttribute_Second = xAttributes.Find(x => x.Name.LocalName == "xmlns");
            if (xAttribute_Second == null)
                return null;

            XName xName = XName.Get("family", xAttribute_First.Value);
            XElement xElement = xDocument.Root.Element(xName);
            if (xElement == null)
                return null;

            xName = XName.Get("part", xAttribute_First.Value);
            List<XElement> xElements = xElement.Elements(xName).ToList();
            if (xElements == null || xElements.Count == 0)
                return null;

            List<RevitFilePreviewParameter> result = null;
            xName = XName.Get("title", xAttribute_Second.Value);
            foreach (XElement xElement_Type in xElements)
            {
                XElement xElement_Title = xElement_Type.Element(xName);
                if (xElement_Title == null)
                    continue;

                string familyTypeName_Temp = xElement_Title.Value;

                if (string.IsNullOrEmpty(familyTypeName_Temp))
                    continue;

                if (familyTypeName != familyTypeName_Temp)
                    continue;

                result = new List<RevitFilePreviewParameter>();
                List<XElement> xElements_Parameters = xElement_Type.Elements().ToList();
                if (xElements_Parameters == null || xElements_Parameters.Count == 0)
                    continue;

                foreach(XElement xElement_Parameter in xElements_Parameters)
                {
                    if (xElement_Parameter == null)
                        continue;

                    if (("title").Equals(xElement_Parameter?.Name?.LocalName))
                        continue;

                    RevitFilePreviewParameter revitFilePreviewParameter = Create.RevitFilePreviewParameter(xElement_Parameter);
                    if (revitFilePreviewParameter == null)
                        continue;

                    result.Add(revitFilePreviewParameter);

                }

                break;
            }
            return result;

        }

        public RevitFilePreviewParameter GetRevitFilePreviewParameter(string familyTypeName, string parameterName)
        {
            if (string.IsNullOrEmpty(familyTypeName) || string.IsNullOrEmpty(parameterName))
                return null;

            return GetRevitFilePreviewParameters(familyTypeName)?.Find(x => parameterName.Equals(x.NameOfParameter));
        }

        public string GetOmniClass()
        {
            if (xDocument != null && xDocument.Root != null)
            {
                List<XElement> xElements = xDocument.Root.Elements().ToList();
                if (xElements != null && xElements.Count > 0)
                {
                    XName xName = XName.Get("category", namespaceName);
                    xElements = xElements.FindAll(x => x.Name == xName);
                    if (xElements != null)
                        foreach (XElement xElement in xElements)
                        {
                            List<XElement> xElements_Child = xElement.Elements().ToList();
                            xName = XName.Get("scheme", namespaceName);

                            if (xElements_Child != null && xElements_Child.Find(x => x.Name == xName && x.Value == "std:oc1") != null)
                            {
                                xName = XName.Get("term", namespaceName);
                                XElement xElement_Child = xElements_Child.Find(x => x.Name == xName);
                                if (xElement_Child != null)
                                    return xElement_Child.Value;
                            }
                        }
                }
            }
            return null;
        }


        private void ReadStructuredStorageFile(StorageInfo storageInfo)
        {
            StreamInfo[] streamInfos = storageInfo.GetStreams();
            List<string> names = streamInfos.ToList().ConvertAll(x => x.Name);
            foreach (StreamInfo streamInfo in streamInfos)
            {
                if (streamInfo.Name.Equals("RevitPreview4.0"))
                    previewData = ParseStreamInfo(streamInfo);
                else if (streamInfo.Name.Equals("BasicFileInfo"))
                    fileInfoData = ParseStreamInfo(streamInfo);
                if (streamInfo.Name.Equals("PartAtom"))
                {
                    byte[] fileInfoData = ParseStreamInfo(streamInfo);
                    try
                    {
                        xDocument = XDocument.Parse(Encoding.UTF8.GetString(fileInfoData));
                    }
                    catch (Exception aException)
                    {
                        xDocument = null;
                    }

                    if (xDocument == null)
                    {
                        try
                        {
                            xDocument = XDocument.Parse(Encoding.Default.GetString(fileInfoData));
                        }
                        catch
                        {
                            xDocument = null;
                        }
                    }
                }
            }
        }

        private int GetPngStartingOffset()
        {
            bool markerFound = false;
            int startingOffset = 0;
            int previousValue = 0;
            using (MemoryStream ms = new MemoryStream(previewData))
            {
                for (int i = 0; i < previewData.Length; i++)
                {
                    int currentValue = ms.ReadByte();
                    // possible start of PNG file data
                    if (currentValue == 137)   // 0x89
                    {
                        markerFound = true;
                        startingOffset = i;
                        previousValue = currentValue;
                        continue;
                    }

                    switch (currentValue)
                    {
                        case 80:   // 0x50
                            if (markerFound && (previousValue == 137))
                            {
                                previousValue = currentValue;
                                continue;
                            }
                            markerFound = false;
                            break;

                        case 78:   // 0x4E
                            if (markerFound && (previousValue == 80))
                            {
                                previousValue = currentValue;
                                continue;
                            }
                            markerFound = false;
                            break;

                        case 71:   // 0x47
                            if (markerFound && (previousValue == 78))
                            {
                                previousValue = currentValue;
                                continue;
                            }
                            markerFound = false;
                            break;

                        case 13:   // 0x0D
                            if (markerFound && (previousValue == 71))
                            {
                                previousValue = currentValue;
                                continue;
                            }
                            markerFound = false;
                            break;

                        case 10:   // 0x0A
                            if (markerFound && (previousValue == 26))
                            {
                                return startingOffset;
                            }
                            if (markerFound && (previousValue == 13))
                            {
                                previousValue = currentValue;
                                continue;
                            }
                            markerFound = false;
                            break;

                        case 26:   // 0x1A
                            if (markerFound && (previousValue == 10))
                            {
                                previousValue = currentValue;
                                continue;
                            }
                            markerFound = false;
                            break;
                    }
                }
            }
            return 0;
        }

        private string[] GetFileInfo()
        {
            if ((fileInfoData != null) || (fileInfoData.Length > 0))
            {
                string value = Encoding.Default.GetString(fileInfoData).Replace("\0", string.Empty);
                if (value == null || value.Length == 0)
                    return null;

                return value.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            return null;
        }

        private Dictionary<string, string> GetParameterDictionary(params string[] parameterNames)
        {
            if (parameterNames == null)
                return null;

            string[] fileInfo = GetFileInfo();
            if (fileInfo == null)
                return null;

            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (string aLine in fileInfo)
            {
                if (string.IsNullOrEmpty(aLine))
                    continue;

                foreach (string parameterName in parameterNames)
                {
                    if (aLine.StartsWith(parameterName))
                        if (aLine.Length > parameterName.Length + 1)
                            result[parameterName] = aLine.Substring(parameterName.Length + 1).Trim();

                    if (result.Count == parameterNames.Length)
                        return result;
                }
            }

            return result;
        }


        private static void CloseStorageInfo(StorageInfo storageInfo)
        {
            InvokeStorageRootMethod(storageInfo, "Close");
        }

        private static byte[] ParseStreamInfo(StreamInfo streamInfo)
        {
            byte[] result = null;
            try
            {
                using (Stream stream = streamInfo.GetStream(FileMode.Open, FileAccess.Read))
                {
                    result = new byte[stream.Length];
                    stream.Read(result, 0, result.Length);
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                result = null;
            }
        }

        private static byte[] BitmapSourceToArray(BitmapSource bitmapSource)
        {
            BitmapEncoder bitmapEncoder = new JpegBitmapEncoder();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                bitmapEncoder.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static object InvokeStorageRootMethod(StorageInfo storageInfoRoot, string methodName, params object[] methodArgs)
        {
            BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            Type storageRootType = typeof(StorageInfo).Assembly.GetType("System.IO.Packaging.StorageRoot", true, false);
            object result = storageRootType.InvokeMember(methodName, bindingFlags, null, storageInfoRoot, methodArgs);
            return result;
        }
    }
}
