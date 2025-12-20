using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.Services;
using OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter.Models;
using OpenSourceToolkit.NET.ViewModels.Tools; // For session types (ImageEditorSession, SessionSummary, etc.)

namespace OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter
{
    /// <summary>
    /// Controller for session persistence/autosave and session management.
    /// </summary>
    public sealed class SessionController : ObservableObject
    {
        private readonly ISessionStorageService _sessionStorage;
        private readonly ImageProcessor _imageProcessor;
        private CancellationTokenSource _autoSaveCts;
        private bool _sessionDirty;
        private bool _isLoadingSession;

        // Thumbnail preview size (80x80 for display in the strip)
        private const int ThumbnailPreviewSize = 80;

        // ═══════════════════════════════════════════════════════════════════════════
        // Session State
        // ═══════════════════════════════════════════════════════════════════════════

        private ImageEditorSession _currentSession;
        public ImageEditorSession CurrentSession
        {
            get => _currentSession;
            private set
            {
                if (SetProperty(ref _currentSession, value))
                {
                    OnPropertyChanged(nameof(CurrentSessionDisplayName));
                    OnPropertyChanged(nameof(HasCurrentSession));
                    RenameSessionCommand?.NotifyCanExecuteChanged();
                    SwitchSessionCommand?.NotifyCanExecuteChanged();
                    RevertToOriginalCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        public bool HasCurrentSession => CurrentSession != null;
        public string CurrentSessionDisplayName => CurrentSession?.DisplayName ?? "No Session";

        private ObservableCollection<SessionSummary> _availableSessions;
        public ObservableCollection<SessionSummary> AvailableSessions
        {
            get => _availableSessions ?? (_availableSessions = new ObservableCollection<SessionSummary>());
            set => SetProperty(ref _availableSessions, value);
        }

        private SessionSummary _selectedSessionSummary;
        public SessionSummary SelectedSessionSummary
        {
            get => _selectedSessionSummary;
            set { if (SetProperty(ref _selectedSessionSummary, value)) SwitchSessionCommand?.NotifyCanExecuteChanged(); }
        }

        public bool HasMultipleSessions => AvailableSessions.Count > 1;
        public bool IsDirty => _sessionDirty;

        // ═══════════════════════════════════════════════════════════════════════════
        // Commands
        // ═══════════════════════════════════════════════════════════════════════════

        public RelayCommand NewSessionCommand { get; }
        public RelayCommand SwitchSessionCommand { get; }
        public RelayCommand DeleteSessionCommand { get; }
        public RelayCommand SaveSessionCommand { get; }
        public RelayCommand RenameSessionCommand { get; }
        public RelayCommand RevertToOriginalCommand { get; }

        // ═══════════════════════════════════════════════════════════════════════════
        // External Actions/Delegates (wired by root)
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>Action to show rename session dialog. Returns new name or null if cancelled.</summary>
        public Func<string, Task<string>> ShowRenameSessionDialogAction { get; set; }

        /// <summary>Action to confirm destructive action with unsaved images. Returns true if user confirms.</summary>
        public Func<List<string>, string, Task<bool>> ConfirmDestructiveActionAsync { get; set; }

        /// <summary>Delegate to get list of unsaved thumbnail names.</summary>
        public Func<List<string>> GetUnsavedThumbnailNames { get; set; }

        /// <summary>Delegate to check unsaved changes before switching (returns true if should proceed).</summary>
        public Func<Task<bool>> CheckUnsavedChangesAsync { get; set; }

        /// <summary>Delegate to capture workspace state for saving.</summary>
        public Func<SessionWorkspaceState> CaptureWorkspaceState { get; set; }

        /// <summary>Delegate to capture thumbnails for saving.</summary>
        public Func<SessionThumbnailsState> CaptureThumbnailsState { get; set; }

        /// <summary>Delegate to capture AI chat for saving.</summary>
        public Func<SessionAiState> CaptureAiState { get; set; }

        /// <summary>Delegate to restore workspace from session.</summary>
        public Action<SessionWorkspaceState> RestoreWorkspaceState { get; set; }

        /// <summary>Delegate to restore thumbnails from session.</summary>
        public Action<SessionThumbnailsState> RestoreThumbnailsState { get; set; }

        /// <summary>Delegate to restore AI state from session.</summary>
        public Action<SessionAiState> RestoreAiState { get; set; }

        /// <summary>Delegate to clear workspace without prompt.</summary>
        public Action ClearWorkspace { get; set; }

        /// <summary>Delegate to load original image and restore to workspace.</summary>
        public Func<byte[], Task> RevertToOriginalAsync { get; set; }

        /// <summary>Delegate to load a thumbnail into workspace by its data (called after restore if no workspace image).</summary>
        public Action<SessionThumbnailItemState> LoadThumbnailToWorkspace { get; set; }

        // ═══════════════════════════════════════════════════════════════════════════
        // Constructor
        // ═══════════════════════════════════════════════════════════════════════════

        public SessionController(ISessionStorageService sessionStorage, ImageProcessor imageProcessor = null)
        {
            _sessionStorage = sessionStorage ?? throw new ArgumentNullException(nameof(sessionStorage));
            _imageProcessor = imageProcessor ?? new ImageProcessor();

            NewSessionCommand = new RelayCommand(ExecuteNewSession);
            SwitchSessionCommand = new RelayCommand(ExecuteSwitchSession, CanSwitchSession);
            DeleteSessionCommand = new RelayCommand(ExecuteDeleteSession);
            SaveSessionCommand = new RelayCommand(ExecuteSaveSession);
            RenameSessionCommand = new RelayCommand(ExecuteRenameSession, () => CurrentSession != null);
            RevertToOriginalCommand = new RelayCommand(ExecuteRevertToOriginal, () => CurrentSession != null && !string.IsNullOrEmpty(CurrentSession?.OriginalImageFileName));
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Public Methods
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initializes session management on tool load.
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                var index = await _sessionStorage.LoadSessionIndexAsync();
                AvailableSessions.Clear();
                foreach (var summary in index.Sessions.OrderByDescending(s => s.LastModifiedAt))
                    AvailableSessions.Add(summary);
                OnPropertyChanged(nameof(HasMultipleSessions));

                var lastSessionId = AppSettings.Current.ImageEditorSessions?.LastActiveSessionId;
                if (!string.IsNullOrEmpty(lastSessionId) && _sessionStorage.SessionExists(lastSessionId))
                    await LoadSessionAsync(lastSessionId);
                else if (AvailableSessions.Count > 0)
                    await LoadSessionAsync(AvailableSessions[0].Id);
                else
                    await CreateNewSessionAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Session] Error initializing sessions: {ex.Message}");
                await CreateNewSessionAsync();
            }
        }

        /// <summary>
        /// Creates a new empty session.
        /// </summary>
        public async Task CreateNewSessionAsync()
        {
            if (CheckUnsavedChangesAsync != null && !await CheckUnsavedChangesAsync())
                return;

            if (_sessionDirty && CurrentSession != null)
                await SaveCurrentSessionAsync();

            ClearWorkspace?.Invoke();

            var session = ImageEditorSession.CreateNew();
            CurrentSession = session;

            await _sessionStorage.SaveSessionAsync(session);

            var summary = SessionSummary.FromSession(session);
            AvailableSessions.Insert(0, summary);
            SelectedSessionSummary = summary;
            OnPropertyChanged(nameof(HasMultipleSessions));

            UpdateLastActiveSession(session.Id);
            _sessionDirty = false;
        }

        /// <summary>
        /// Loads a session from disk by ID.
        /// </summary>
        public async Task LoadSessionAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return;

            if (CurrentSession != null && CurrentSession.Id != sessionId)
            {
                if (CheckUnsavedChangesAsync != null && !await CheckUnsavedChangesAsync())
                    return;
            }

            try
            {
                _isLoadingSession = true;

                if (_sessionDirty && CurrentSession != null && CurrentSession.Id != sessionId)
                    await SaveCurrentSessionAsync();

                var session = await _sessionStorage.LoadSessionAsync(sessionId);
                if (session == null)
                {
                    Console.WriteLine($"[Session] Session {sessionId} not found, creating new");
                    await CreateNewSessionAsync();
                    return;
                }

                ClearWorkspace?.Invoke();
                CurrentSession = session;

                // Restore thumbnails
                SessionThumbnailItemState latestThumbnail = null;
                if (session.Thumbnails != null && session.Thumbnails.Count > 0)
                {
                    var thumbnailsState = new SessionThumbnailsState();
                    foreach (var thumbData in session.Thumbnails)
                    {
                        var thumbBytes = await _sessionStorage.LoadThumbnailFromSessionAsync(sessionId, thumbData.ImageFileName);
                        if (thumbBytes != null)
                        {
                            // Try to load cached preview (80x80) for faster display
                            byte[] previewBytes = null;
                            var previewFilename = GetThumbnailPreviewFilename(thumbData.ImageFileName);
                            if (!string.IsNullOrEmpty(previewFilename))
                                previewBytes = await _sessionStorage.LoadThumbnailPreviewAsync(sessionId, previewFilename);

                            var item = new SessionThumbnailItemState
                            {
                                Id = thumbData.Id,
                                Label = thumbData.Label,
                                ImageBytes = thumbBytes,
                                PreviewBytes = previewBytes, // Cached 80x80 preview (null if not yet created)
                                MimeType = thumbData.MimeType,
                                SendToAi = thumbData.SendToAi,
                                CreatedAt = thumbData.CreatedAt,
                                SavedAt = thumbData.SavedAt
                            };
                            thumbnailsState.Items.Add(item);
                        }
                    }
                    if (thumbnailsState.Items.Count > 0)
                        latestThumbnail = thumbnailsState.Items[thumbnailsState.Items.Count - 1];

                    RestoreThumbnailsState?.Invoke(thumbnailsState);
                }

                // Priority: Load latest thumbnail if exists (User preference: "latest thumb should be loaded")
                // Only fallback to saved workspace if no thumbnails exist.
                // Must dispatch to UI thread since we're after async I/O operations
                if (latestThumbnail != null)
                {
                    await global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        LoadThumbnailToWorkspace(latestThumbnail));
                }
                else if (!string.IsNullOrEmpty(session.WorkspaceImageFileName))
                {
                    var imageBytes = await _sessionStorage.LoadImageFromSessionAsync(sessionId, session.WorkspaceImageFileName);
                    if (imageBytes != null)
                    {
                        await global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            RestoreWorkspaceState(new SessionWorkspaceState
                            {
                                ImageBytes = imageBytes,
                                Width = session.WorkspaceWidth,
                                Height = session.WorkspaceHeight,
                                FileName = Path.GetFileName(session.OriginalSourcePath ?? "restored.png"),
                                Format = session.OriginalFormat,
                                SourcePath = session.OriginalSourcePath
                            }));
                    }
                }

                // Restore AI state
                RestoreAiState?.Invoke(new SessionAiState
                {
                    ChatHistory = session.ChatHistory,
                    SelectedConnection = session.SelectedAiConnection
                });

                SelectedSessionSummary = AvailableSessions.FirstOrDefault(s => s.Id == sessionId);
                UpdateLastActiveSession(sessionId);
                _sessionDirty = false;

                // Validate and create missing thumbnail previews (runs in background after load completes)
                _ = ValidateSessionThumbnailsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Session] Error loading session {sessionId}: {ex.Message}");
            }
            finally
            {
                _isLoadingSession = false;
            }
        }

        /// <summary>
        /// Saves the current session state to disk.
        /// </summary>
        public async Task SaveCurrentSessionAsync()
        {
            if (CurrentSession == null) return;

            try
            {
                var session = CurrentSession;
                var sessionId = session.Id;

                // Capture workspace state - use fixed filename to avoid accumulating old files
                var workspaceState = CaptureWorkspaceState?.Invoke();
                if (workspaceState?.ImageBytes != null)
                {
                    const string workspaceFilename = "workspace.png";
                    await _sessionStorage.SaveImageToSessionAsync(sessionId, workspaceState.ImageBytes, workspaceFilename);
                    session.WorkspaceImageFileName = workspaceFilename;
                    session.WorkspaceWidth = workspaceState.Width;
                    session.WorkspaceHeight = workspaceState.Height;
                    session.OriginalFormat = workspaceState.Format;
                    session.OriginalSourcePath = workspaceState.SourcePath;
                    // Note: original.png is saved at image load time via SaveOriginalImageAsync, not here
                }
                else
                {
                    session.WorkspaceImageFileName = null;
                }

                // Capture thumbnails
                var thumbnailsState = CaptureThumbnailsState?.Invoke();
                session.Thumbnails.Clear();
                if (thumbnailsState?.Items != null)
                {
                    int thumbIndex = 0;
                    foreach (var thumb in thumbnailsState.Items)
                    {
                        if (thumb.ImageBytes != null)
                        {
                            var ext = GetExtensionForMimeType(thumb.MimeType);
                            var safeLabel = SanitizeFilename(thumb.Label ?? $"image_{thumbIndex}");
                            var imageFilename = $"{thumbIndex:D3}_{safeLabel}{ext}";
                            await _sessionStorage.SaveThumbnailToSessionAsync(sessionId, thumb.ImageBytes, imageFilename);

                            session.Thumbnails.Add(new ThumbnailItemData
                            {
                                Id = thumb.Id,
                                Label = thumb.Label,
                                ImageFileName = imageFilename,
                                MimeType = thumb.MimeType,
                                SendToAi = thumb.SendToAi,
                                CreatedAt = thumb.CreatedAt,
                                SavedAt = thumb.SavedAt
                            });
                            thumbIndex++;
                        }
                    }
                }

                // Capture AI state
                var aiState = CaptureAiState?.Invoke();
                session.ChatHistory = aiState?.ChatHistory;
                session.SelectedAiConnection = aiState?.SelectedConnection;

                await _sessionStorage.SaveSessionAsync(session);

                // Update summary in list
                var existingSummary = AvailableSessions.FirstOrDefault(s => s.Id == sessionId);
                if (existingSummary != null)
                {
                    var newSummary = SessionSummary.FromSession(session);
                    existingSummary.DisplayName = newSummary.DisplayName;
                    existingSummary.LastModifiedAt = newSummary.LastModifiedAt;
                    existingSummary.ThumbnailCount = newSummary.ThumbnailCount;
                    existingSummary.HasWorkspaceImage = newSummary.HasWorkspaceImage;
                }

                _sessionDirty = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Session] Error saving session: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes the current session. Prompts for confirmation if there are unsaved images.
        /// </summary>
        public async Task DeleteCurrentSessionAsync()
        {
            if (CurrentSession == null) return;

            // Check for unsaved thumbnails and prompt user
            var unsavedNames = GetUnsavedThumbnailNames?.Invoke();
            if (unsavedNames != null && unsavedNames.Count > 0)
            {
                if (ConfirmDestructiveActionAsync == null)
                    return; // Can't confirm, abort

                var confirmed = await ConfirmDestructiveActionAsync(unsavedNames, "Delete Session");
                if (!confirmed)
                    return;
            }

            var sessionId = CurrentSession.Id;

            var summary = AvailableSessions.FirstOrDefault(s => s.Id == sessionId);
            if (summary != null)
                AvailableSessions.Remove(summary);

            await _sessionStorage.DeleteSessionAsync(sessionId);
            OnPropertyChanged(nameof(HasMultipleSessions));

            if (AvailableSessions.Count > 0)
                await LoadSessionAsync(AvailableSessions[0].Id);
            else
                await CreateNewSessionAsync();
        }

        /// <summary>
        /// Marks the session as dirty (needs saving). Triggers debounced auto-save.
        /// </summary>
        public void MarkDirty()
        {
            if (_isLoadingSession) return;

            _sessionDirty = true;

            if (AppSettings.Current.ImageEditorSessions?.AutoSaveSessions ?? true)
                ScheduleAutoSave();
        }

        /// <summary>
        /// Saves session on cleanup (fire and forget).
        /// </summary>
        public void SaveOnCleanup()
        {
            if (_sessionDirty && CurrentSession != null)
                _ = SaveCurrentSessionAsync();

            _autoSaveCts?.Cancel();
        }

        /// <summary>
        /// Saves original image to session folder (only once per session).
        /// Called when a new image is loaded into workspace - preserves pristine original for undo/revert.
        /// </summary>
        public async Task SaveOriginalImageAsync(byte[] imageBytes, string originalFileName)
        {
            if (CurrentSession == null || imageBytes == null) return;
            // Only save once per session - first loaded image becomes the "original"
            if (!string.IsNullOrEmpty(CurrentSession.OriginalImageFileName)) return;

            const string filename = "original.png";
            await _sessionStorage.SaveImageToSessionAsync(CurrentSession.Id, imageBytes, filename);
            CurrentSession.OriginalImageFileName = filename;

            RevertToOriginalCommand?.NotifyCanExecuteChanged();
        }

        /// <summary>
        /// Validates session thumbnails and creates missing preview files (80x80).
        /// Should be called after session load completes to optimize future loads.
        /// Naming: if full image is "008_Gen#8.png", preview is "008_Gen#8-thumb.png"
        /// </summary>
        public async Task ValidateSessionThumbnailsAsync()
        {
            if (CurrentSession == null) return;

            var sessionId = CurrentSession.Id;
            var thumbnails = CurrentSession.Thumbnails;
            if (thumbnails == null || thumbnails.Count == 0) return;

            int createdCount = 0;
            foreach (var thumbData in thumbnails)
            {
                if (string.IsNullOrEmpty(thumbData.ImageFileName)) continue;

                var previewFilename = GetThumbnailPreviewFilename(thumbData.ImageFileName);
                if (_sessionStorage.ThumbnailPreviewExists(sessionId, previewFilename))
                    continue; // Already has preview

                // Load full image and create 80x80 preview
                var fullImageBytes = await _sessionStorage.LoadThumbnailFromSessionAsync(sessionId, thumbData.ImageFileName);
                if (fullImageBytes == null) continue;

                try
                {
                    var previewOptions = new ImageProcessingOptions
                    {
                        Width = ThumbnailPreviewSize,
                        Height = ThumbnailPreviewSize,
                        MaintainAspectRatio = true,
                        Format = "png"
                    };
                    var previewBytes = _imageProcessor.ProcessImage(fullImageBytes, previewOptions);
                    await _sessionStorage.SaveThumbnailPreviewAsync(sessionId, previewBytes, previewFilename);
                    createdCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Session] Failed to create thumbnail preview for {thumbData.ImageFileName}: {ex.Message}");
                }
            }

            if (createdCount > 0)
                Console.WriteLine($"[Session] Created {createdCount} missing thumbnail preview(s) for session {CurrentSession.DisplayName}");
        }

        /// <summary>
        /// Gets the preview filename for a full-resolution thumbnail image.
        /// Example: "008_Gen#8.png" -> "008_Gen#8-thumb.png"
        /// </summary>
        public static string GetThumbnailPreviewFilename(string fullImageFilename)
        {
            if (string.IsNullOrEmpty(fullImageFilename)) return null;

            var ext = Path.GetExtension(fullImageFilename);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(fullImageFilename);
            return $"{nameWithoutExt}-thumb{ext}";
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Private Methods
        // ═══════════════════════════════════════════════════════════════════════════

        private bool CanSwitchSession() => SelectedSessionSummary != null &&
                                          CurrentSession != null &&
                                          SelectedSessionSummary.Id != CurrentSession.Id;

        private async void ExecuteNewSession() => await CreateNewSessionAsync();
        private async void ExecuteSwitchSession()
        {
            if (SelectedSessionSummary != null)
                await LoadSessionAsync(SelectedSessionSummary.Id);
        }
        private async void ExecuteDeleteSession() => await DeleteCurrentSessionAsync();
        private async void ExecuteSaveSession() => await SaveCurrentSessionAsync();
        private async void ExecuteRenameSession() => await RenameCurrentSessionAsync();

        private async void ExecuteRevertToOriginal()
        {
            if (CurrentSession == null || string.IsNullOrEmpty(CurrentSession.OriginalImageFileName))
                return;

            var originalBytes = await _sessionStorage.LoadImageFromSessionAsync(CurrentSession.Id, CurrentSession.OriginalImageFileName);
            if (originalBytes != null)
                RevertToOriginalAsync?.Invoke(originalBytes);
        }

        private async Task RenameCurrentSessionAsync()
        {
            if (CurrentSession == null) return;
            if (ShowRenameSessionDialogAction == null) return;

            var newName = await ShowRenameSessionDialogAction(CurrentSession.DisplayName);
            if (string.IsNullOrWhiteSpace(newName)) return;

            var error = ValidateSessionName(newName);
            if (error != null)
            {
                Console.WriteLine($"[Session] Invalid session name: {error}");
                return;
            }

            CurrentSession.DisplayName = newName.Trim();
            CurrentSession.LastModifiedAt = DateTime.Now;
            OnPropertyChanged(nameof(CurrentSessionDisplayName));

            var summary = AvailableSessions.FirstOrDefault(s => s.Id == CurrentSession.Id);
            if (summary != null)
            {
                summary.DisplayName = CurrentSession.DisplayName;
                summary.LastModifiedAt = CurrentSession.LastModifiedAt;
                var index = AvailableSessions.IndexOf(summary);
                if (index >= 0)
                {
                    AvailableSessions.RemoveAt(index);
                    AvailableSessions.Insert(index, summary);
                    SelectedSessionSummary = summary;
                }
            }

            await SaveCurrentSessionAsync();
            Console.WriteLine($"[Session] Renamed to: {CurrentSession.DisplayName}");
        }

        private void ScheduleAutoSave()
        {
            _autoSaveCts?.Cancel();
            _autoSaveCts = new CancellationTokenSource();
            var token = _autoSaveCts.Token;
            var delayMs = AppSettings.Current.ImageEditorSessions?.AutoSaveDelayMs ?? 5000;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delayMs, token);
                    if (!token.IsCancellationRequested && _sessionDirty)
                        await SaveCurrentSessionAsync();
                }
                catch (TaskCanceledException)
                {
                    // Expected when cancelled
                }
            });
        }

        private void UpdateLastActiveSession(string sessionId)
        {
            if (AppSettings.Current.ImageEditorSessions == null)
                AppSettings.Current.ImageEditorSessions = new ImageEditorSessionSettings();

            AppSettings.Current.ImageEditorSessions.LastActiveSessionId = sessionId;
            AppSettings.Save();
        }

        private static readonly char[] InvalidFileNameChars = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
        public const int MaxSessionNameLength = 50;

        public static string ValidateSessionName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Session name cannot be empty.";
            if (name.Length > MaxSessionNameLength)
                return $"Session name cannot exceed {MaxSessionNameLength} characters.";
            foreach (var c in InvalidFileNameChars)
            {
                if (name.Contains(c))
                    return $"Session name cannot contain '{c}' character.";
            }
            if (name.EndsWith(".") || name.EndsWith(" "))
                return "Session name cannot end with a dot or space.";
            return null;
        }

        private static string SanitizeFilename(string name)
        {
            if (string.IsNullOrEmpty(name)) return "image";
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = new string(name.Where(c => !invalid.Contains(c) && c != ' ').ToArray());
            return string.IsNullOrEmpty(sanitized) ? "image" : sanitized;
        }

        private static string GetExtensionForFormat(string format)
        {
            if (string.IsNullOrEmpty(format)) return ".png";
            var lower = format.ToLowerInvariant();
            switch (lower)
            {
                case "jpeg": return ".jpg";
                case "jpg": return ".jpg";
                case "png": return ".png";
                case "gif": return ".gif";
                case "webp": return ".webp";
                case "bmp": return ".bmp";
                case "tiff": return ".tiff";
                case "ico": return ".ico";
                case "svg": return ".svg";
                default: return "." + lower;
            }
        }

        private static string GetExtensionForMimeType(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType)) return ".png";
            switch (mimeType.ToLowerInvariant())
            {
                case "image/jpeg": return ".jpg";
                case "image/png": return ".png";
                case "image/gif": return ".gif";
                case "image/webp": return ".webp";
                case "image/bmp": return ".bmp";
                case "image/tiff": return ".tiff";
                case "image/x-icon": return ".ico";
                case "image/svg+xml": return ".svg";
                default: return ".png";
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // Session State DTOs (for capture/restore)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Captured workspace state for session serialization.</summary>
    public class SessionWorkspaceState
    {
        public byte[] ImageBytes { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string FileName { get; set; }
        public string Format { get; set; }
        public string SourcePath { get; set; }
    }

    /// <summary>Captured thumbnails state for session serialization.</summary>
    public class SessionThumbnailsState
    {
        public System.Collections.Generic.List<SessionThumbnailItemState> Items { get; set; } = new System.Collections.Generic.List<SessionThumbnailItemState>();
    }

    /// <summary>Single thumbnail item state.</summary>
    public class SessionThumbnailItemState
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public byte[] ImageBytes { get; set; }
        /// <summary>Cached 80x80 preview bytes (null if not yet created, will be generated on first load)</summary>
        public byte[] PreviewBytes { get; set; }
        public string MimeType { get; set; }
        public bool SendToAi { get; set; }
        public DateTime CreatedAt { get; set; }
        /// <summary>Timestamp when image was saved outside session (null = not saved)</summary>
        public DateTime? SavedAt { get; set; }
    }

    /// <summary>Captured AI state for session serialization.</summary>
    public class SessionAiState
    {
        public string ChatHistory { get; set; }
        public string SelectedConnection { get; set; }
    }
}
