namespace OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter.Models
{
    /// <summary>
    /// Represents an image file in the batch conversion list.
    /// </summary>
    public class ImageFileModel
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public Avalonia.Media.Imaging.Bitmap Preview { get; set; }
        public long OriginalSize { get; set; }
        public string OriginalFormat { get; set; }

        // Dimensions info for defaulting resize
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }

        // Raw bytes for processing
        public byte[] RawBytes { get; set; }

        // Conversion result info
        public string ConvertedPath { get; set; }
        public long? ConvertedSize { get; set; }
        public string Status { get; set; } = "Pending";
        public string ErrorMessage { get; set; }

        public string SizeDisplay => FormatSize(OriginalSize);
        public string DimensionsDisplay => $"{OriginalWidth} x {OriginalHeight}";

        private static string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
