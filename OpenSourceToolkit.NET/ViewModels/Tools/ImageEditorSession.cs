using System;
using System.Collections.Generic;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    /// <summary>
    /// Represents a persistent session for the Image Editor, storing all workspace state.
    /// </summary>
    public class ImageEditorSession
    {
        /// <summary>GUID identifier for the session</summary>
        public string Id { get; set; }

        /// <summary>Session creation timestamp</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Last activity timestamp</summary>
        public DateTime LastModifiedAt { get; set; }

        /// <summary>User-editable display name, defaults to "yyyyMMdd-HHmm" format</summary>
        public string DisplayName { get; set; }

        /// <summary>Filename of current workspace image in session folder (e.g., "workspace.png")</summary>
        public string WorkspaceImageFileName { get; set; }

        /// <summary>Filename of the pristine original image (never modified)</summary>
        public string OriginalImageFileName { get; set; }

        /// <summary>Original file path of the loaded image (for display/reference only)</summary>
        public string OriginalSourcePath { get; set; }

        /// <summary>Serializable thumbnail metadata</summary>
        public List<ThumbnailItemData> Thumbnails { get; set; } = new List<ThumbnailItemData>();

        // Note: Undo history is NOT persisted - it's transient in-memory only.
        // When user switches images or closes app, they're prompted to save changes.

        /// <summary>AI chat history text</summary>
        public string ChatHistory { get; set; }

        /// <summary>Last used AI connection name</summary>
        public string SelectedAiConnection { get; set; }

        /// <summary>Workspace image dimensions</summary>
        public int WorkspaceWidth { get; set; }
        public int WorkspaceHeight { get; set; }

        /// <summary>Original image format (e.g., "PNG", "JPEG")</summary>
        public string OriginalFormat { get; set; }

        public ImageEditorSession()
        {
            Id = Guid.NewGuid().ToString("N");
            CreatedAt = DateTime.Now;
            LastModifiedAt = DateTime.Now;
            DisplayName = DateTime.Now.ToString("yyyyMMdd-HHmm");
        }

        /// <summary>
        /// Creates a new session with a unique ID and current timestamp.
        /// </summary>
        public static ImageEditorSession CreateNew()
        {
            return new ImageEditorSession();
        }
    }

    /// <summary>
    /// Represents a single undo history entry (in-memory only, not persisted).
    /// </summary>
    public class UndoHistoryItem
    {
        /// <summary>Image bytes for this state</summary>
        public byte[] ImageBytes { get; set; }

        /// <summary>Description of the state (e.g., "Before crop", "Before adjustment")</summary>
        public string Description { get; set; }

        /// <summary>Timestamp when this state was saved</summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>Image dimensions at this state</summary>
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// Serializable version of ThumbnailItem for session persistence.
    /// </summary>
    public class ThumbnailItemData
    {
        /// <summary>Unique identifier for the thumbnail</summary>
        public string Id { get; set; }

        /// <summary>Display label</summary>
        public string Label { get; set; }

        /// <summary>Filename in session's thumbnails folder (e.g., "thumb_001.png")</summary>
        public string ImageFileName { get; set; }

        /// <summary>MIME type of the image</summary>
        public string MimeType { get; set; }

        /// <summary>Whether this image should be sent with AI chat messages</summary>
        public bool SendToAi { get; set; }

        /// <summary>Timestamp when the thumbnail was created</summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>Timestamp when image was saved outside session (null = not saved)</summary>
        public DateTime? SavedAt { get; set; }
    }

    /// <summary>
    /// Index file tracking all sessions for the Image Editor.
    /// </summary>
    public class SessionIndex
    {
        /// <summary>Lightweight list of sessions for dropdown display</summary>
        public List<SessionSummary> Sessions { get; set; } = new List<SessionSummary>();

        /// <summary>ID of the currently active session</summary>
        public string ActiveSessionId { get; set; }
    }

    /// <summary>
    /// Lightweight session summary for dropdown display.
    /// Implements Equals/GetHashCode by Id for proper ComboBox selection binding.
    /// </summary>
    public class SessionSummary
    {
        /// <summary>Session GUID</summary>
        public string Id { get; set; }

        /// <summary>Display name for the session</summary>
        public string DisplayName { get; set; }

        /// <summary>Session creation timestamp</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Last modification timestamp</summary>
        public DateTime LastModifiedAt { get; set; }

        /// <summary>Number of thumbnails in the session (quick preview)</summary>
        public int ThumbnailCount { get; set; }

        /// <summary>Whether the session has a workspace image</summary>
        public bool HasWorkspaceImage { get; set; }

        /// <summary>
        /// Creates a summary from a full session object.
        /// </summary>
        public static SessionSummary FromSession(ImageEditorSession session)
        {
            return new SessionSummary
            {
                Id = session.Id,
                DisplayName = session.DisplayName,
                CreatedAt = session.CreatedAt,
                LastModifiedAt = session.LastModifiedAt,
                ThumbnailCount = session.Thumbnails?.Count ?? 0,
                HasWorkspaceImage = !string.IsNullOrEmpty(session.WorkspaceImageFileName)
            };
        }

        // Equality by Id for proper ComboBox SelectedItem binding
        public override bool Equals(object obj)
        {
            return obj is SessionSummary other && Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}
