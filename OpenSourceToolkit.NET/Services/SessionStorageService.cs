using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OpenSourceToolkit.NET.ViewModels.Tools;

namespace OpenSourceToolkit.NET.Services
{
    /// <summary>
    /// Implementation of session storage for Image Editor sessions.
    ///
    /// Folder structure:
    /// %LocalAppData%/OpenSourceToolkit/ImageEditorSessions/
    /// ├── sessions-index.json
    /// ├── {SessionGuid}/
    /// │   ├── session.json
    /// │   ├── workspace_{name}.{ext}    - Current workspace state
    /// │   ├── original_{name}.{ext}     - Pristine original (never modified)
    /// │   ├── thumbnails/               - Full-res images in gallery
    /// │   │   ├── 000_Original.png
    /// │   │   └── 001_Gen_1.png
    /// │   └── history/                  - Undo history (max 10)
    /// │       ├── undo_001.png
    /// │       └── undo_002.png
    /// </summary>
    public class SessionStorageService : ISessionStorageService
    {
        private static readonly string BasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OpenSourceToolkit",
            "ImageEditorSessions"
        );

        private const string IndexFileName = "sessions-index.json";
        private const string SessionFileName = "session.json";
        private const string ThumbnailsFolderName = "thumbnails";
        private const string HistoryFolderName = "history";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        // Singleton instance
        private static readonly Lazy<SessionStorageService> _instance =
            new Lazy<SessionStorageService>(() => new SessionStorageService());

        public static SessionStorageService Default => _instance.Value;

        public string GetSessionsBasePath() => BasePath;

        public string GetSessionFolderPath(string sessionId)
        {
            return Path.Combine(BasePath, sessionId);
        }

        public bool SessionExists(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return false;
            var sessionPath = Path.Combine(GetSessionFolderPath(sessionId), SessionFileName);
            return File.Exists(sessionPath);
        }

        public async Task<SessionIndex> LoadSessionIndexAsync()
        {
            try
            {
                var indexPath = Path.Combine(BasePath, IndexFileName);
                if (!File.Exists(indexPath))
                    return new SessionIndex();

                var json = await ReadAllTextAsync(indexPath);
                return JsonSerializer.Deserialize<SessionIndex>(json) ?? new SessionIndex();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error loading session index: {ex.Message}");
                return new SessionIndex();
            }
        }

        public async Task SaveSessionIndexAsync(SessionIndex index)
        {
            try
            {
                EnsureDirectoryExists(BasePath);
                var indexPath = Path.Combine(BasePath, IndexFileName);
                var json = JsonSerializer.Serialize(index, JsonOptions);
                await WriteAllTextAsync(indexPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error saving session index: {ex.Message}");
            }
        }

        public async Task<ImageEditorSession> LoadSessionAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return null;

            try
            {
                var sessionFolder = GetSessionFolderPath(sessionId);
                var sessionPath = Path.Combine(sessionFolder, SessionFileName);

                if (!File.Exists(sessionPath))
                    return null;

                var json = await ReadAllTextAsync(sessionPath);
                return JsonSerializer.Deserialize<ImageEditorSession>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error loading session {sessionId}: {ex.Message}");
                return null;
            }
        }

        public async Task SaveSessionAsync(ImageEditorSession session)
        {
            if (session == null)
                return;

            try
            {
                session.LastModifiedAt = DateTime.Now;

                var sessionFolder = GetSessionFolderPath(session.Id);
                EnsureDirectoryExists(sessionFolder);

                var sessionPath = Path.Combine(sessionFolder, SessionFileName);
                var json = JsonSerializer.Serialize(session, JsonOptions);
                await WriteAllTextAsync(sessionPath, json);

                // Update the index
                await UpdateSessionInIndexAsync(session);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error saving session {session.Id}: {ex.Message}");
            }
        }

        public async Task DeleteSessionAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId))
                return;

            try
            {
                var sessionFolder = GetSessionFolderPath(sessionId);
                if (Directory.Exists(sessionFolder))
                {
                    Directory.Delete(sessionFolder, recursive: true);
                }

                // Remove from index
                var index = await LoadSessionIndexAsync();
                index.Sessions.RemoveAll(s => s.Id == sessionId);
                if (index.ActiveSessionId == sessionId)
                    index.ActiveSessionId = null;
                await SaveSessionIndexAsync(index);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error deleting session {sessionId}: {ex.Message}");
            }
        }

        public async Task<string> SaveImageToSessionAsync(string sessionId, byte[] imageData, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || imageData == null || string.IsNullOrEmpty(filename))
                return null;

            try
            {
                var sessionFolder = GetSessionFolderPath(sessionId);
                EnsureDirectoryExists(sessionFolder);

                var filePath = Path.Combine(sessionFolder, filename);
                await WriteAllBytesAsync(filePath, imageData);
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error saving image to session {sessionId}: {ex.Message}");
                return null;
            }
        }

        public async Task<byte[]> LoadImageFromSessionAsync(string sessionId, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(filename))
                return null;

            try
            {
                var filePath = Path.Combine(GetSessionFolderPath(sessionId), filename);
                if (!File.Exists(filePath))
                    return null;

                return await ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error loading image from session {sessionId}: {ex.Message}");
                return null;
            }
        }

        public async Task<string> SaveThumbnailToSessionAsync(string sessionId, byte[] imageData, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || imageData == null || string.IsNullOrEmpty(filename))
                return null;

            try
            {
                var thumbnailsFolder = Path.Combine(GetSessionFolderPath(sessionId), ThumbnailsFolderName);
                EnsureDirectoryExists(thumbnailsFolder);

                var filePath = Path.Combine(thumbnailsFolder, filename);
                await WriteAllBytesAsync(filePath, imageData);
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error saving thumbnail to session {sessionId}: {ex.Message}");
                return null;
            }
        }

        public async Task<byte[]> LoadThumbnailFromSessionAsync(string sessionId, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(filename))
                return null;

            try
            {
                var filePath = Path.Combine(GetSessionFolderPath(sessionId), ThumbnailsFolderName, filename);
                if (!File.Exists(filePath))
                    return null;

                return await ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error loading thumbnail from session {sessionId}: {ex.Message}");
                return null;
            }
        }

        public async Task<string> SaveThumbnailPreviewAsync(string sessionId, byte[] imageData, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || imageData == null || string.IsNullOrEmpty(filename))
                return null;

            try
            {
                var thumbnailsFolder = Path.Combine(GetSessionFolderPath(sessionId), ThumbnailsFolderName);
                EnsureDirectoryExists(thumbnailsFolder);

                var filePath = Path.Combine(thumbnailsFolder, filename);
                await WriteAllBytesAsync(filePath, imageData);
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error saving thumbnail preview to session {sessionId}: {ex.Message}");
                return null;
            }
        }

        public async Task<byte[]> LoadThumbnailPreviewAsync(string sessionId, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(filename))
                return null;

            try
            {
                var filePath = Path.Combine(GetSessionFolderPath(sessionId), ThumbnailsFolderName, filename);
                if (!File.Exists(filePath))
                    return null;

                return await ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error loading thumbnail preview from session {sessionId}: {ex.Message}");
                return null;
            }
        }

        public bool ThumbnailPreviewExists(string sessionId, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(filename))
                return false;

            var filePath = Path.Combine(GetSessionFolderPath(sessionId), ThumbnailsFolderName, filename);
            return File.Exists(filePath);
        }

        public async Task<string> SaveHistoryImageAsync(string sessionId, byte[] imageData, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || imageData == null || string.IsNullOrEmpty(filename))
                return null;

            try
            {
                var historyFolder = Path.Combine(GetSessionFolderPath(sessionId), HistoryFolderName);
                EnsureDirectoryExists(historyFolder);

                var filePath = Path.Combine(historyFolder, filename);
                await WriteAllBytesAsync(filePath, imageData);
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error saving history image to session {sessionId}: {ex.Message}");
                return null;
            }
        }

        public async Task<byte[]> LoadHistoryImageAsync(string sessionId, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(filename))
                return null;

            try
            {
                var filePath = Path.Combine(GetSessionFolderPath(sessionId), HistoryFolderName, filename);
                if (!File.Exists(filePath))
                    return null;

                return await ReadAllBytesAsync(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionStorage] Error loading history image from session {sessionId}: {ex.Message}");
                return null;
            }
        }

        public Task DeleteHistoryImageAsync(string sessionId, string filename)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(filename))
                return Task.CompletedTask;

            return Task.Run(() =>
            {
                try
                {
                    var filePath = Path.Combine(GetSessionFolderPath(sessionId), HistoryFolderName, filename);
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SessionStorage] Error deleting history image from session {sessionId}: {ex.Message}");
                }
            });
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Helper Methods
        // ═══════════════════════════════════════════════════════════════════════════

        private async Task UpdateSessionInIndexAsync(ImageEditorSession session)
        {
            var index = await LoadSessionIndexAsync();

            var summary = SessionSummary.FromSession(session);
            var existing = index.Sessions.FirstOrDefault(s => s.Id == session.Id);

            if (existing != null)
            {
                // Update existing entry
                var idx = index.Sessions.IndexOf(existing);
                index.Sessions[idx] = summary;
            }
            else
            {
                // Add new entry
                index.Sessions.Add(summary);
            }

            await SaveSessionIndexAsync(index);
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        // Async file operations for .NET 4.7.2 compatibility
        private static Task<string> ReadAllTextAsync(string path)
        {
            return Task.Run(() => File.ReadAllText(path));
        }

        private static Task WriteAllTextAsync(string path, string contents)
        {
            return Task.Run(() => File.WriteAllText(path, contents));
        }

        private static Task<byte[]> ReadAllBytesAsync(string path)
        {
            return Task.Run(() => File.ReadAllBytes(path));
        }

        private static Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            return Task.Run(() => File.WriteAllBytes(path, bytes));
        }
    }
}
