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
    /// ViewModel for the thumbnail strip: stores original + AI-generated images with full-resolution data.
    /// Manages add/remove/clear/save operations and the SendToAi flag for AI context.
    /// </summary>
    public sealed class ThumbnailStripViewModel : ObservableObject
    {
        private readonly ImageProcessor _imageProcessor;

        // ═══════════════════════════════════════════════════════════════════════════
        // Thumbnail Collection
        // ═══════════════════════════════════════════════════════════════════════════

        private ObservableCollection<ThumbnailItem> _thumbnailItems;
        public ObservableCollection<ThumbnailItem> ThumbnailItems
        {
            get => _thumbnailItems ?? (_thumbnailItems = new ObservableCollection<ThumbnailItem>());
            set => SetProperty(ref _thumbnailItems, value);
        }

        public bool HasThumbnails => ThumbnailItems.Count > 0;

        // ═══════════════════════════════════════════════════════════════════════════
        // Collapse State
        // ═══════════════════════════════════════════════════════════════════════════

        private bool _isCollapsed;
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set
            {
                if (SetProperty(ref _isCollapsed, value))
                {
                    OnPropertyChanged(nameof(ThumbnailStripVisible));
                    OnPropertyChanged(nameof(ShowThumbnailExpandButton));
                    OnCollapseStateChanged?.Invoke(value);
                }
            }
        }

        /// <summary>True when thumbnail strip should be visible (has items and not collapsed)</summary>
        public bool ThumbnailStripVisible => HasThumbnails && !IsCollapsed;

        /// <summary>True when the expand button should show in footer (has items and is collapsed)</summary>
        public bool ShowThumbnailExpandButton => HasThumbnails && IsCollapsed;

        // ═══════════════════════════════════════════════════════════════════════════
        // Commands
        // ═══════════════════════════════════════════════════════════════════════════

        public RelayCommand<ThumbnailItem> RemoveThumbnailCommand { get; }
        public AsyncRelayCommand<ThumbnailItem> SaveThumbnailImageCommand { get; }
        public RelayCommand<ThumbnailItem> LoadThumbnailToWorkspaceCommand { get; }
        public RelayCommand ClearAllCommand { get; }
        public RelayCommand ToggleThumbnailStripCommand { get; }

        // ═══════════════════════════════════════════════════════════════════════════
        // External Actions/Events (wired by root/view)
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>Action to show save dialog for full-resolution image. Returns the selected file path or null.</summary>
        public Func<string, Task<string>> SaveFullImageAction { get; set; }

        /// <summary>Action to confirm thumbnail deletion. Returns true if user confirms.</summary>
        public Func<string, Task<bool>> ConfirmDeleteThumbnailAction { get; set; }

        /// <summary>Action to confirm clearing all thumbnails with unsaved images. Returns true if user confirms.</summary>
        public Func<List<string>, string, Task<bool>> ConfirmDestructiveActionAsync { get; set; }

        /// <summary>Raised when a thumbnail should be loaded to workspace.</summary>
        public event Action<ThumbnailItem> LoadRequested;

        /// <summary>Raised when the collection changes (for dirty tracking).</summary>
        public event Action OnChanged;

        /// <summary>Raised when collapse state changes (for settings persistence).</summary>
        public Action<bool> OnCollapseStateChanged { get; set; }

        // ═══════════════════════════════════════════════════════════════════════════
        // Constructor
        // ═══════════════════════════════════════════════════════════════════════════

        public ThumbnailStripViewModel(ImageProcessor imageProcessor)
        {
            _imageProcessor = imageProcessor ?? throw new ArgumentNullException(nameof(imageProcessor));

            RemoveThumbnailCommand = new RelayCommand<ThumbnailItem>(RemoveThumbnailWithConfirm);
            SaveThumbnailImageCommand = new AsyncRelayCommand<ThumbnailItem>(SaveFullImageAsync);
            LoadThumbnailToWorkspaceCommand = new RelayCommand<ThumbnailItem>(item => LoadRequested?.Invoke(item));
            ClearAllCommand = new RelayCommand(ClearWithConfirmation);
            ToggleThumbnailStripCommand = new RelayCommand(() => IsCollapsed = !IsCollapsed);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Public Methods
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Adds an image to the thumbnail strip with full-resolution data preserved.
        /// </summary>
        /// <param name="imageBytes">Image data</param>
        /// <param name="label">Display label</param>
        /// <param name="mimeType">MIME type</param>
        /// <param name="selectForAi">If true, unchecks all other thumbnails and checks this one for AI flow</param>
        /// <param name="filePath">Original file path if loaded from disk (for matching when saving)</param>
        public void Add(byte[] imageBytes, string label, string mimeType = "image/png", bool selectForAi = false, string filePath = null)
        {
            if (imageBytes == null || imageBytes.Length == 0) return;

            try
            {
                // Create thumbnail preview (max 80px for display)
                var thumbnailOptions = new ImageProcessingOptions
                {
                    Width = 80,
                    Height = 80,
                    MaintainAspectRatio = true,
                    Format = "png"
                };
                byte[] thumbnailBytes = _imageProcessor.ProcessImage(imageBytes, thumbnailOptions);

                global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Uncheck all existing thumbnails if selectForAi is true
                    if (selectForAi)
                    {
                        foreach (var thumb in ThumbnailItems)
                            thumb.SendToAi = false;
                    }

                    using (var ms = new MemoryStream(thumbnailBytes))
                    {
                        var item = new ThumbnailItem
                        {
                            Label = label,
                            RawBytes = imageBytes,
                            MimeType = mimeType,
                            Thumbnail = new Bitmap(ms),
                            SendToAi = selectForAi,
                            FilePath = filePath
                        };
                        ThumbnailItems.Add(item);
                        NotifyCollectionChanged();
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add thumbnail: {ex.Message}");
            }
        }

        /// <summary>
        /// Restores a thumbnail item from session data.
        /// Uses cached preview bytes if available, otherwise processes full image.
        /// </summary>
        /// <param name="previewBytes">Cached 80x80 preview bytes (null to generate from imageBytes)</param>
        public void RestoreItem(string id, string label, byte[] imageBytes, string mimeType, bool sendToAi, DateTime createdAt, DateTime? savedAt = null, byte[] previewBytes = null)
        {
            if (imageBytes == null) return;

            try
            {
                // Use cached preview if available, otherwise generate from full image
                byte[] thumbnailBytes = previewBytes;
                if (thumbnailBytes == null)
                {
                    var thumbnailOptions = new ImageProcessingOptions
                    {
                        Width = 80,
                        Height = 80,
                        MaintainAspectRatio = true,
                        Format = "png"
                    };
                    thumbnailBytes = _imageProcessor.ProcessImage(imageBytes, thumbnailOptions);
                }

                global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    using (var ms = new MemoryStream(thumbnailBytes))
                    {
                        var item = new ThumbnailItem
                        {
                            Id = id,
                            Label = label,
                            RawBytes = imageBytes,
                            MimeType = mimeType,
                            Thumbnail = new Bitmap(ms),
                            SendToAi = sendToAi,
                            CreatedAt = createdAt,
                            SavedAt = savedAt
                        };
                        ThumbnailItems.Add(item);
                        OnPropertyChanged(nameof(HasThumbnails));
                        OnPropertyChanged(nameof(ThumbnailStripVisible));
                        OnPropertyChanged(nameof(ShowThumbnailExpandButton));
                        OnPropertyChanged(nameof(HasUnsavedThumbnails));
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to restore thumbnail {label}: {ex.Message}");
            }
        }

        /// <summary>
        /// Clears all thumbnails from the strip (no confirmation).
        /// </summary>
        public void Clear()
        {
            ThumbnailItems.Clear();
            NotifyCollectionChanged();
        }

        /// <summary>
        /// Clears all thumbnails with confirmation if there are unsaved images.
        /// </summary>
        private async void ClearWithConfirmation()
        {
            var unsaved = GetUnsavedThumbnails();
            if (unsaved.Count > 0 && ConfirmDestructiveActionAsync != null)
            {
                var unsavedNames = unsaved.Select(t => t.Label ?? "Unnamed").ToList();
                var confirmed = await ConfirmDestructiveActionAsync(unsavedNames, "Clear All Images");
                if (!confirmed)
                    return;
            }
            Clear();
        }

        /// <summary>
        /// Gets all images marked for sending to AI, scaled down for API efficiency (max 1024x1024).
        /// </summary>
        public List<(byte[] Data, string MimeType)> GetMarkedForAi()
        {
            return ThumbnailItems
                .Where(t => t.SendToAi && t.RawBytes != null)
                .Select(t => (_imageProcessor.ConvertToAiPng(t.RawBytes), "image/png"))
                .ToList();
        }

        /// <summary>
        /// Gets all thumbnail items (for session serialization).
        /// </summary>
        public IEnumerable<ThumbnailItem> GetAll() => ThumbnailItems;

        // ═══════════════════════════════════════════════════════════════════════════
        // Private Methods
        // ═══════════════════════════════════════════════════════════════════════════

        private async void RemoveThumbnailWithConfirm(ThumbnailItem item)
        {
            if (item == null) return;

            bool confirmed = true;
            if (ConfirmDeleteThumbnailAction != null)
                confirmed = await ConfirmDeleteThumbnailAction(item.Label ?? "this image");

            if (confirmed)
            {
                ThumbnailItems.Remove(item);
                NotifyCollectionChanged();
            }
        }

        private async Task SaveFullImageAsync(ThumbnailItem item)
        {
            if (item?.RawBytes == null || SaveFullImageAction == null) return;

            try
            {
                var suggestedName = item.Label?.Replace(" ", "_").Replace("#", "") ?? "image";
                var outputPath = await SaveFullImageAction(suggestedName);

                if (!string.IsNullOrEmpty(outputPath))
                {
                    File.WriteAllBytes(outputPath, item.RawBytes);
                    // Track that this image was saved outside the session
                    item.SavedAt = DateTime.Now;
                    OnChanged?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save image: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all thumbnails that have NOT been saved outside the session.
        /// </summary>
        public List<ThumbnailItem> GetUnsavedThumbnails()
        {
            return ThumbnailItems.Where(t => !t.IsSavedOutsideSession).ToList();
        }

        /// <summary>
        /// Returns true if any thumbnails exist that haven't been saved outside the session.
        /// </summary>
        public bool HasUnsavedThumbnails => ThumbnailItems.Any(t => !t.IsSavedOutsideSession);

        private void NotifyCollectionChanged()
        {
            OnPropertyChanged(nameof(HasThumbnails));
            OnPropertyChanged(nameof(ThumbnailStripVisible));
            OnPropertyChanged(nameof(ShowThumbnailExpandButton));
            OnChanged?.Invoke();
        }
    }
}
