using System.Threading.Tasks;
using OpenSourceToolkit.NET.ViewModels.Tools;

namespace OpenSourceToolkit.NET.Services
{
    /// <summary>
    /// Service interface for persisting Image Editor sessions to disk.
    /// </summary>
    public interface ISessionStorageService
    {
        /// <summary>
        /// Gets the base path for session storage.
        /// </summary>
        string GetSessionsBasePath();

        /// <summary>
        /// Loads the session index containing all session summaries.
        /// </summary>
        Task<SessionIndex> LoadSessionIndexAsync();

        /// <summary>
        /// Saves the session index to disk.
        /// </summary>
        Task SaveSessionIndexAsync(SessionIndex index);

        /// <summary>
        /// Loads a full session by ID.
        /// </summary>
        Task<ImageEditorSession> LoadSessionAsync(string sessionId);

        /// <summary>
        /// Saves a session to disk (metadata and updates index).
        /// </summary>
        Task SaveSessionAsync(ImageEditorSession session);

        /// <summary>
        /// Deletes a session and all its files.
        /// </summary>
        Task DeleteSessionAsync(string sessionId);

        /// <summary>
        /// Saves an image file to a session's folder.
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="imageData">Image bytes</param>
        /// <param name="filename">Target filename (e.g., "workspace.png")</param>
        /// <returns>Full path to the saved file</returns>
        Task<string> SaveImageToSessionAsync(string sessionId, byte[] imageData, string filename);

        /// <summary>
        /// Loads an image file from a session's folder.
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="filename">Filename to load</param>
        /// <returns>Image bytes or null if not found</returns>
        Task<byte[]> LoadImageFromSessionAsync(string sessionId, string filename);

        /// <summary>
        /// Saves a thumbnail image to a session's thumbnails subfolder.
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="imageData">Image bytes</param>
        /// <param name="filename">Target filename (e.g., "thumb_001.png")</param>
        /// <returns>Full path to the saved file</returns>
        Task<string> SaveThumbnailToSessionAsync(string sessionId, byte[] imageData, string filename);

        /// <summary>
        /// Loads a thumbnail image from a session's thumbnails subfolder.
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="filename">Filename to load</param>
        /// <returns>Image bytes or null if not found</returns>
        Task<byte[]> LoadThumbnailFromSessionAsync(string sessionId, string filename);

        /// <summary>
        /// Saves a display-size thumbnail preview to a session's thumbnails subfolder.
        /// Naming convention: if full image is "008_Gen#8.png", preview is "008_Gen#8-thumb.png"
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="imageData">Thumbnail preview bytes (80x80)</param>
        /// <param name="filename">Target filename (e.g., "008_Gen#8-thumb.png")</param>
        /// <returns>Full path to the saved file</returns>
        Task<string> SaveThumbnailPreviewAsync(string sessionId, byte[] imageData, string filename);

        /// <summary>
        /// Loads a display-size thumbnail preview from a session's thumbnails subfolder.
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="filename">Filename to load (e.g., "008_Gen#8-thumb.png")</param>
        /// <returns>Thumbnail preview bytes or null if not found</returns>
        Task<byte[]> LoadThumbnailPreviewAsync(string sessionId, string filename);

        /// <summary>
        /// Checks if a thumbnail preview file exists.
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="filename">Preview filename to check</param>
        /// <returns>True if the preview file exists</returns>
        bool ThumbnailPreviewExists(string sessionId, string filename);

        /// <summary>
        /// Saves an image to the undo history subfolder.
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="imageData">Image bytes</param>
        /// <param name="filename">Target filename</param>
        /// <returns>Full path to the saved file</returns>
        Task<string> SaveHistoryImageAsync(string sessionId, byte[] imageData, string filename);

        /// <summary>
        /// Loads an image from the undo history subfolder.
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="filename">Filename to load</param>
        /// <returns>Image bytes or null if not found</returns>
        Task<byte[]> LoadHistoryImageAsync(string sessionId, string filename);

        /// <summary>
        /// Deletes a history image file.
        /// </summary>
        Task DeleteHistoryImageAsync(string sessionId, string filename);

        /// <summary>
        /// Gets the path to a session's folder.
        /// </summary>
        string GetSessionFolderPath(string sessionId);

        /// <summary>
        /// Checks if a session exists on disk.
        /// </summary>
        bool SessionExists(string sessionId);
    }
}
