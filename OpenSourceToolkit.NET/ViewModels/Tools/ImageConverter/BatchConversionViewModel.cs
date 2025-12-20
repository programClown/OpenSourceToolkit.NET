using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter.Models;

namespace OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter
{
    /// <summary>
    /// ViewModel for batch conversion: file list, convert/GIF/PDF operations, rename/resize/output settings.
    /// </summary>
    public sealed class BatchConversionViewModel : ObservableObject
    {
        private readonly ImageProcessor _imageProcessor;

        // ═══════════════════════════════════════════════════════════════════════════
        // File Collection
        // ═══════════════════════════════════════════════════════════════════════════

        private ObservableCollection<ImageFileModel> _files;
        public ObservableCollection<ImageFileModel> Files
        {
            get => _files;
            set => SetProperty(ref _files, value);
        }

        private ImageFileModel _selectedFile;
        public ImageFileModel SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (SetProperty(ref _selectedFile, value))
                {
                    OnPropertyChanged(nameof(IsOptionsEnabled));
                    UpdateResizeDefaults();
                }
            }
        }

        public bool IsOptionsEnabled => SelectedFile != null;
        public bool CanConvertBatch => Files.Count > 0 && !IsProcessing;

        // ═══════════════════════════════════════════════════════════════════════════
        // Output Settings
        // ═══════════════════════════════════════════════════════════════════════════

        private string _outputFormat = "png";
        public string OutputFormat
        {
            get => _outputFormat;
            set
            {
                if (SetProperty(ref _outputFormat, value))
                {
                    OnPropertyChanged(nameof(IsQualityVisible));
                    OnPropertyChanged(nameof(IsIcoFormat));
                }
            }
        }

        public bool IsQualityVisible =>
            OutputFormat?.ToLower() == "jpeg" ||
            OutputFormat?.ToLower() == "jpg" ||
            OutputFormat?.ToLower() == "webp";

        public bool IsIcoFormat => OutputFormat?.ToLower() == "ico";

        public ObservableCollection<string> AvailableFormats { get; } = new ObservableCollection<string>(ImageProcessor.SupportedFormats);

        private int _quality = 90;
        public int Quality
        {
            get => _quality;
            set => SetProperty(ref _quality, value);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Resize Settings
        // ═══════════════════════════════════════════════════════════════════════════

        private bool _resizeEnabled;
        public bool ResizeEnabled
        {
            get => _resizeEnabled;
            set
            {
                if (SetProperty(ref _resizeEnabled, value))
                {
                    if (value) UpdateResizeDefaults();
                    else { ResizeWidth = null; ResizeHeight = null; }
                }
            }
        }

        private int? _resizeWidth;
        public int? ResizeWidth
        {
            get => _resizeWidth;
            set
            {
                if (SetProperty(ref _resizeWidth, value))
                {
                    if (MaintainAspectRatio && ResizeEnabled && value.HasValue && SelectedFile != null && SelectedFile.OriginalWidth > 0)
                    {
                        double ratio = (double)SelectedFile.OriginalHeight / SelectedFile.OriginalWidth;
                        _resizeHeight = (int)Math.Round(value.Value * ratio);
                        OnPropertyChanged(nameof(ResizeHeight));
                    }
                }
            }
        }

        private int? _resizeHeight;
        public int? ResizeHeight
        {
            get => _resizeHeight;
            set
            {
                if (SetProperty(ref _resizeHeight, value))
                {
                    if (MaintainAspectRatio && ResizeEnabled && value.HasValue && SelectedFile != null && SelectedFile.OriginalHeight > 0)
                    {
                        double ratio = (double)SelectedFile.OriginalWidth / SelectedFile.OriginalHeight;
                        _resizeWidth = (int)Math.Round(value.Value * ratio);
                        OnPropertyChanged(nameof(ResizeWidth));
                    }
                }
            }
        }

        private bool _maintainAspectRatio = true;
        public bool MaintainAspectRatio
        {
            get => _maintainAspectRatio;
            set => SetProperty(ref _maintainAspectRatio, value);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ICO Multi-size
        // ═══════════════════════════════════════════════════════════════════════════

        private bool _generateMultiSizeIco;
        public bool GenerateMultiSizeIco
        {
            get => _generateMultiSizeIco;
            set => SetProperty(ref _generateMultiSizeIco, value);
        }

        private IcoSizePreset _selectedIcoPreset;
        public IcoSizePreset SelectedIcoPreset
        {
            get => _selectedIcoPreset;
            set { if (SetProperty(ref _selectedIcoPreset, value) && value != null) OnPropertyChanged(nameof(SelectedIcoSizesDisplay)); }
        }

        public string SelectedIcoSizesDisplay => SelectedIcoPreset != null
            ? string.Join(", ", SelectedIcoPreset.Sizes.Select(s => $"{s}px"))
            : "";

        public ObservableCollection<IcoSizePreset> IcoSizePresets { get; } = new ObservableCollection<IcoSizePreset>
        {
            new IcoSizePreset("Favicon (16×16)", new[] { 16 }),
            new IcoSizePreset("Small Icon (32×32)", new[] { 32 }),
            new IcoSizePreset("Medium Icon (48×48)", new[] { 48 }),
            new IcoSizePreset("Large Icon (64×64)", new[] { 64 }),
            new IcoSizePreset("Extra Large (128×128)", new[] { 128 }),
            new IcoSizePreset("Jumbo (256×256)", new[] { 256 }),
            new IcoSizePreset("Favicon Set (16, 32, 48)", new[] { 16, 32, 48 }),
            new IcoSizePreset("Windows Standard (16, 32, 48, 256)", new[] { 16, 32, 48, 256 }),
            new IcoSizePreset("All Sizes (16-256)", new[] { 16, 32, 48, 64, 128, 256 })
        };

        // ═══════════════════════════════════════════════════════════════════════════
        // Metadata
        // ═══════════════════════════════════════════════════════════════════════════

        private bool _stripMetadata;
        public bool StripMetadata
        {
            get => _stripMetadata;
            set => SetProperty(ref _stripMetadata, value);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Rename Pattern
        // ═══════════════════════════════════════════════════════════════════════════

        private bool _useRenamePattern;
        public bool UseRenamePattern
        {
            get => _useRenamePattern;
            set { if (SetProperty(ref _useRenamePattern, value)) OnPropertyChanged(nameof(RenamePatternPreview)); }
        }

        private string _renamePattern = "{name}_{width}x{height}";
        public string RenamePattern
        {
            get => _renamePattern;
            set { if (SetProperty(ref _renamePattern, value)) OnPropertyChanged(nameof(RenamePatternPreview)); }
        }

        public string RenamePatternPreview
        {
            get
            {
                if (SelectedFile == null) return "";
                return ImageProcessor.GenerateOutputFilename(
                    RenamePattern,
                    SelectedFile.FilePath,
                    SelectedFile.OriginalWidth,
                    SelectedFile.OriginalHeight,
                    OutputFormat,
                    0) + "." + OutputFormat;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // GIF Settings
        // ═══════════════════════════════════════════════════════════════════════════

        private int _gifFrameDelay = 100;
        public int GifFrameDelay
        {
            get => _gifFrameDelay;
            set => SetProperty(ref _gifFrameDelay, value);
        }

        private bool _gifLoop = true;
        public bool GifLoop
        {
            get => _gifLoop;
            set => SetProperty(ref _gifLoop, value);
        }

        private bool _gifOptimize = true;
        public bool GifOptimize
        {
            get => _gifOptimize;
            set => SetProperty(ref _gifOptimize, value);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // PDF Settings
        // ═══════════════════════════════════════════════════════════════════════════

        private int _pdfDpi = 150;
        public int PdfDpi
        {
            get => _pdfDpi;
            set => SetProperty(ref _pdfDpi, value);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Processing State
        // ═══════════════════════════════════════════════════════════════════════════

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (SetProperty(ref _isProcessing, value))
                    OnPropertyChanged(nameof(CanConvertBatch));
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Commands
        // ═══════════════════════════════════════════════════════════════════════════

        public RelayCommand SelectFilesCommand { get; }
        public RelayCommand ClearAllCommand { get; }
        public RelayCommand<ImageFileModel> RemoveFileCommand { get; }
        public AsyncRelayCommand ConvertAllCommand { get; }
        public AsyncRelayCommand CreateGifCommand { get; }
        public AsyncRelayCommand CreatePdfCommand { get; }
        public RelayCommand ExtractPdfPagesCommand { get; }

        // ═══════════════════════════════════════════════════════════════════════════
        // External Actions (wired by root/view)
        // ═══════════════════════════════════════════════════════════════════════════

        public Action SelectFilesAction { get; set; }
        public Func<Task<string>> SelectOutputFolderAction { get; set; }
        public Func<Task<string>> SaveGifAction { get; set; }
        public Func<Task<string>> SavePdfAction { get; set; }
        public Action OpenPdfAction { get; set; }
        public Action<string> ShowErrorAction { get; set; }

        // ═══════════════════════════════════════════════════════════════════════════
        // Constructor
        // ═══════════════════════════════════════════════════════════════════════════

        public BatchConversionViewModel(ImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));
            Files = new ObservableCollection<ImageFileModel>();
            Files.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CanConvertBatch));
            _selectedIcoPreset = IcoSizePresets[IcoSizePresets.Count - 1];

            SelectFilesCommand = new RelayCommand(() => SelectFilesAction?.Invoke());
            ClearAllCommand = new RelayCommand(ClearAll);
            RemoveFileCommand = new RelayCommand<ImageFileModel>(RemoveFile);
            ConvertAllCommand = new AsyncRelayCommand(ConvertAllAsync);
            CreateGifCommand = new AsyncRelayCommand(CreateGifAsync);
            CreatePdfCommand = new AsyncRelayCommand(CreatePdfAsync);
            ExtractPdfPagesCommand = new RelayCommand(() => OpenPdfAction?.Invoke());
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Public Methods
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Adds files to the batch list.
        /// </summary>
        public void AddFiles(string[] filePaths)
        {
            if (filePaths == null) return;

            Task.Run(async () =>
            {
                foreach (var path in filePaths)
                {
                    try
                    {
                        if (!File.Exists(path)) continue;

                        bool alreadyExists = false;
                        await global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            alreadyExists = Files.Any(f => f.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase));
                        });

                        if (alreadyExists) continue;

                        var info = new FileInfo(path);
                        byte[] fileBytes = File.ReadAllBytes(path);
                        byte[] previewBytes = null;
                        int width = 0, height = 0;

                        try
                        {
                            var imageInfo = new ImageMagick.MagickImageInfo(path);
                            width = (int)imageInfo.Width;
                            height = (int)imageInfo.Height;
                            previewBytes = _imageProcessor.ConvertToPreviewPng(fileBytes);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to inspect/preview {path}: {ex.Message}");
                        }

                        if (previewBytes != null)
                        {
                            await global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                using (var stream = new MemoryStream(previewBytes))
                                {
                                    var preview = new Bitmap(stream);
                                    var model = new ImageFileModel
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        FileName = info.Name,
                                        FilePath = path,
                                        OriginalSize = info.Length,
                                        OriginalFormat = info.Extension.TrimStart('.').ToUpper(),
                                        Preview = preview,
                                        OriginalWidth = width > 0 ? width : (int)preview.Size.Width,
                                        OriginalHeight = height > 0 ? height : (int)preview.Size.Height,
                                        RawBytes = fileBytes
                                    };
                                    Files.Add(model);
                                    if (SelectedFile == null) SelectedFile = model;
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error adding file {path}: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Extracts pages from a PDF file to images.
        /// </summary>
        public async Task ExtractPdfPages(string pdfPath)
        {
            if (string.IsNullOrEmpty(pdfPath) || !File.Exists(pdfPath)) return;

            IsProcessing = true;
            try
            {
                string outputDir = null;
                if (SelectOutputFolderAction != null)
                    outputDir = await SelectOutputFolderAction();

                if (string.IsNullOrEmpty(outputDir)) { IsProcessing = false; return; }

                await Task.Run(() =>
                {
                    byte[] pdfBytes = File.ReadAllBytes(pdfPath);
                    var options = new PdfToImagesOptions
                    {
                        OutputFormat = OutputFormat,
                        Dpi = PdfDpi
                    };

                    var pages = _imageProcessor.PdfToImages(pdfBytes, options);
                    string baseName = Path.GetFileNameWithoutExtension(pdfPath);

                    for (int i = 0; i < pages.Count; i++)
                    {
                        string outputPath = Path.Combine(outputDir, $"{baseName}_page_{i + 1:D3}.{OutputFormat}");
                        File.WriteAllBytes(outputPath, pages[i]);
                    }
                });
            }
            catch (Exception ex)
            {
                ShowErrorAction?.Invoke($"PDF extraction failed: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Private Methods
        // ═══════════════════════════════════════════════════════════════════════════

        private void RemoveFile(ImageFileModel file)
        {
            if (file != null)
            {
                Files.Remove(file);
                if (SelectedFile == file) SelectedFile = Files.FirstOrDefault();
            }
        }

        private void ClearAll()
        {
            Files.Clear();
            SelectedFile = null;
        }

        private void UpdateResizeDefaults()
        {
            if (SelectedFile != null && ResizeEnabled)
            {
                if (ResizeWidth == null || ResizeWidth == 0) ResizeWidth = SelectedFile.OriginalWidth;
                if (ResizeHeight == null || ResizeHeight == 0) ResizeHeight = SelectedFile.OriginalHeight;
            }
        }

        private async Task ConvertAllAsync()
        {
            if (Files.Count == 0) return;

            IsProcessing = true;
            try
            {
                string outputDir = null;
                if (SelectOutputFolderAction != null)
                    outputDir = await SelectOutputFolderAction();

                if (SelectOutputFolderAction != null && string.IsNullOrEmpty(outputDir))
                {
                    IsProcessing = false;
                    return;
                }

                await Task.Run(() =>
                {
                    int index = 0;
                    foreach (var file in Files)
                    {
                        try
                        {
                            global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => file.Status = "Processing...");
                            index++;

                            byte[] inputBytes = file.RawBytes ?? File.ReadAllBytes(file.FilePath);

                            var batchBuilder = ImageProcessingOptionsBuilder.ForBatch(OutputFormat, Quality)
                                .WithStripMetadata(StripMetadata);

                            if (ResizeEnabled)
                                batchBuilder.WithResize(ResizeWidth, ResizeHeight, MaintainAspectRatio);
                            if (GenerateMultiSizeIco)
                                batchBuilder.WithMultiSizeIco();

                            var batchOptions = batchBuilder.Build();
                            byte[] resultBytes = _imageProcessor.ProcessImage(inputBytes, batchOptions);

                            string targetDir = outputDir ?? Path.GetDirectoryName(file.FilePath);
                            string newFileName;
                            if (UseRenamePattern && !string.IsNullOrEmpty(RenamePattern))
                            {
                                int outWidth = batchOptions.Width ?? file.OriginalWidth;
                                int outHeight = batchOptions.Height ?? file.OriginalHeight;
                                newFileName = ImageProcessor.GenerateOutputFilename(
                                    RenamePattern, file.FilePath, outWidth, outHeight, OutputFormat, index) + "." + OutputFormat.ToLower();
                            }
                            else
                            {
                                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FilePath);
                                newFileName = $"{fileNameWithoutExt}_converted.{OutputFormat.ToLower()}";
                            }
                            string outputPath = Path.Combine(targetDir, newFileName);

                            File.WriteAllBytes(outputPath, resultBytes);

                            global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                file.ConvertedPath = outputPath;
                                file.ConvertedSize = resultBytes.Length;
                                file.Status = "Success";
                                file.ErrorMessage = null;
                            });
                        }
                        catch (Exception ex)
                        {
                            global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                file.Status = "Error";
                                file.ErrorMessage = ex.Message;
                            });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ShowErrorAction?.Invoke($"Conversion failed: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task CreateGifAsync()
        {
            if (Files.Count < 2)
            {
                ShowErrorAction?.Invoke("At least 2 images are required to create an animated GIF");
                return;
            }

            IsProcessing = true;
            try
            {
                string outputPath = null;
                if (SaveGifAction != null)
                    outputPath = await SaveGifAction();

                if (string.IsNullOrEmpty(outputPath)) { IsProcessing = false; return; }

                await Task.Run(() =>
                {
                    var frames = new List<byte[]>();
                    foreach (var file in Files)
                        frames.Add(file.RawBytes ?? File.ReadAllBytes(file.FilePath));

                    var options = new GifCreationOptions
                    {
                        FrameDelay = GifFrameDelay,
                        Loop = GifLoop,
                        OptimizeForSize = GifOptimize,
                        ResizeWidth = ResizeEnabled ? ResizeWidth : null,
                        ResizeHeight = ResizeEnabled ? ResizeHeight : null
                    };

                    byte[] gifBytes = _imageProcessor.CreateAnimatedGif(frames, options);
                    File.WriteAllBytes(outputPath, gifBytes);
                });
            }
            catch (Exception ex)
            {
                ShowErrorAction?.Invoke($"GIF creation failed: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task CreatePdfAsync()
        {
            if (Files.Count == 0)
            {
                ShowErrorAction?.Invoke("At least 1 image is required to create a PDF");
                return;
            }

            IsProcessing = true;
            try
            {
                string outputPath = null;
                if (SavePdfAction != null)
                    outputPath = await SavePdfAction();

                if (string.IsNullOrEmpty(outputPath)) { IsProcessing = false; return; }

                await Task.Run(() =>
                {
                    var images = new List<byte[]>();
                    foreach (var file in Files)
                        images.Add(file.RawBytes ?? File.ReadAllBytes(file.FilePath));

                    var options = new ImagesToPdfOptions
                    {
                        FitToPage = true,
                        Quality = Quality
                    };

                    byte[] pdfBytes = _imageProcessor.ImagesToPdf(images, options);
                    File.WriteAllBytes(outputPath, pdfBytes);
                });
            }
            catch (Exception ex)
            {
                ShowErrorAction?.Invoke($"PDF creation failed: {ex.Message}");
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
