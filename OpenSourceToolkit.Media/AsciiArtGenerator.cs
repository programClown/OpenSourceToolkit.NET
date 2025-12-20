using System;
using System.IO;
using System.Text;
using SkiaSharp;

namespace OpenSourceToolkit.Media
{
    public class AsciiArtGenerator
    {
        private static readonly string[] Density = { "@", "#", "S", "%", "?", "*", "+", ";", ":", ",", "." };

        public static string ConvertImageToAscii(string imagePath, int width = 100)
        {
            using var bitmap = SKBitmap.Decode(imagePath);
            if (bitmap == null)
                throw new ArgumentException($"Failed to load image: {imagePath}");
            
            return ConvertBitmapToAscii(bitmap, width);
        }

        public static string ConvertBitmapToAscii(SKBitmap bitmap, int width = 100)
        {
            var height = (int)(bitmap.Height * ((double)width / bitmap.Width) * 0.55); // 0.55 accounts for char aspect ratio

            using var resized = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
            if (resized == null)
                throw new InvalidOperationException("Failed to resize image");

            var sb = new StringBuilder();
            for (int y = 0; y < resized.Height; y++)
            {
                for (int x = 0; x < resized.Width; x++)
                {
                    var pixel = resized.GetPixel(x, y);
                    var brightness = (int)(pixel.Red * 0.3 + pixel.Green * 0.59 + pixel.Blue * 0.11);
                    var index = (int)(brightness / 255.0 * (Density.Length - 1));
                    sb.Append(Density[Density.Length - 1 - index]);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string ConvertTextToAscii(string text, string flfFontPath)
        {
            if (!File.Exists(flfFontPath))
            {
                throw new FileNotFoundException($"FIGlet font file not found: {flfFontPath}");
            }

            // FIGlet integration pending - namespace resolution issues with the library
            throw new NotImplementedException("FIGlet integration is pending correct namespace resolution.");
        }
    }
}
