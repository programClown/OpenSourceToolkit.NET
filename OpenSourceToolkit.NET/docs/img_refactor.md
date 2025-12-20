# Image Converter ViewModel Refactor (child VMs)

## Goals

- Split 4k+ `ImageConverterToolViewModel` into focused child view models to reduce file size and improve maintainability.
- Preserve existing behavior and bindings during transition; minimize regression risk.
- Prepare clear wiring between workspace, thumbnails, AI, batch, and session persistence.

## Target Structure

- Namespace: `OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter`
- Files/Classes:
  - `ImageConverterToolViewModel.cs` (root orchestrator; ToolViewModel overrides; owns children)
  - `WorkspaceEditorViewModel.cs` (single-image state/editing + undo/redo)
  - `BatchConversionViewModel.cs` (batch list + convert/GIF/PDF, rename/resize/output)
  - `ThumbnailStripViewModel.cs` (thumbnails + send-to-AI flags, load/save)
  - `AiAssistantViewModel.cs` (connections, chat, generation/analysis, streaming)
  - `SessionController.cs` (session persistence/autosave, dropdown state)
  - `Models/` folder: `ImageFileModel.cs`, `ThumbnailItem.cs`, `IcoSizePreset.cs` (shared data types)
  - Optional shared DTOs: `OutputSettings.cs`, `AdjustmentSettings.cs`, `WatermarkSettings.cs`

## Responsibilities

- WorkspaceEditorViewModel
  - Workspace image load/save, preview bytes, zoom/compare, adjustments/filters/transform, crop, background removal, watermark, histogram.
  - Undo/redo stack (`MaxUndoHistory`), `HasUnsavedChanges`, `CheckUnsavedChangesAsync` hook.
  - Emits `ThumbnailAddRequested(bytes,label,mime,selectForAi,filePath)`; accepts `LoadFromThumbnailAsync`/`LoadGeneratedImage`.
- BatchConversionViewModel
  - `Files`, `SelectedFile`, resize/output/quality, ICO multi-size, rename pattern.
  - `ConvertAll`, `CreateGif`, `CreatePdf`, `ExtractPdfPages`; actions: `SelectOutputFolderAction`, `SaveGifAction`, `SavePdfAction`.
- ThumbnailStripViewModel
  - `ThumbnailItems`, add/remove/clear/save-full/load-to-workspace, `HasThumbnails`, collapse state.
  - Raises `LoadRequested(ThumbnailItem)`, `OnChanged` for dirty tracking; exposes `GetMarkedForAi`.
- AiAssistantViewModel
  - AI connections list/selection, chat text/history, prompt input, font size; `Send/Abort/Copy/Clear/Save` commands.
  - Image generation/analysis with streaming; delegates `GetImagesForAi`, `OnImageGenerated`, clipboard and error actions.
- SessionController
  - Wraps `ISessionStorageService`; manages `CurrentSession`, `AvailableSessions`, `SelectedSessionSummary`, autosave timer.
  - Calls Workspace/Thumbnails/AI to serialize/restore (pristine image + settings + thumbnails + AI chat).
- Root ImageConverterToolViewModel
  - Tool metadata, Cleanup; owns child instances.
  - Bridges view actions (file pickers, dialogs, clipboard) into children; optionally exposes proxy props/commands during XAML transition.

## Wiring Between Children

- Thumbnails.LoadRequested -> Workspace.LoadFromThumbnailAsync (with unsaved-check delegate).
- Workspace.ThumbnailAddRequested -> Thumbnails.Add.
- AI.GetImagesForAi = Thumbnails.GetMarkedForAi; AI.OnImageGenerated -> Workspace.LoadGeneratedImage + Thumbnails.Add(selectForAi:true).
- SessionController.OnLoad -> Workspace.Restore + Thumbnails.Restore + AI.RestoreChat; OnSave pulls state from the same.
- Dirty tracking: Workspace change, Thumbnail change, AI chat change -> SessionController.MarkDirty.

## XAML Strategy

- Preferred: bind to child properties (`Workspace.Brightness`, `Batch.OutputFormat`, `Thumbnails.ThumbnailItems`, `Ai.AiChatText`, `Sessions.AvailableSessions`).
- Temporary proxies in root allowed to limit churn; remove after bindings updated.
- Update command bindings similarly (`Workspace.ApplyCropCommand`, `Batch.ConvertAllCommand`, `Ai.SendAiMessageCommand`, `Thumbnails.LoadThumbnailToWorkspaceCommand`).

## Implementation Steps (safe order)

1) Move data models to `Models/` (same namespace) — no behavior change.
2) Add child VM class files with constructors/fields; copy logic block-by-block from current VM into the matching class (workspace, batch, thumbnails, AI, session).
3) Instantiate children in root; wire delegates/events; inject shared services (`ImageProcessor`, `IAiService`, `ISessionStorageService`). Keep proxy props/commands initially.
4) Update `ImageConverterToolView.axaml` bindings to nested child props/commands. Remove proxies once build passes.
5) Hook session persistence last: serialize/restore workspace state, thumbnails, AI chat text; enable autosave timer.
6) Verify flows: single-image load/edit/undo, thumbnail add/remove/load, AI chat/generation (mock), batch convert/GIF/PDF, session switch/restore.

## Risks & Mitigations

- Binding churn: use temporary proxies; change XAML in small passes.
- UI-thread access: keep Dispatcher invokes around UI objects in child VMs.
- Autosave/session ordering: implement after core splits; test load/save early.

## Acceptance / Done

- `ImageConverterToolViewModel.cs` shrinks to orchestrator + Tool metadata; no monolithic logic remains.
- Child VMs compile and drive all existing features with updated bindings.
- Sessions save/restore image state, thumbnails, and AI chat without regression.
- Basic manual run checks (load/edit/undo, thumbnail interactions, AI request, batch convert) succeed.

## Child VM Public API (sketch)

```csharp
// WorkspaceEditorViewModel (single-image editing)
public sealed class WorkspaceEditorViewModel
{
  // State
  public byte[] ImageBytes { get; set; }               // pristine or current working bytes
  public bool HasUnsavedChanges { get; }
  public int MaxUndoHistory { get; set; }
  public double ZoomScale { get; set; }
  public bool CompareMode { get; set; }
  public AdjustmentSettings Adjustments { get; }       // brightness/contrast/saturation/... DTO

  // Commands
  public IRelayCommand LoadImageCommand { get; }
  public IRelayCommand SaveImageCommand { get; }
  public IRelayCommand UndoCommand { get; }
  public IRelayCommand RedoCommand { get; }
  public IRelayCommand ApplyCropCommand { get; }
  public IRelayCommand RemoveBackgroundCommand { get; }
  public IRelayCommand AddWatermarkCommand { get; }

  // Events/Delegates (bridge to thumbnails and unsaved-check)
  public event Action<byte[], string, string, bool, string> ThumbnailAddRequested; // (bytes,label,mime,selectForAi,filePathOrNull)
  public Func<Func<Task<bool>>, ThumbnailItem, Task> LoadFromThumbnailAsync { get; set; }
  public Action<byte[], string, string> LoadGeneratedImage { get; set; }
}

// ThumbnailStripViewModel (thumbnail list)
public sealed class ThumbnailStripViewModel
{
  public ObservableCollection<ThumbnailItem> ThumbnailItems { get; }
  public bool HasThumbnails { get; }
  public bool IsCollapsed { get; set; }

  // Commands
  public IRelayCommand AddFromBytesCommand { get; }
  public IRelayCommand RemoveSelectedCommand { get; }
  public IRelayCommand ClearCommand { get; }
  public IRelayCommand SaveAllFullsizeCommand { get; }
  public IRelayCommand LoadToWorkspaceCommand { get; }

  // Wiring
  public event Action<ThumbnailItem> LoadRequested;
  public event Action OnChanged;                        // for dirty tracking
  public Func<IEnumerable<ThumbnailItem>> GetMarkedForAi { get; set; }
}

// BatchConversionViewModel (batch/GIF/PDF)
public sealed class BatchConversionViewModel
{
  public ObservableCollection<ImageFileModel> Files { get; }
  public ImageFileModel SelectedFile { get; set; }
  public OutputSettings Output { get; }                 // format, quality, resize, rename pattern
  public IReadOnlyList<IcoSizePreset> IcoSizePresets { get; }

  // Commands
  public IRelayCommand ConvertAllCommand { get; }
  public IRelayCommand CreateGifCommand { get; }
  public IRelayCommand CreatePdfCommand { get; }
  public IRelayCommand ExtractPdfPagesCommand { get; }

  // External actions
  public Func<Task<string>> SelectOutputFolderAction { get; set; }
  public Func<byte[], Task> SaveGifAction { get; set; }
  public Func<byte[], Task> SavePdfAction { get; set; }
}

// AiAssistantViewModel (connections/chat/generation)
public sealed class AiAssistantViewModel
{
  public ObservableCollection<string> Providers { get; }
  public string SelectedProvider { get; set; }
  public string AiChatText { get; set; }
  public ObservableCollection<string> ChatHistory { get; }
  public double ChatFontSize { get; set; }

  // Commands
  public IRelayCommand SendCommand { get; }
  public IRelayCommand AbortCommand { get; }
  public IRelayCommand CopyLastCommand { get; }
  public IRelayCommand ClearCommand { get; }
  public IRelayCommand SaveChatCommand { get; }

  // Bridges
  public Func<IEnumerable<ThumbnailItem>> GetImagesForAi { get; set; }
  public Action<byte[], string, string> OnImageGenerated { get; set; }
  public Action<string> CopyToClipboardAction { get; set; } // wired in view code-behind
  public Action<string> ShowErrorAction { get; set; }
}

// SessionController (persistence/autosave)
public sealed class SessionController
{
  public string CurrentSession { get; set; }
  public ObservableCollection<string> AvailableSessions { get; }
  public string SelectedSessionSummary { get; }
  public bool IsDirty { get; private set; }

  public IRelayCommand SaveSessionCommand { get; }
  public IRelayCommand LoadSessionCommand { get; }
  public IRelayCommand NewSessionCommand { get; }
  public IRelayCommand DeleteSessionCommand { get; }

  // Orchestration
  public Func<object> CaptureWorkspaceState { get; set; }
  public Func<object> CaptureThumbnailsState { get; set; }
  public Func<object> CaptureAiState { get; set; }
  public Action<object> RestoreWorkspaceState { get; set; }
  public Action<object> RestoreThumbnailsState { get; set; }
  public Action<object> RestoreAiState { get; set; }

  public void MarkDirty() { /* set IsDirty and schedule autosave */ }
}
```

## Command CanExecute and Wiring Notes

- Always call `NotifyCanExecuteChanged()` on every `RelayCommand` affected by a property change.
  - Examples: image load state toggles `Save`, `Undo`, `Redo`, `ApplyCrop`, `RemoveBackground`.
  - Batch selection affects `ConvertAll`, `CreateGif`, `CreatePdf`, `ExtractPdfPages`.
- Root VM remains the single place bridging UI-only actions (file pickers, dialogs, clipboard) into child VMs.
- Dirty tracking must be triggered by: workspace adjustments, thumbnail list edits, AI chat changes.

## XAML Guidance (Avalonia v11)

- Use compiled bindings with `x:DataType` for all `DataTemplate`s (e.g., `x:DataType="vm:ThumbnailItem"`).
- Use `PathIcon` for glyphs from `Themes/Icons.axaml`.
- Replace `TappedGestureRecognizer` with the `DoubleTapped` event. Use `Tag="{Binding}"` to pass context and handle in code-behind.
- Respect single-child containers: wrap multiple elements in a `Grid` or `StackPanel`.
- Prefer app-wide styles (e.g., TextBox defaults) in `App.axaml` instead of per-control duplication.
- Set conservative window defaults (e.g., 600x500) with explicit `MinWidth`/`MinHeight`.

## Session Persistence Model

```json
{
  "version": 1,
  "createdUtc": "2025-01-01T00:00:00Z",
  "workspace": {
    "imageBytesBase64": "<...>",
    "adjustments": { "brightness": 0, "contrast": 0, "saturation": 0, "gamma": 1.0 },
    "compareMode": false,
    "zoomScale": 1.0,
    "undoDepth": 10
  },
  "thumbnails": [
    { "id": "1", "label": "orig", "mime": "image/png", "bytesBase64": "<...>", "markedForAi": true }
  ],
  "ai": {
    "provider": "local",
    "chatHistory": [
      { "role": "user", "content": "Describe the image" },
      { "role": "assistant", "content": "It shows ..." }
    ],
    "promptDraft": ""
  }
}
```

- `SessionController` owns serialization; children provide capture/restore DTOs.
- Include a `version` field to enable future migrations.

## Threading and Cancellation

- Perform heavy image operations off the UI thread. Marshal UI updates via `Dispatcher.UIThread.Post`.
- Keep a `CancellationTokenSource` per long-running operation (AI streaming, background removal, batch conversion).
- Cancel previous operations on re-entry (e.g., a new AI send aborts the previous stream).
- Avoid deep `try/catch`; catch only where user-facing errors or cleanup are required. Surface errors via root-provided `ShowErrorAction`.

## Error Handling and UX

- Child VMs expose `Action<string> ShowErrorAction` for user-visible messages wired by root VM.
- Clipboard access goes through `Action<string> CopyToClipboardAction` in the View’s code-behind.
- Provide visual feedback for active/toggled buttons via explicit classes (e.g., `accent`).

## Migration Checklist

1) Create `ViewModels/Tools/ImageConverter` and `Models` folders; move DTOs first (no behavior change).
2) Create child VM classes; copy logic block-by-block from the original monolith.
3) Instantiate children in root; inject `ImageProcessor`, `IAiService`, `ISessionStorageService`.
4) Add temporary proxy properties/commands on root to preserve existing bindings.
5) Update XAML bindings incrementally to `Workspace.*`, `Batch.*`, `Thumbnails.*`, `Ai.*`, `Sessions.*`.
6) Remove proxies after successful build and manual verification.
7) Implement session autosave and verify session switch/restore ordering.
8) Re-test double-click handlers, clipboard bridges, and app-wide style effects.

## Testing and Verification Plan

- Workspace
  - Load/edit/undo/redo cycles keep `HasUnsavedChanges` accurate; commands enable/disable correctly.
  - Crop/transform/filter produce expected bytes (spot-check histograms).
- Thumbnails
  - Add/remove/clear; load to workspace with unsaved-change prompts; save-all produces correct files.
- AI
  - Chat send/abort; image generation flow adds to thumbnails and workspace; clipboard copy works.
- Batch
  - Convert all, GIF/PDF creation, PDF extraction; rename pattern correctness; ICO multi-size handling.
- Sessions
  - Save/Load/Autosave round-trips workspace, thumbnails, and AI chat without loss.

## Performance and Memory

- Reuse `byte[]` where safe; avoid redundant bitmap decode/encode; dispose native resources promptly.
- Debounce slider-driven adjustments; update previews incrementally.
- Compute histograms off the UI thread; cap thumbnail count or resolution if necessary.

## PR/Delivery Strategy

- PR1: Models + child VM skeletons with wiring (no behavior change).
- PR2: Workspace split + proxies.
- PR3: Thumbnails split + wiring and XAML updates.
- PR4: Batch split + output actions.
- PR5: AI split + streaming/cancellation.
- PR6: Sessions + autosave; JSON versioning.
- PR7: Remove proxies, finalize bindings, polish styles and icons.

---

## Implementation Status (Completed)

### Phase 1: Child VM Creation ✅

All child ViewModels have been created and wired:

| File | Lines | Description |
|------|-------|-------------|
| `WorkspaceEditorViewModel.cs` | ~1600 | Single-image editing, adjustments, filters, crop, watermark, undo/redo |
| `ThumbnailStripViewModel.cs` | ~280 | Thumbnail list, add/remove/clear, SendToAi flag |
| `BatchConversionViewModel.cs` | ~645 | Batch conversion, GIF/PDF creation, rename patterns |
| `AiAssistantViewModel.cs` | ~486 | AI connections, chat, image generation/analysis |
| `SessionController.cs` | ~637 | Session persistence, autosave, capture/restore state |
| `Models/ImageFileModel.cs` | - | Batch file model |
| `Models/ThumbnailItem.cs` | - | Thumbnail item with full-res data |
| `Models/IcoSizePreset.cs` | - | ICO multi-size presets |

### Phase 2: XAML Binding Migration ✅

All XAML bindings updated to use direct child VM paths:

**Namespace Added:**

```xml
xmlns:imgvm="using:OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter"
```

**Binding Changes:**

| Category | Example Before | Example After |
|----------|----------------|---------------|
| Workspace | `{Binding Brightness}` | `{Binding Workspace.Brightness}` |
| Workspace | `{Binding HasWorkspaceImage}` | `{Binding Workspace.HasWorkspaceImage}` |
| Workspace | `{Binding SaveWorkspaceImageCommand}` | `{Binding Workspace.SaveWorkspaceImageCommand}` |
| Thumbnails | `{Binding ThumbnailItems}` | `{Binding Thumbnails.ThumbnailItems}` |
| Thumbnails | `{Binding ToggleThumbnailStripCommand}` | `{Binding Thumbnails.ToggleThumbnailStripCommand}` |
| Batch | `{Binding Files}` | `{Binding Batch.Files}` |
| Batch | `{Binding ConvertAllCommand}` | `{Binding Batch.ConvertAllCommand}` |
| AI | `{Binding AiChatText}` | `{Binding Ai.AiChatText}` |
| AI | `{Binding SendAiMessageCommand}` | `{Binding Ai.SendAiMessageCommand}` |
| Sessions | `{Binding AvailableSessions}` | `{Binding Sessions.AvailableSessions}` |
| Sessions | `{Binding NewSessionCommand}` | `{Binding Sessions.NewSessionCommand}` |

**DataTemplate Commands:**

```xml
<!-- Before -->
Command="{Binding $parent[UserControl].((vm:ImageConverterToolViewModel)DataContext).SaveThumbnailImageCommand}"

<!-- After -->
Command="{Binding $parent[UserControl].((vm:ImageConverterToolViewModel)DataContext).Thumbnails.SaveThumbnailImageCommand}"
```

**Static Property References:**

```xml
<!-- Before -->
{x:Static vm:ImageConverterToolViewModel.ImageGenSizeOptions}

<!-- After -->
{x:Static imgvm:AiAssistantViewModel.ImageGenSizeOptions}
```

### Phase 3: Proxy Removal ✅

Root VM (`ImageConverterToolViewModel.cs`) reduced from ~715 lines to ~470 lines.

**Removed:**

- ~240 lines of proxy properties (Workspace, Thumbnails, Batch, AI, Sessions)
- ~60 lines of proxy commands

**Retained in Root VM:**

- Child VM properties (`Workspace`, `Thumbnails`, `Batch`, `Ai`, `Sessions`)
- Tool category selection logic (left toolbar buttons)
- Combined property: `IsProcessing` (depends on both `Workspace.IsProcessing` and `Batch.IsProcessing`)
- External Actions (for View code-behind wiring)
- Public API methods for external callers
- Constructor and child VM wiring

### Final Architecture

```text
ImageConverterToolViewModel (orchestrator, ~470 lines)
├── Workspace: WorkspaceEditorViewModel (~1600 lines)
├── Thumbnails: ThumbnailStripViewModel (~280 lines)
├── Batch: BatchConversionViewModel (~645 lines)
├── Ai: AiAssistantViewModel (~486 lines)
└── Sessions: SessionController (~637 lines)
```

### Files Modified

1. **`ImageConverterToolView.axaml`** - All bindings updated to direct child VM paths
2. **`ImageConverterToolViewModel.cs`** - Proxy properties/commands removed, only orchestration remains

### Notes

- The `IsProcessing` property remains in root VM as it combines state from multiple children (`Workspace.IsProcessing || Batch.IsProcessing`)
- Property change forwarding from children to root is retained for any remaining proxy needs
- External Actions remain as proxies since they're wired in View code-behind and benefit from single-point configuration
