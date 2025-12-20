using System;
using System.IO;
using ImageMagick;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.Converters;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class ImageProcessorTests
    {
        private ImageProcessor _processor;
        private byte[] _testPngBytes;

        [TestInitialize]
        public void Setup()
        {
            _processor = new ImageProcessor();
            // Create a 100x100 red PNG
            using (var image = new MagickImage(MagickColors.Red, 100, 100))
            {
                image.Format = MagickFormat.Png;
                _testPngBytes = image.ToByteArray();
            }
        }

        [TestMethod]
        public void ProcessImage_ConvertPngToJpg_Success()
        {
            var result = _processor.ProcessImage(_testPngBytes, null, null, true, "jpg");

            using (var image = new MagickImage(result))
            {
                Assert.AreEqual(MagickFormat.Jpeg, image.Format);
                Assert.AreEqual(100u, image.Width);
                Assert.AreEqual(100u, image.Height);
            }
        }

        [TestMethod]
        public void ProcessImage_Resize_Works()
        {
            // Resize to 50x50
            var result = _processor.ProcessImage(_testPngBytes, 50, 50, false, "png");

            using (var image = new MagickImage(result))
            {
                Assert.AreEqual(50u, image.Width);
                Assert.AreEqual(50u, image.Height);
            }
        }

        [TestMethod]
        public void ProcessImage_MaintainAspectRatio_Works()
        {
            // Resize to 50x? (should be 50x50 since original is square)
            var result = _processor.ProcessImage(_testPngBytes, 50, null, true, "png");

            using (var image = new MagickImage(result))
            {
                Assert.AreEqual(50u, image.Width);
                Assert.AreEqual(50u, image.Height);
            }
        }

        [TestMethod]
        public void ProcessImage_ConvertToWebP_Works()
        {
            var result = _processor.ProcessImage(_testPngBytes, null, null, true, "webp");

            using (var image = new MagickImage(result))
            {
                Assert.AreEqual(MagickFormat.WebP, image.Format);
            }
        }

        [TestMethod]
        public void ProcessImage_ConvertToIco_Works()
        {
            // ICO usually needs specific sizes, but Magick handles it
            var result = _processor.ProcessImage(_testPngBytes, 32, 32, true, "ico");

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);

            // Verify it is readable as ICO
            // Sometimes auto-detection from stream needs a hint or the container structure varies
            var settings = new MagickReadSettings { Format = MagickFormat.Ico };
            using (var image = new MagickImage(result, settings))
            {
                Assert.AreEqual(MagickFormat.Ico, image.Format);
                Assert.AreEqual(32u, image.Width);
            }
        }

        [TestMethod]
        public void ProcessImage_TransparencyFlattening_Works()
        {
            // Create transparent image
            byte[] transparentBytes;
            using (var img = new MagickImage(MagickColors.Transparent, 100, 100))
            {
                img.Format = MagickFormat.Png;
                transparentBytes = img.ToByteArray();
            }

            // Convert to JPG (should flatten to white)
            var result = _processor.ProcessImage(transparentBytes, null, null, true, "jpg");

            using (var image = new MagickImage(result))
            {
                Assert.AreEqual(MagickFormat.Jpeg, image.Format);
                // Check pixel color - should be white (approx)
                using (var pixels = image.GetPixels())
                {
                    var color = pixels.GetPixel(0, 0).ToColor();
                    // Jpeg compression might make it slightly off pure white, but should be very bright
                    Assert.IsTrue(color.R > 250);
                    Assert.IsTrue(color.G > 250);
                    Assert.IsTrue(color.B > 250);
                }
            }
        }
    }
}
