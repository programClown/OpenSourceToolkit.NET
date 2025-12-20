using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Media;
using OpenSourceToolkit.NET.Localization;
using System;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class AsciiArtToolViewModel : ToolViewModel
    {
        public override int Id => 18;
        public override string Name => ToolkitLocalization.GetString("Tool_AsciiArt_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_AsciiArt_Description");
        public override string IconKey => "AsciiArtIcon";

        private string _path;
        public string Path
        {
            get => _path;
            set
            {
                if (SetProperty(ref _path, value))
                {
                    // Auto-convert if valid
                    ValidateAndConvert();
                }
            }
        }

        private string _output;
        public string Output
        {
            get => _output;
            set => SetProperty(ref _output, value);
        }

        private double _fontSize = 8.0;
        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        private void ValidateAndConvert()
        {
            if (string.IsNullOrWhiteSpace(Path))
            {
                // Don't show error if just empty (e.g. clearing field)
                Output = string.Empty;
                return;
            }

            // 1. Extension Check
            var validExtensions = new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
            var ext = System.IO.Path.GetExtension(Path)?.ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !validExtensions.Contains(ext))
            {
                Output = "Error: Invalid file extension. Supported formats: PNG, JPG, JPEG, BMP, GIF.";
                return;
            }

            if (!File.Exists(Path))
            {
                Output = "Error: File does not exist.";
                return;
            }

            // 2. Content Check (Try to load)
            try
            {
                // Basic sanity check on file header/content via System.Drawing.Bitmap
                // This will throw if not a valid image
                using (var fs = new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    using (var img = System.Drawing.Image.FromStream(fs))
                    {
                        // Valid image
                    }
                }
            }
            catch
            {
                Output = "Error: The file content is not a valid or supported image format.";
                return;
            }

            // 3. Convert
            try
            {
                // Fixed width 150 chars for better resolution on desktop
                Output = AsciiArtGenerator.ConvertImageToAscii(Path, 150);
            }
            catch (Exception ex)
            {
                Output = $"Error during conversion: {ex.Message}";
            }
        }
    }
}
