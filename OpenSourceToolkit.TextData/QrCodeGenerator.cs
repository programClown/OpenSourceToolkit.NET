using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.Versioning;

namespace OpenSourceToolkit.TextData
{
    public enum QrEccLevel
    {
        L = 1, // 7%
        M = 2, // 15%
        Q = 3, // 25%
        H = 4  // 30%
    }

    public class QrCodeGenerator
    {
        /// <summary>
        /// Returns true if PNG QR code generation is supported on the current platform (Windows only).
        /// </summary>
        public static bool IsPngSupported => OperatingSystem.IsWindows();

        [SupportedOSPlatform("windows")]
        public static byte[] GeneratePng(string text, int pixelsPerModule = 20, QrEccLevel eccLevel = QrEccLevel.Q)
        {
            if (!OperatingSystem.IsWindows())
                throw new PlatformNotSupportedException("PNG QR code generation is only supported on Windows.");

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(text, (QRCodeGenerator.ECCLevel)eccLevel))
            using (var qrCode = new QRCode(qrCodeData))
            using (var bitmap = qrCode.GetGraphic(pixelsPerModule))
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static string GenerateSvg(string text, int pixelsPerModule = 20, QrEccLevel eccLevel = QrEccLevel.Q)
        {
            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(text, (QRCodeGenerator.ECCLevel)eccLevel))
            using (var svgQrCode = new SvgQRCode(qrCodeData))
            {
                return svgQrCode.GetGraphic(pixelsPerModule);
            }
        }
    }
}
