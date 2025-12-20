using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.Localization;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class ClipboardImageModel
    {
        public string Id { get; set; }
        public Bitmap DisplayBitmap { get; set; }
        public byte[] OriginalBytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; }
        public long Size { get; set; }
        public DateTime Timestamp { get; set; }

        public string Dimensions => $"{Width} x {Height}";
        public string SizeDisplay => FormatSize(Size);
        public string TimestampDisplay => Timestamp.ToString("g");

        private static string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
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

    public class ClipboardImageSaverToolViewModel : ToolViewModel
    {
        public override int Id => 30;
        public override string Name => ToolkitLocalization.GetString("Tool_ClipboardImageSaver_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_ClipboardImageSaver_Description");
        public override string IconKey => "ClipboardImageIcon";

        private readonly ImageProcessor _imageProcessor;

        private ObservableCollection<ClipboardImageModel> _images;
        public ObservableCollection<ClipboardImageModel> Images
        {
            get => _images;
            set => SetProperty(ref _images, value);
        }

        private ClipboardImageModel _currentImage;
        public ClipboardImageModel CurrentImage
        {
            get => _currentImage;
            set
            {
                if (SetProperty(ref _currentImage, value) && value != null)
                {
                    ShowNotification("Image loaded in viewer");
                    UpdateResizeDefaults();
                }
            }
        }

        private string _notificationMessage;
        public string NotificationMessage
        {
            get => _notificationMessage;
            set => SetProperty(ref _notificationMessage, value);
        }

        private bool _isNotificationVisible;
        public bool IsNotificationVisible
        {
            get => _isNotificationVisible;
            set => SetProperty(ref _isNotificationVisible, value);
        }

        private async void ShowNotification(string message)
        {
            NotificationMessage = message;
            IsNotificationVisible = true;
            await Task.Delay(2000);
            IsNotificationVisible = false;
        }

        private string _outputFormat = "png";
        public string OutputFormat
        {
            get => _outputFormat;
            set
            {
                if (SetProperty(ref _outputFormat, value))
                {
                    OnPropertyChanged(nameof(IsQualityVisible));
                }
            }
        }

        public bool IsQualityVisible => OutputFormat?.ToLower() == "jpeg" || OutputFormat?.ToLower() == "jpg" || OutputFormat?.ToLower() == "webp";

        public ObservableCollection<string> AvailableFormats { get; } = new ObservableCollection<string>
        {
            "png", "jpeg", "bmp", "gif", "tiff"
        };

        private int _quality = 90;
        public int Quality
        {
            get => _quality;
            set => SetProperty(ref _quality, value);
        }

        private void UpdateResizeDefaults()
        {
            // If we just pasted an image or switched to it, we might want to auto-populate the resize fields
            // BUT only if they were empty or if user explicitly wants "original size" logic.
            // However, standard "Paint" style behavior is usually that these fields show current dimensions.
            // Let's follow the requested logic: default to current image dimensions.

            if (CurrentImage != null)
            {
                // If values are null or zero (meaning "original"), we set them to explicit original
                // so the UI shows the user what "Original" means.
                // Or we just update them always so the user sees the starting point.
                // The prompt said: "if checked/enabled should default ... to current"
                // We can just update them whenever CurrentImage changes.

                 ResizeWidth = CurrentImage.Width;
                 ResizeHeight = CurrentImage.Height;
            }
        }

        private int? _resizeWidth;
        public int? ResizeWidth
        {
            get => _resizeWidth;
            set => SetProperty(ref _resizeWidth, value);
        }

        private int? _resizeHeight;
        public int? ResizeHeight
        {
            get => _resizeHeight;
            set => SetProperty(ref _resizeHeight, value);
        }

        private bool _maintainAspectRatio = true;
        public bool MaintainAspectRatio
        {
            get => _maintainAspectRatio;
            set => SetProperty(ref _maintainAspectRatio, value);
        }

        public ICommand ClearAllCommand { get; }
        public ICommand DeleteImageCommand { get; }
        public ICommand DownloadCurrentImageCommand { get; }

        // This action is set by the View to handle the actual file save dialog
        public Action<byte[], string> SaveImageAction { get; set; }

        public ClipboardImageSaverToolViewModel()
        {
            _imageProcessor = new ImageProcessor();
            Images = new ObservableCollection<ClipboardImageModel>();
            ClearAllCommand = new RelayCommand(ClearAll);
            DeleteImageCommand = new RelayCommand<ClipboardImageModel>(DeleteImage);
            DownloadCurrentImageCommand = new RelayCommand(DownloadCurrentImage);
        }

        public void AddImageFromClipboard(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return;

            try
            {
                using (var ms = new MemoryStream(imageData))
                {
                    var bitmap = new Bitmap(ms);
                    // Reset position for processing if needed, though Bitmap(Stream) reads it.

                    var newImage = new ClipboardImageModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        OriginalBytes = imageData,
                        DisplayBitmap = bitmap,
                        Width = (int)bitmap.Size.Width,
                        Height = (int)bitmap.Size.Height,
                        Format = "PNG", // Default assumption for clipboard data usually
                        Size = imageData.Length,
                        Timestamp = DateTime.Now
                    };

                    Images.Insert(0, newImage);
                    CurrentImage = newImage;
                }
            }
            catch (Exception ex)
            {
                // Log or show error
                Console.WriteLine($"Error adding image: {ex.Message}");
            }
        }

        public void DownloadCurrentImage()
        {
            if (CurrentImage == null || SaveImageAction == null) return;

            try
            {
                var processedBytes = _imageProcessor.ProcessImage(
                    CurrentImage.OriginalBytes,
                    ResizeWidth,
                    ResizeHeight,
                    MaintainAspectRatio,
                    OutputFormat,
                    Quality
                );

                SaveImageAction(processedBytes, OutputFormat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing image: {ex.Message}");
            }
        }

        public void DownloadAllImages()
        {
             if (Images.Count == 0 || SaveImageAction == null) return;
             // Batch download would require folder picker usually, or sequential saves.
             // For simplicity in this MVP, we might just do current or left it for future,
             // but let's support processing the current one as the main action.
        }

        private void ClearAll()
        {
            Images.Clear();
            CurrentImage = null;
        }

        private void DeleteImage(ClipboardImageModel image)
        {
            if (image == null) return;
            Images.Remove(image);
            if (CurrentImage == image)
            {
                CurrentImage = Images.FirstOrDefault();
            }
        }
    }
}
