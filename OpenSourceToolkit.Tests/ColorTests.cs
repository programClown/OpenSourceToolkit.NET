using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.Colors;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class ColorTests
    {
        [TestMethod]
        public void HexToRgb_Works()
        {
                var (r, g, b) = ColorConverter.HexToRgb("#FF0000");
            Assert.AreEqual(255, r);
            Assert.AreEqual(0, g);
            Assert.AreEqual(0, b);
        }

        [TestMethod]
        public void RgbToHex_Works()
        {
            var hex = ColorConverter.RgbToHex(0, 255, 0);
            Assert.AreEqual("#00FF00", hex);
        }

        [TestMethod]
        public void RgbToHsl_Works()
        {
            // Red
            var (h, s, l) = ColorConverter.RgbToHsl(255, 0, 0);
                Assert.AreEqual(0, h);
                Assert.AreEqual(1.0, s);
                Assert.AreEqual(0.5, l);
            }

            [TestMethod]
            public void Color_RoundTrip_RgbHexRgb_Works()
            {
                var original = (R: 12, G: 34, B: 56);
                var hex = ColorConverter.RgbToHex(original.R, original.G, original.B);
                var (r, g, b) = ColorConverter.HexToRgb(hex);

                Assert.AreEqual(original.R, r);
                Assert.AreEqual(original.G, g);
                Assert.AreEqual(original.B, b);
            }

            [TestMethod]
            public void RgbToHsl_BlackAndWhite_HaveExpectedLightness()
            {
                var (hBlack, sBlack, lBlack) = ColorConverter.RgbToHsl(0, 0, 0);
                var (hWhite, sWhite, lWhite) = ColorConverter.RgbToHsl(255, 255, 255);

                Assert.AreEqual(0.0, lBlack, 1e-10);
                Assert.AreEqual(1.0, lWhite, 1e-10);
                Assert.AreEqual(0.0, sBlack, 1e-10);
                Assert.AreEqual(0.0, sWhite, 1e-10);
        }
    }
}
