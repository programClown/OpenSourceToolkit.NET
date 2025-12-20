using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace OpenSourceToolkit.Converters
{
    public static class ImageConverter
    {
        public static void Convert(string inputPath, string outputPath, ImageFormat format)
        {
            using (var image = Image.FromFile(inputPath))
            {
                image.Save(outputPath, format);
            }
        }

        public static byte[] Convert(byte[] inputBytes, ImageFormat format)
        {
            using (var inputStream = new MemoryStream(inputBytes))
            using (var image = Image.FromStream(inputStream))
            using (var outputStream = new MemoryStream())
            {
                image.Save(outputStream, format);
                return outputStream.ToArray();
            }
        }
    }
}
