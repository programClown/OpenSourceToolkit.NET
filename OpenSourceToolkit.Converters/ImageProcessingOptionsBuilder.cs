namespace OpenSourceToolkit.Converters
{
    /// <summary>
    /// Fluent builder for ImageProcessingOptions.
    /// Provides a clean API for constructing options with method chaining.
    /// </summary>
    public class ImageProcessingOptionsBuilder
    {
        private readonly ImageProcessingOptions _options = new ImageProcessingOptions();

        // ═══════════════════════════════════════════════════════════════════════════
        // Output Settings
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithFormat(string format)
        {
            _options.Format = format;
            return this;
        }

        public ImageProcessingOptionsBuilder WithQuality(int quality)
        {
            _options.Quality = quality;
            return this;
        }

        public ImageProcessingOptionsBuilder WithResize(int? width, int? height, bool maintainAspectRatio = true)
        {
            _options.Width = width;
            _options.Height = height;
            _options.MaintainAspectRatio = maintainAspectRatio;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Adjustments
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithBrightness(int brightness)
        {
            _options.Brightness = brightness;
            return this;
        }

        public ImageProcessingOptionsBuilder WithContrast(int contrast)
        {
            _options.Contrast = contrast;
            return this;
        }

        public ImageProcessingOptionsBuilder WithSaturation(int saturation)
        {
            _options.Saturation = saturation;
            return this;
        }

        public ImageProcessingOptionsBuilder WithAdjustments(int brightness, int contrast, int saturation)
        {
            _options.Brightness = brightness;
            _options.Contrast = contrast;
            _options.Saturation = saturation;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Filters
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithGrayscale(bool enabled = true)
        {
            _options.Grayscale = enabled;
            return this;
        }

        public ImageProcessingOptionsBuilder WithSepia(bool enabled = true)
        {
            _options.Sepia = enabled;
            return this;
        }

        public ImageProcessingOptionsBuilder WithInvert(bool enabled = true)
        {
            _options.Invert = enabled;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Blur / Sharpen
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithBlur(int radius)
        {
            _options.BlurRadius = radius;
            return this;
        }

        public ImageProcessingOptionsBuilder WithSharpen(int amount)
        {
            _options.SharpenAmount = amount;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Transform
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithRotation(int angle)
        {
            _options.RotationAngle = angle;
            return this;
        }

        public ImageProcessingOptionsBuilder WithFlipHorizontal(bool enabled = true)
        {
            _options.FlipHorizontal = enabled;
            return this;
        }

        public ImageProcessingOptionsBuilder WithFlipVertical(bool enabled = true)
        {
            _options.FlipVertical = enabled;
            return this;
        }

        public ImageProcessingOptionsBuilder WithTransform(int rotationAngle, bool flipHorizontal, bool flipVertical)
        {
            _options.RotationAngle = rotationAngle;
            _options.FlipHorizontal = flipHorizontal;
            _options.FlipVertical = flipVertical;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Crop
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithCrop(int x, int y, int width, int height)
        {
            _options.CropEnabled = true;
            _options.CropX = x;
            _options.CropY = y;
            _options.CropWidth = width;
            _options.CropHeight = height;
            return this;
        }

        public ImageProcessingOptionsBuilder WithCropDisabled()
        {
            _options.CropEnabled = false;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Watermark
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithTextWatermark(
            string text,
            WatermarkPosition position = WatermarkPosition.BottomRight,
            int opacity = 50,
            int fontSize = 24,
            string color = "#FFFFFF",
            int padding = 10)
        {
            _options.WatermarkEnabled = true;
            _options.WatermarkText = text;
            _options.WatermarkImageBytes = null;
            _options.WatermarkPosition = position;
            _options.WatermarkOpacity = opacity;
            _options.WatermarkFontSize = fontSize;
            _options.WatermarkColor = color;
            _options.WatermarkPadding = padding;
            return this;
        }

        public ImageProcessingOptionsBuilder WithImageWatermark(
            byte[] imageBytes,
            WatermarkPosition position = WatermarkPosition.BottomRight,
            int opacity = 50,
            int padding = 10)
        {
            _options.WatermarkEnabled = true;
            _options.WatermarkText = null;
            _options.WatermarkImageBytes = imageBytes;
            _options.WatermarkPosition = position;
            _options.WatermarkOpacity = opacity;
            _options.WatermarkPadding = padding;
            return this;
        }

        public ImageProcessingOptionsBuilder WithWatermarkDisabled()
        {
            _options.WatermarkEnabled = false;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Phase 3 Effects
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithAutoEnhance(bool enabled = true)
        {
            _options.AutoEnhance = enabled;
            return this;
        }

        public ImageProcessingOptionsBuilder WithVignette(int radius = 50, int softness = 50)
        {
            _options.Vignette = true;
            _options.VignetteRadius = radius;
            _options.VignetteSoftness = softness;
            return this;
        }

        public ImageProcessingOptionsBuilder WithVignetteDisabled()
        {
            _options.Vignette = false;
            return this;
        }

        public ImageProcessingOptionsBuilder WithPosterize(int levels = 4)
        {
            _options.Posterize = true;
            _options.PosterizeLevels = levels;
            return this;
        }

        public ImageProcessingOptionsBuilder WithPosterizeDisabled()
        {
            _options.Posterize = false;
            return this;
        }

        public ImageProcessingOptionsBuilder WithEdgeDetect(int radius = 1)
        {
            _options.EdgeDetect = true;
            _options.EdgeDetectRadius = radius;
            return this;
        }

        public ImageProcessingOptionsBuilder WithEdgeDetectDisabled()
        {
            _options.EdgeDetect = false;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Background Removal
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithBackgroundRemoval(string backgroundColor = "transparent", int tolerance = 10)
        {
            _options.RemoveBackground = true;
            _options.BackgroundColor = backgroundColor;
            _options.BackgroundTolerance = tolerance;
            return this;
        }

        public ImageProcessingOptionsBuilder WithBackgroundRemovalDisabled()
        {
            _options.RemoveBackground = false;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Metadata
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithStripMetadata(bool enabled = true)
        {
            _options.StripMetadata = enabled;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ICO Multi-size
        // ═══════════════════════════════════════════════════════════════════════════

        public ImageProcessingOptionsBuilder WithMultiSizeIco(int[] sizes = null)
        {
            _options.GenerateMultiSizeIco = true;
            if (sizes != null)
            {
                _options.IcoSizes = sizes;
            }
            return this;
        }

        public ImageProcessingOptionsBuilder WithMultiSizeIcoDisabled()
        {
            _options.GenerateMultiSizeIco = false;
            return this;
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Build
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Builds and returns the configured ImageProcessingOptions.
        /// </summary>
        public ImageProcessingOptions Build()
        {
            return _options;
        }

        /// <summary>
        /// Creates a new builder instance.
        /// </summary>
        public static ImageProcessingOptionsBuilder Create()
        {
            return new ImageProcessingOptionsBuilder();
        }

        /// <summary>
        /// Creates a builder pre-configured for preview (PNG format, no resize/output settings).
        /// </summary>
        public static ImageProcessingOptionsBuilder ForPreview()
        {
            return new ImageProcessingOptionsBuilder().WithFormat("png");
        }

        /// <summary>
        /// Creates a builder pre-configured for batch conversion (format and resize only).
        /// </summary>
        public static ImageProcessingOptionsBuilder ForBatch(string format, int quality = 90)
        {
            return new ImageProcessingOptionsBuilder()
                .WithFormat(format)
                .WithQuality(quality);
        }

        /// <summary>
        /// Creates options for single image editing with all effect parameters.
        /// </summary>
        /// <param name="format">Output format (e.g., "png", "jpg")</param>
        /// <param name="includeResizeAndOutput">Include resize/format/quality settings (false for preview)</param>
        public static ImageProcessingOptions BuildSingleImageOptions(
            // Output settings
            string format,
            int quality,
            bool includeResizeAndOutput,
            // Resize
            bool resizeEnabled,
            int? resizeWidth,
            int? resizeHeight,
            bool maintainAspectRatio,
            // Adjustments
            int brightness,
            int contrast,
            int saturation,
            // Filters
            bool grayscale,
            bool sepia,
            bool invert,
            // Blur/Sharpen
            int blurRadius,
            int sharpenAmount,
            // Transform
            int rotationAngle,
            bool flipHorizontal,
            bool flipVertical,
            // Crop
            bool cropEnabled,
            int cropX,
            int cropY,
            int cropWidth,
            int cropHeight,
            // Watermark
            bool watermarkEnabled,
            string watermarkText,
            byte[] watermarkImageBytes,
            WatermarkPosition watermarkPosition,
            int watermarkOpacity,
            int watermarkFontSize,
            string watermarkColor,
            int watermarkPadding,
            // Phase 3 Effects
            bool autoEnhance,
            bool vignette,
            int vignetteRadius,
            int vignetteSoftness,
            bool posterize,
            int posterizeLevels,
            bool edgeDetect,
            int edgeDetectRadius,
            // Background Removal
            bool removeBackground,
            string backgroundColor,
            int backgroundTolerance,
            // Metadata
            bool stripMetadata,
            bool generateMultiSizeIco,
            int[] icoSizes = null)
        {
            var builder = Create()
                .WithAdjustments(brightness, contrast, saturation)
                .WithGrayscale(grayscale)
                .WithSepia(sepia)
                .WithInvert(invert)
                .WithBlur(blurRadius)
                .WithSharpen(sharpenAmount)
                .WithTransform(rotationAngle, flipHorizontal, flipVertical)
                .WithAutoEnhance(autoEnhance);

            // Crop
            if (cropEnabled)
                builder.WithCrop(cropX, cropY, cropWidth, cropHeight);

            // Watermark
            if (watermarkEnabled)
            {
                if (watermarkImageBytes != null && watermarkImageBytes.Length > 0)
                    builder.WithImageWatermark(watermarkImageBytes, watermarkPosition, watermarkOpacity, watermarkPadding);
                else if (!string.IsNullOrEmpty(watermarkText))
                    builder.WithTextWatermark(watermarkText, watermarkPosition, watermarkOpacity, watermarkFontSize, watermarkColor, watermarkPadding);
            }

            // Phase 3 Effects
            if (vignette)
                builder.WithVignette(vignetteRadius, vignetteSoftness);
            if (posterize)
                builder.WithPosterize(posterizeLevels);
            if (edgeDetect)
                builder.WithEdgeDetect(edgeDetectRadius);

            // Background Removal
            if (removeBackground)
                builder.WithBackgroundRemoval(backgroundColor, backgroundTolerance);

            // Output settings
            if (includeResizeAndOutput)
            {
                builder.WithFormat(format)
                       .WithQuality(quality)
                       .WithStripMetadata(stripMetadata);

                if (resizeEnabled)
                    builder.WithResize(resizeWidth, resizeHeight, maintainAspectRatio);

                if (generateMultiSizeIco)
                    builder.WithMultiSizeIco(icoSizes);
            }
            else
            {
                builder.WithFormat("png");
            }

            return builder.Build();
        }
    }
}
