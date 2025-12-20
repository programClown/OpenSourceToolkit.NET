using OpenSourceToolkit.Converters;

namespace OpenSourceToolkit.NET.Services
{
    /// <summary>
    /// Abstraction for image processing operations.
    /// Wraps ImageProcessor to allow mocking in tests.
    /// </summary>
    public interface IImageProcessingService
    {
        /// <summary>
        /// Processes an image with the given options.
        /// </summary>
        byte[] ProcessImage(byte[] inputBytes, ImageProcessingOptions options);

        /// <summary>
        /// Converts image to PNG suitable for preview display.
        /// </summary>
        byte[] ConvertToPreviewPng(byte[] inputBytes);

        /// <summary>
        /// Converts image to PNG suitable for AI API (max 1024x1024).
        /// </summary>
        byte[] ConvertToAiPng(byte[] inputBytes);

        /// <summary>
        /// Creates a thumbnail from the input image.
        /// </summary>
        byte[] CreateThumbnail(byte[] inputBytes, int maxWidth, int maxHeight);
    }

    /// <summary>
    /// Production implementation wrapping ImageProcessor.
    /// </summary>
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly ImageProcessor _processor;

        public ImageProcessingService()
        {
            _processor = new ImageProcessor();
        }

        public ImageProcessingService(ImageProcessor processor)
        {
            _processor = processor;
        }

        public byte[] ProcessImage(byte[] inputBytes, ImageProcessingOptions options)
        {
            return _processor.ProcessImage(inputBytes, options);
        }

        public byte[] ConvertToPreviewPng(byte[] inputBytes)
        {
            return _processor.ConvertToPreviewPng(inputBytes);
        }

        public byte[] ConvertToAiPng(byte[] inputBytes)
        {
            return _processor.ConvertToAiPng(inputBytes);
        }

        public byte[] CreateThumbnail(byte[] inputBytes, int maxWidth, int maxHeight)
        {
            var options = new ImageProcessingOptions
            {
                Width = maxWidth,
                Height = maxHeight,
                MaintainAspectRatio = true,
                Format = "png"
            };
            return _processor.ProcessImage(inputBytes, options);
        }
    }
}
