using Autodesk.Revit.DB;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Collections.Generic;

namespace SAM.Core.Pdf.Revit
{
    public static partial class Convert
    {
        public static bool ToPdf(this Document document, IEnumerable<ElementId> viewElementIds, string path, PdfSharp.PageSize pageSize = PdfSharp.PageSize.A4, ImageResolution imageResolution = ImageResolution.DPI_600)
        {
            if(document == null || viewElementIds == null || string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if(!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(path)))
            {
                return false;
            }

            //Creating directory for temporary images
            string directory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
            System.IO.Directory.CreateDirectory(directory);

            string name = string.IsNullOrEmpty(document.Title) ? "???" : document.Title;

            //Adjusting image pixel size
            int pixelSize = 512;
            switch (pageSize)
            {
                case PdfSharp.PageSize.A0:
                    pixelSize = 14043;
                    break;

                case PdfSharp.PageSize.A1:
                    pixelSize = 9933;
                    break;

                case PdfSharp.PageSize.A2:
                    pixelSize = 7016;
                    break;

                case PdfSharp.PageSize.A3:
                    pixelSize = 4961;
                    break;

                case PdfSharp.PageSize.A4:
                    pixelSize = 3508;
                    break;

                case PdfSharp.PageSize.A5:
                    pixelSize = 2480;
                    break;
            }

            //Adjusting image resolution
            switch (imageResolution)
            {
                case ImageResolution.DPI_600:
                    pixelSize = pixelSize * 2;
                    break;

                case ImageResolution.DPI_300:
                    pixelSize = pixelSize * 1;
                    break;

                case ImageResolution.DPI_150:
                    pixelSize = System.Convert.ToInt32(pixelSize * 0.5);
                    break;

                case ImageResolution.DPI_72:
                    pixelSize = System.Convert.ToInt32(pixelSize * 0.25);
                    break;
            }

            
            //Creating Revit Export Options for View Image
            ImageExportOptions imageExportOptions = new ImageExportOptions()
            {

                FilePath = System.IO.Path.Combine(directory, name),
                FitDirection = FitDirectionType.Horizontal,
                ZoomType = ZoomFitType.FitToPage,
                ImageResolution = imageResolution,
                HLRandWFViewsFileType = ImageFileType.PNG,
                ShadowViewsFileType = ImageFileType.PNG,
                PixelSize = pixelSize,
                ExportRange = ExportRange.SetOfViews
            };


            //Exporting temporary images from Views. Necessary to do it one by one to keep view order
            List<string> imagePaths = new List<string>();
            foreach(ElementId elementId in viewElementIds)
            {
                if(elementId == null)
                {
                    continue;
                }

                View view = document.GetElement(elementId) as View;
                if(view == null)
                {
                    continue;
                }

                imageExportOptions.SetViewsAndSheets(new List<ElementId>() { elementId });

                document.ExportImage(imageExportOptions);

                foreach(string imagePath in System.IO.Directory.GetFiles(directory))
                {
                    if(!imagePaths.Contains(imagePath))
                    {
                        imagePaths.Add(imagePath);
                        break;
                    }
                }
            }

            //Creating pdf Document from view images
            using (PdfDocument pdfDocument = new PdfDocument())
            {
                foreach (string imagePath in imagePaths)
                {
                    PdfPage pdfPage = pdfDocument.AddPage();
                    pdfPage.Size = pageSize;
                    
                    byte[] bytes = System.IO.File.ReadAllBytes(imagePath);
                    using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes, false))
                    {
                        double widthFactor = 1;
                        double heightFactor = 1;

                        XImage xImage = XImage.FromStream(memoryStream);
                        if (xImage.PointHeight > xImage.PointWidth)
                        {
                            pdfPage.Orientation = PdfSharp.PageOrientation.Portrait;
                            widthFactor = xImage.PointWidth / pdfPage.Width.Point;
                        }
                        else
                        {
                            pdfPage.Orientation = PdfSharp.PageOrientation.Landscape;
                            heightFactor = xImage.PointHeight / pdfPage.Height.Point;
                        }

                        XGraphics xGraphics = XGraphics.FromPdfPage(pdfPage);
                        xGraphics.DrawImage(xImage, 0, 0, pdfPage.Width.Point * widthFactor, pdfPage.Height.Point * heightFactor);
                    }
                }

                pdfDocument.Save(path);
            }

            //Removing temporary image files -> Not necessary
            foreach (string imagePath in imagePaths)
            {
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            //Recursive removing whole temporary directory where view images were saved
            System.IO.Directory.Delete(directory, true);

            return System.IO.File.Exists(path);
        }
    }
}