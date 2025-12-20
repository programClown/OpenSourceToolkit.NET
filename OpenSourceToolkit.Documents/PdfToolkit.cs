using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using System;
using System.IO;
using System.Collections.Generic;

namespace OpenSourceToolkit.Documents
{
    public class PdfToolkit
    {
        public static void MergePdfs(IEnumerable<string> inputFiles, string outputFile)
        {
            using (var outputDocument = new PdfDocument())
            {
                foreach (var file in inputFiles)
                {
                    using (var inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import))
                    {
                        int count = inputDocument.PageCount;
                        for (int idx = 0; idx < count; idx++)
                        {
                            var page = inputDocument.Pages[idx];
                            outputDocument.AddPage(page);
                        }
                    }
                }
                outputDocument.Save(outputFile);
            }
        }

        public static void SplitPdf(string inputFile, string outputDirectory)
        {
            using (var inputDocument = PdfReader.Open(inputFile, PdfDocumentOpenMode.Import))
            {
                for (int idx = 0; idx < inputDocument.PageCount; idx++)
                {
                    using (var outputDocument = new PdfDocument())
                    {
                        outputDocument.AddPage(inputDocument.Pages[idx]);
                        outputDocument.Save(Path.Combine(outputDirectory, $"page_{idx + 1}.pdf"));
                    }
                }
            }
        }

        /// <summary>
        /// Adds a text watermark with default options (for backwards compatibility).
        /// </summary>
        public static void AddWatermark(string inputFile, string outputFile, string watermarkText)
        {
            var options = new PdfWatermarkOptions { Text = watermarkText };
            AddWatermark(inputFile, outputFile, options);
        }

        /// <summary>
        /// Adds a text watermark with full customization options.
        /// </summary>
        public static void AddWatermark(string inputFile, string outputFile, PdfWatermarkOptions options)
        {
            using (var document = PdfReader.Open(inputFile, PdfDocumentOpenMode.Modify))
            {
                // Build font style
                XFontStyleEx fontStyle = XFontStyleEx.Regular;
                if (options.IsBold && options.IsItalic)
                    fontStyle = XFontStyleEx.BoldItalic;
                else if (options.IsBold)
                    fontStyle = XFontStyleEx.Bold;
                else if (options.IsItalic)
                    fontStyle = XFontStyleEx.Italic;

                var font = new XFont(options.FontFamily, options.FontSize, fontStyle);
                
                // Parse color and apply opacity
                var color = ParseColor(options.Color, options.Opacity);
                var brush = new XSolidBrush(color);

                for (int idx = 0; idx < document.PageCount; idx++)
                {
                    var page = document.Pages[idx];
                    using (var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append))
                    {
                        var size = gfx.MeasureString(options.Text, font);
                        double pageWidth = page.Width.Point;
                        double pageHeight = page.Height.Point;

                        // Calculate position based on watermark position setting
                        var (x, y) = CalculateWatermarkPosition(
                            pageWidth, pageHeight,
                            size.Width, size.Height,
                            options.Position, options.Padding);

                        // Save graphics state
                        var state = gfx.Save();

                        // Move to position, rotate, and draw
                        gfx.TranslateTransform(x, y);
                        gfx.RotateTransform(options.Rotation);
                        gfx.DrawString(options.Text, font, brush, new XPoint(-size.Width / 2, size.Height / 4));

                        // Restore graphics state
                        gfx.Restore(state);
                    }
                }
                document.Save(outputFile);
            }
        }

        private static (double x, double y) CalculateWatermarkPosition(
            double pageWidth, double pageHeight,
            double wmWidth, double wmHeight,
            PdfWatermarkPosition position, int padding)
        {
            double x, y;

            switch (position)
            {
                case PdfWatermarkPosition.TopLeft:
                    x = padding + wmWidth / 2;
                    y = padding + wmHeight / 2;
                    break;
                case PdfWatermarkPosition.TopCenter:
                    x = pageWidth / 2;
                    y = padding + wmHeight / 2;
                    break;
                case PdfWatermarkPosition.TopRight:
                    x = pageWidth - padding - wmWidth / 2;
                    y = padding + wmHeight / 2;
                    break;
                case PdfWatermarkPosition.MiddleLeft:
                    x = padding + wmWidth / 2;
                    y = pageHeight / 2;
                    break;
                case PdfWatermarkPosition.MiddleCenter:
                    x = pageWidth / 2;
                    y = pageHeight / 2;
                    break;
                case PdfWatermarkPosition.MiddleRight:
                    x = pageWidth - padding - wmWidth / 2;
                    y = pageHeight / 2;
                    break;
                case PdfWatermarkPosition.BottomLeft:
                    x = padding + wmWidth / 2;
                    y = pageHeight - padding - wmHeight / 2;
                    break;
                case PdfWatermarkPosition.BottomCenter:
                    x = pageWidth / 2;
                    y = pageHeight - padding - wmHeight / 2;
                    break;
                case PdfWatermarkPosition.BottomRight:
                    x = pageWidth - padding - wmWidth / 2;
                    y = pageHeight - padding - wmHeight / 2;
                    break;
                default:
                    x = pageWidth / 2;
                    y = pageHeight / 2;
                    break;
            }

            return (x, y);
        }

        private static XColor ParseColor(string hexColor, int opacity)
        {
            // Default to red if parsing fails
            byte r = 255, g = 0, b = 0;

            if (!string.IsNullOrEmpty(hexColor))
            {
                string hex = hexColor.TrimStart('#');
                if (hex.Length == 6)
                {
                    r = Convert.ToByte(hex.Substring(0, 2), 16);
                    g = Convert.ToByte(hex.Substring(2, 2), 16);
                    b = Convert.ToByte(hex.Substring(4, 2), 16);
                }
            }

            // Convert opacity 0-100 to alpha 0-255
            int alpha = (int)(opacity * 2.55);
            return XColor.FromArgb(alpha, r, g, b);
        }
    }
}
