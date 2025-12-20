using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.Documents;
using PdfSharp.Pdf;
using PdfSharp.Fonts;
using System;
using System.IO;
using System.Reflection;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class DocumentTests
    {
        static DocumentTests()
        {
            // Ensure a font resolver is available for tests
            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new TestFontResolver();
            }
        }

        [TestMethod]
        public void PdfToolkit_CreateAndManipulate_VerifyFlow()
        {
            string tempFile1 = Path.GetTempFileName() + ".pdf";
            string tempFile2 = Path.GetTempFileName() + ".pdf";
            string outFile = Path.GetTempFileName() + ".pdf";

            try
            {
                // Create dummy PDFs
                CreateDummyPdf(tempFile1, "Page 1");
                CreateDummyPdf(tempFile2, "Page 2");

                // Merge
                PdfToolkit.MergePdfs(new[] { tempFile1, tempFile2 }, outFile);

                Assert.IsTrue(File.Exists(outFile));
                using (var doc = PdfSharp.Pdf.IO.PdfReader.Open(outFile, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import))
                {
                    Assert.AreEqual(2, doc.PageCount);
                }
            }
            finally
            {
                if (File.Exists(tempFile1)) File.Delete(tempFile1);
                if (File.Exists(tempFile2)) File.Delete(tempFile2);
                if (File.Exists(outFile)) File.Delete(outFile);
            }
        }

        [TestMethod]
        public void PdfToolkit_SplitPdf_CreatesOneFilePerPage()
        {
            string inputFile = Path.GetTempFileName() + ".pdf";
            string outputDir = Path.Combine(Path.GetTempPath(), "OST_Split_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(outputDir);

            try
            {
                CreateDummyPdf(inputFile, "Page 1");

                using (var doc = PdfSharp.Pdf.IO.PdfReader.Open(inputFile, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Modify))
                {
                    doc.AddPage();
                    doc.Save(inputFile);
                }

                PdfToolkit.SplitPdf(inputFile, outputDir);

                var files = Directory.GetFiles(outputDir, "page_*.pdf");
                Assert.AreEqual(2, files.Length);
            }
            finally
            {
                if (File.Exists(inputFile)) File.Delete(inputFile);
                if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
            }
        }

        [TestMethod]
        public void PdfToolkit_AddWatermark_DrawsText()
        {
            string inputFile = Path.GetTempFileName() + ".pdf";
            string outputFile = Path.GetTempFileName() + ".pdf";

            try
            {
                CreateDummyPdf(inputFile, "Content");
                PdfToolkit.AddWatermark(inputFile, outputFile, "DRAFT");

                Assert.IsTrue(File.Exists(outputFile));
            }
            finally
            {
                if (File.Exists(inputFile)) File.Delete(inputFile);
                if (File.Exists(outputFile)) File.Delete(outputFile);
            }
        }

        private void CreateDummyPdf(string path, string text)
        {
            using (var doc = new PdfDocument())
            {
                var page = doc.AddPage();
                using (var gfx = PdfSharp.Drawing.XGraphics.FromPdfPage(page))
                {
                    // Draw a rectangle
                    gfx.DrawRectangle(PdfSharp.Drawing.XBrushes.Black, 10, 10, 100, 50);

                    // Draw text using the resolver
                    var font = new PdfSharp.Drawing.XFont("Roboto", 12);
                    gfx.DrawString(text, font, PdfSharp.Drawing.XBrushes.Black, 20, 30);
                }
                doc.Save(path);
            }
        }
    }

    public class TestFontResolver : IFontResolver
    {
        private byte[] _fontData;

        public TestFontResolver()
        {
            // Load the font immediately or lazily
            string fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fonts", "Roboto-Regular.ttf");
            if (File.Exists(fontPath))
            {
                _fontData = File.ReadAllBytes(fontPath);
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            // Map any request to our loaded font family "Roboto"
            // In a real scenario, you'd match the familyName
            return new FontResolverInfo("Roboto");
        }

        public byte[] GetFont(string faceName)
        {
            return _fontData;
        }
    }
}
