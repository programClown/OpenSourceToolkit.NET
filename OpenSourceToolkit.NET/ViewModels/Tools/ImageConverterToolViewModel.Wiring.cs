using System.Linq;
using OpenSourceToolkit.NET.Services;
using OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class ImageConverterToolViewModel
    {
        private void WireChildViewModels()
        {
            // Workspace -> Thumbnails: add loaded/generated images
            Workspace.ThumbnailAddRequested += (bytes, label, mime, selectForAi, filePath) =>
            {
                Thumbnails.Add(bytes, label, mime, selectForAi, filePath);

                // When loading a new "Original" image (not AI-generated), save it as the session's original backup
                if (label == "Original" && bytes != null)
                {
                    _ = Sessions.SaveOriginalImageAsync(bytes, filePath ?? "original.png");
                }
            };

            // Workspace -> Session: mark dirty on changes
            Workspace.OnWorkspaceChanged += () => Sessions.MarkDirty();
            
            // Workspace -> Session: immediate save when workspace is cleared
            Workspace.OnWorkspaceClearRequested += () => _ = Sessions.SaveCurrentSessionAsync();

            // Workspace: re-apply category-controlled flags after adjustments reset
            Workspace.OnAdjustmentsReset += () => ReapplyCategoryFlags();

            // Workspace: check unsaved changes delegate
            Workspace.CheckUnsavedChangesAsync = CheckUnsavedChangesAsync;

            // Thumbnails -> Workspace: load selected thumbnail to workspace
            Thumbnails.LoadRequested += async (item) =>
            {
                if (item?.RawBytes != null)
                    await Workspace.LoadFromThumbnailAsync(item.RawBytes, item.Label, item.MimeType, item.FilePath);
            };

            // Thumbnails: mark dirty on changes
            Thumbnails.OnChanged += () => Sessions.MarkDirty();

            // Thumbnails: save collapse state
            Thumbnails.OnCollapseStateChanged = (collapsed) =>
            {
                if (AppSettings.Current.ImageEditorSessions != null)
                {
                    AppSettings.Current.ImageEditorSessions.ThumbnailStripCollapsed = collapsed;
                    AppSettings.Save();
                }
            };

            // Sessions: get unsaved thumbnail names for destructive action confirmation
            Sessions.GetUnsavedThumbnailNames = () =>
            {
                return Thumbnails.GetUnsavedThumbnails().Select(t => t.Label ?? "Unnamed").ToList();
            };

            // AI -> Workspace: get images for analysis
            Ai.GetImagesForAi = () => Thumbnails.GetMarkedForAi();

            // AI -> Workspace: get current workspace image
            Ai.GetWorkspaceImage = () =>
            {
                var file = Workspace.WorkspaceFile;
                return file?.RawBytes != null ? (file.RawBytes, file.OriginalFormat) : (null, null);
            };

            // AI -> Workspace: load generated image
            Ai.OnImageGenerated += (bytes, label, mime) =>
            {
                global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Workspace.LoadGeneratedImage(bytes, label, mime);
                    var thumbLabel = $"Gen #{Thumbnails.ThumbnailItems.Count}";
                    Thumbnails.Add(bytes, thumbLabel, mime, selectForAi: true);
                });
            };

            // AI: push undo before generation
            Ai.PushUndoState = () => Workspace.PushUndoState("Before AI generation");

            // AI: mark dirty on chat changes
            Ai.OnChatChanged += () => Sessions.MarkDirty();

            // Sessions: check unsaved changes
            Sessions.CheckUnsavedChangesAsync = CheckUnsavedChangesAsync;

            // Sessions: clear workspace
            Sessions.ClearWorkspace = () =>
            {
                Workspace.ClearWorkspaceInternal(false);
                Thumbnails.Clear();
                Ai.ChatMessages.Clear();
            };

            // Sessions: capture workspace state
            Sessions.CaptureWorkspaceState = () =>
            {
                var file = Workspace.WorkspaceFile;
                if (file?.RawBytes == null) return null;
                return new SessionWorkspaceState
                {
                    ImageBytes = file.RawBytes,
                    Width = file.OriginalWidth,
                    Height = file.OriginalHeight,
                    FileName = file.FileName,
                    Format = file.OriginalFormat,
                    SourcePath = file.FilePath
                };
            };

            // Sessions: capture thumbnails state
            Sessions.CaptureThumbnailsState = () =>
            {
                var state = new SessionThumbnailsState();
                foreach (var item in Thumbnails.GetAll())
                {
                    if (item.RawBytes != null)
                    {
                        state.Items.Add(new SessionThumbnailItemState
                        {
                            Id = item.Id,
                            Label = item.Label,
                            ImageBytes = item.RawBytes,
                            MimeType = item.MimeType,
                            SendToAi = item.SendToAi,
                            CreatedAt = item.CreatedAt,
                            SavedAt = item.SavedAt
                        });
                    }
                }
                return state;
            };

            // Sessions: capture AI state
            Sessions.CaptureAiState = () => new SessionAiState
            {
                ChatHistory = Ai.SerializeChatHistory(),
                SelectedConnection = Ai.SelectedAiConnection
            };

            // Sessions: restore workspace state (caller handles UI thread dispatch)
            Sessions.RestoreWorkspaceState = (state) =>
            {
                if (state?.ImageBytes != null)
                    Workspace.RestoreFromBytes(state.ImageBytes, state.Width, state.Height, state.FileName, state.Format, state.SourcePath);
            };

            // Sessions: restore thumbnails state (with cached previews if available)
            Sessions.RestoreThumbnailsState = (state) =>
            {
                if (state?.Items != null)
                {
                    foreach (var item in state.Items)
                        Thumbnails.RestoreItem(item.Id, item.Label, item.ImageBytes, item.MimeType, item.SendToAi, item.CreatedAt, item.SavedAt, item.PreviewBytes);
                }
            };

            // Sessions: restore AI state
            Sessions.RestoreAiState = (state) =>
            {
                Ai.RestoreChatHistory(state?.ChatHistory, state?.SelectedConnection);
            };

            // Sessions: revert to original
            Sessions.RevertToOriginalAsync = async (originalBytes) =>
            {
                Workspace.PushUndoState("Before revert to original");
                Workspace.RestoreFromBytes(originalBytes, 0, 0, "original.png", "PNG", null);
            };

            // Sessions: load thumbnail to workspace (when restoring session without workspace image)
            Sessions.LoadThumbnailToWorkspace = (thumbState) =>
            {
                if (thumbState?.ImageBytes != null)
                {
                    // Extract format from mime type (e.g., "image/png" -> "PNG")
                    var format = "PNG";
                    if (!string.IsNullOrEmpty(thumbState.MimeType))
                    {
                        var parts = thumbState.MimeType.Split('/');
                        if (parts.Length > 1)
                            format = parts[parts.Length - 1].ToUpper();
                    }
                    // Call synchronously - we're already being invoked, no need for extra dispatcher
                    Workspace.RestoreFromBytes(thumbState.ImageBytes, 0, 0, thumbState.Label ?? "restored.png", format, null);
                }
            };

            // Forward property changes from children to root (for XAML bindings)
            Workspace.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
            Thumbnails.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
            Batch.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
            Ai.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
            Sessions.PropertyChanged += (s, e) => OnPropertyChanged(e.PropertyName);
        }
    }
}
