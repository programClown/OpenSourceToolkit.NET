using System;
using System.Drawing;

namespace OpenSourceToolkit.Colors
{
    /// <summary>
    /// Provides color conversion helpers for common color formats and color spaces.
    /// </summary>
    public static class ColorConverter
    {
        /// <summary>
        /// Converts RGB color components to a hexadecimal color string.
        /// </summary>
        /// <param name="r">The red component from 0 to 255.</param>
        /// <param name="g">The green component from 0 to 255.</param>
        /// <param name="b">The blue component from 0 to 255.</param>
        /// <returns>A hexadecimal color string in the format #RRGGBB.</returns>
        public static string RgbToHex(int r, int g, int b)
        {
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        /// <summary>
        /// Converts a hexadecimal color string to RGB color components.
        /// </summary>
        /// <param name="hex">The hexadecimal color string to convert.</param>
        /// <returns>The red, green, and blue components, or zeros when conversion fails.</returns>
        public static (int R, int G, int B) HexToRgb(string hex)
        {
            try
            {
                var color = ColorTranslator.FromHtml(hex);
                return (color.R, color.G, color.B);
            }
            catch
            {
                return (0, 0, 0);
            }
        }

        /// <summary>
        /// Converts RGB color components to HSL color components.
        /// </summary>
        /// <param name="r">The red component from 0 to 255.</param>
        /// <param name="g">The green component from 0 to 255.</param>
        /// <param name="b">The blue component from 0 to 255.</param>
        /// <returns>The hue in degrees, saturation from 0 to 1, and lightness from 0 to 1.</returns>
        public static (double H, double S, double L) RgbToHsl(int r, int g, int b)
        {
             double rd = r / 255.0;
             double gd = g / 255.0;
             double bd = b / 255.0;

             double max = Math.Max(rd, Math.Max(gd, bd));
             double min = Math.Min(rd, Math.Min(gd, bd));

             double h = 0, s = 0, l = (max + min) / 2.0;

             if (max != min)
             {
                 double d = max - min;
                 s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

                 if (max == rd)
                 {
                     h = (gd - bd) / d + (gd < bd ? 6.0 : 0.0);
                 }
                 else if (max == gd)
                 {
                     h = (bd - rd) / d + 2.0;
                 }
                 else
                 {
                     h = (rd - gd) / d + 4.0;
                 }

                 h /= 6.0;
             }

             return (h * 360.0, s, l);
        }

        /// <summary>
        /// Converts HSL color components to RGB color components.
        /// </summary>
        /// <param name="h">The hue in degrees.</param>
        /// <param name="s">The saturation from 0 to 1.</param>
        /// <param name="l">The lightness from 0 to 1.</param>
        /// <returns>The red, green, and blue components.</returns>
        public static (int R, int G, int B) HslToRgb(double h, double s, double l)
        {
            // h: 0-360, s: 0-1, l: 0-1
            h /= 360.0;
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l; // achromatic
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = Hue2Rgb(p, q, h + 1.0/3.0);
                g = Hue2Rgb(p, q, h);
                b = Hue2Rgb(p, q, h - 1.0/3.0);
            }

            return ((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        private static double Hue2Rgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0/6.0) return p + (q - p) * 6.0 * t;
            if (t < 1.0/2.0) return q;
            if (t < 2.0/3.0) return p + (q - p) * (2.0/3.0 - t) * 6.0;
            return p;
        }

        /// <summary>
        /// Converts RGB color components to HSV color components.
        /// </summary>
        /// <param name="r">The red component from 0 to 255.</param>
        /// <param name="g">The green component from 0 to 255.</param>
        /// <param name="b">The blue component from 0 to 255.</param>
        /// <returns>The hue in degrees, saturation from 0 to 1, and value from 0 to 1.</returns>
        public static (double H, double S, double V) RgbToHsv(int r, int g, int b)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;
            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double h = 0, s = 0, v = max;

            double d = max - min;
            s = max == 0 ? 0 : d / max;

            if (max != min)
            {
                if (max == rd)
                    h = (gd - bd) / d + (gd < bd ? 6.0 : 0.0);
                else if (max == gd)
                    h = (bd - rd) / d + 2.0;
                else
                    h = (rd - gd) / d + 4.0;
                h /= 6.0;
            }

            return (h * 360.0, s, v);
        }

        /// <summary>
        /// Converts RGB color components to CMYK color components.
        /// </summary>
        /// <param name="r">The red component from 0 to 255.</param>
        /// <param name="g">The green component from 0 to 255.</param>
        /// <param name="b">The blue component from 0 to 255.</param>
        /// <returns>The cyan, magenta, yellow, and key components as percentages.</returns>
        public static (int C, int M, int Y, int K) RgbToCmyk(int r, int g, int b)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double k = 1.0 - Math.Max(rd, Math.Max(gd, bd));
            if (k == 1.0) return (0, 0, 0, 100);

            double c = (1.0 - rd - k) / (1.0 - k);
            double m = (1.0 - gd - k) / (1.0 - k);
            double y = (1.0 - bd - k) / (1.0 - k);

            return ((int)(c * 100), (int)(m * 100), (int)(y * 100), (int)(k * 100));
        }

        /// <summary>
        /// Converts RGB color components to CIE L*a*b* color components.
        /// </summary>
        /// <param name="r">The red component from 0 to 255.</param>
        /// <param name="g">The green component from 0 to 255.</param>
        /// <param name="b">The blue component from 0 to 255.</param>
        /// <returns>The lightness, a-axis, and b-axis components.</returns>
        public static (double L, double A, double B) RgbToLab(int r, int g, int b)
        {
            double R = r / 255.0;
            double G = g / 255.0;
            double B = b / 255.0;

            R = R > 0.04045 ? Math.Pow((R + 0.055) / 1.055, 2.4) : R / 12.92;
            G = G > 0.04045 ? Math.Pow((G + 0.055) / 1.055, 2.4) : G / 12.92;
            B = B > 0.04045 ? Math.Pow((B + 0.055) / 1.055, 2.4) : B / 12.92;

            double x = (R * 0.4124 + G * 0.3576 + B * 0.1805) / 0.95047;
            double y = (R * 0.2126 + G * 0.7152 + B * 0.0722) / 1.00000;
            double z = (R * 0.0193 + G * 0.1192 + B * 0.9505) / 1.08883;

            x = x > 0.008856 ? Math.Pow(x, 1.0 / 3.0) : (7.787 * x) + (16.0 / 116.0);
            y = y > 0.008856 ? Math.Pow(y, 1.0 / 3.0) : (7.787 * y) + (16.0 / 116.0);
            z = z > 0.008856 ? Math.Pow(z, 1.0 / 3.0) : (7.787 * z) + (16.0 / 116.0);

            return ((116.0 * y) - 16.0, 500.0 * (x - y), 200.0 * (y - z));
        }
    }
}
