using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter.Models
{
    /// <summary>
    /// Represents an image in the thumbnail strip with full-resolution data and AI send flag.
    /// </summary>
    public class ThumbnailItem : ObservableObject
    {
        public string Id { get; set; }
        public string Label { get; set; }

        /// <summary>Original file path if loaded from disk (for matching when saving)</summary>
        public string FilePath { get; set; }

        private byte[] _rawBytes;
        /// <summary>Full-resolution image bytes stored 1:1</summary>
        public byte[] RawBytes
        {
            get => _rawBytes;
            set
            {
                if (SetProperty(ref _rawBytes, value))
                {
                    OnPropertyChanged(nameof(FileSizeDisplay));
                }
            }
        }

        public string MimeType { get; set; }

        private DateTime _createdAt = DateTime.Now;
        /// <summary>Timestamp when the thumbnail was created</summary>
        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (SetProperty(ref _createdAt, value))
                {
                    OnPropertyChanged(nameof(TimestampDisplay));
                }
            }
        }

        private Bitmap _thumbnail;
        public Bitmap Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        private bool _sendToAi;
        /// <summary>Whether this image should be sent with AI chat messages</summary>
        public bool SendToAi
        {
            get => _sendToAi;
            set => SetProperty(ref _sendToAi, value);
        }

        private DateTime? _savedAt;
        /// <summary>Timestamp when this image was saved outside the session (null = never saved)</summary>
        public DateTime? SavedAt
        {
            get => _savedAt;
            set
            {
                if (SetProperty(ref _savedAt, value))
                {
                    OnPropertyChanged(nameof(IsSavedOutsideSession));
                    OnPropertyChanged(nameof(SavedStatusDisplay));
                }
            }
        }

        /// <summary>True if image has been saved outside the session</summary>
        public bool IsSavedOutsideSession => _savedAt.HasValue;

        /// <summary>Display text for saved status</summary>
        public string SavedStatusDisplay => IsSavedOutsideSession
            ? $"Saved: {_savedAt.Value:g}"
            : "⚠ Not saved outside session";

        /// <summary>File size formatted with thousand separators</summary>
        public string FileSizeDisplay => _rawBytes != null ? string.Format("{0:N0} bytes", _rawBytes.Length) : "";

        /// <summary>Timestamp formatted in local format</summary>
        public string TimestampDisplay => _createdAt.ToString("g");

        public ThumbnailItem()
        {
            Id = Guid.NewGuid().ToString("N").Substring(0, 8);
            _createdAt = DateTime.Now;
        }
    }
}
