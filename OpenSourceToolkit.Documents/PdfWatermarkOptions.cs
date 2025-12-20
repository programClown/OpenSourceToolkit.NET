namespace OpenSourceToolkit.Documents
{
    /// <summary>
    /// Position options for PDF watermarks.
    /// </summary>
    public enum PdfWatermarkPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    /// <summary>
    /// Configuration options for PDF text watermarks.
    /// </summary>
    public class PdfWatermarkOptions
    {
        /// <summary>
        /// The watermark text to display.
        /// </summary>
        public string Text { get; set; } = "DRAFT";

        /// <summary>
        /// Font size in points. Default is 48.
        /// </summary>
        public double FontSize { get; set; } = 48;

        /// <summary>
        /// Rotation angle in degrees. Default is -45 (diagonal).
        /// Range: -180 to 180.
        /// </summary>
        public double Rotation { get; set; } = -45;

        /// <summary>
        /// Opacity from 0 (invisible) to 100 (fully opaque). Default is 30.
        /// </summary>
        public int Opacity { get; set; } = 30;

        /// <summary>
        /// Watermark position on the page. Default is MiddleCenter.
        /// </summary>
        public PdfWatermarkPosition Position { get; set; } = PdfWatermarkPosition.MiddleCenter;

        /// <summary>
        /// Text color in hex format (e.g., "#FF0000" for red). Default is red.
        /// </summary>
        public string Color { get; set; } = "#FF0000";

        /// <summary>
        /// Padding from page edges in points. Default is 20.
        /// </summary>
        public int Padding { get; set; } = 20;

        /// <summary>
        /// Font family name. Default is "Arial".
        /// </summary>
        public string FontFamily { get; set; } = "Arial";

        /// <summary>
        /// Whether to use bold font style. Default is false.
        /// </summary>
        public bool IsBold { get; set; } = false;

        /// <summary>
        /// Whether to use italic font style. Default is false.
        /// </summary>
        public bool IsItalic { get; set; } = false;
    }
}
