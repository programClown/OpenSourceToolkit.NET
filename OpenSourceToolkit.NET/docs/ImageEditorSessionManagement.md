# Image Editor Session Management - Technical Documentation

## Overview

The Image Editor implements a comprehensive session management system that persists workspace state across application restarts. Sessions store images, thumbnails, AI chat history, and settings in GUID-keyed folders on disk.

**Key Design Decisions:**- Undo history is**in-memory only**(not persisted) - users are prompted to save when switching images

- Sessions auto-save metadata with configurable debounce delay
- Original images are preserved separately from workspace images for "Revert to Original" functionality
- Thumbnail images are stored at full resolution in the session

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          ImageConverterToolViewModel                        │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ Session Management                                                  │    │
│  │  - CurrentSession: ImageEditorSession                               │    │
│  │  - AvailableSessions: ObservableCollection<SessionSummary>          │    │
│  │  - SelectedSessionSummary: SessionSummary (dropdown binding)        │    │
│  │  - Commands: New, Switch, Delete, Save, Rename                      │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ Undo History (in-memory only)                                       │    │
│  │  - _undoHistory: List<UndoHistoryItem> (max 10)                     │    │
│  │  - _undoHistoryIndex: position in history                           │    │
│  │  - Commands: Undo, Redo, RevertToOriginal                           │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        ISessionStorageService                               │
│  - LoadSessionIndexAsync() / SaveSessionIndexAsync()                        │
│  - LoadSessionAsync() / SaveSessionAsync() / DeleteSessionAsync()           │
│  - SaveImageToSessionAsync() / LoadImageFromSessionAsync()                  │
│  - SaveThumbnailToSessionAsync() / LoadThumbnailFromSessionAsync()          │
│  - SaveHistoryImageAsync() / LoadHistoryImageAsync() (reserved)             │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         SessionStorageService                               │
│  Singleton: SessionStorageService.Default                                   │
│  Base Path: %LocalAppData%/OpenSourceToolkit/ImageEditorSessions/           │
└─────────────────────────────────────────────────────────────────────────────┘```

---

## File Structure

```%LocalAppData%/OpenSourceToolkit/
├── settings.json                          # App settings (includes session prefs)
├── .secrets                               # Encrypted API keys
└── ImageEditorSessions/
    ├── sessions-index.json                # Session list and active session ID
    ├── a1b2c3d4e5f6g7h8/                  # Session folder (GUID)
    │   ├── session.json                   # Session metadata
    │   ├── workspace_photo.png            # Current workspace state
    │   ├── original_photo.png             # Pristine original (never modified)
    │   └── thumbnails/                    # Full-resolution gallery images
    │       ├── 000_Original.png
    │       ├── 001_Gen_1.png
    │       └── 002_Cropped.png
    └── i9j0k1l2m3n4o5p6/                  # Another session
        ├── session.json
        └── thumbnails/
            └── 000_Original.png```---

## Data Models

###`ImageEditorSession`Main session class containing all workspace state.

| Property | Type | Description |
|----------|------|-------------|
|`Id`|`string`| GUID identifier (32 hex chars, no hyphens) |
|`CreatedAt`|`DateTime`| Session creation timestamp |
|`LastModifiedAt`|`DateTime`| Last activity timestamp |
|`DisplayName`|`string`| User-visible name (default: "yyyyMMdd-HHmm") |
|`WorkspaceImageFileName`|`string`| Current workspace image filename |
|`OriginalImageFileName`|`string`| Pristine original image filename |
|`OriginalSourcePath`|`string`| Original file path (display only) |
|`Thumbnails`|`List<ThumbnailItemData>`| Thumbnail metadata list |
|`ChatHistory`|`string`| AI chat text |
|`SelectedAiConnection`|`string`| Last used AI connection name |
|`WorkspaceWidth`|`int`| Workspace image width |
|`WorkspaceHeight`|`int`| Workspace image height |
|`OriginalFormat`|`string`| Original image format (PNG, JPEG, etc.) |

###`ThumbnailItemData`Serializable thumbnail metadata (full image stored separately).

| Property | Type | Description |
|----------|------|-------------|
|`Id`|`string`| Unique identifier |
|`Label`|`string`| Display label |
|`ImageFileName`|`string`| Filename in thumbnails folder |
|`MimeType`|`string`| MIME type (image/png, etc.) |
|`SendToAi`|`bool`| Include in AI messages |
|`CreatedAt`|`DateTime`| Creation timestamp |

###`SessionIndex`Index file tracking all sessions.

| Property | Type | Description |
|----------|------|-------------|
|`Sessions`|`List<SessionSummary>`| All session summaries |
|`ActiveSessionId`|`string`| Currently active session ID |

###`SessionSummary`Lightweight session info for dropdown display.

| Property | Type | Description |
|----------|------|-------------|
|`Id`|`string`| Session GUID |
|`DisplayName`|`string`| Display name |
|`CreatedAt`|`DateTime`| Creation timestamp |
|`LastModifiedAt`|`DateTime`| Last modification |
|`ThumbnailCount`|`int`| Number of thumbnails |
|`HasWorkspaceImage`|`bool` | Has workspace image |**Note:**`SessionSummary`implements`Equals`/`GetHashCode`by`Id`for proper ComboBox selection binding.

###`UndoHistoryItem`(In-Memory Only)

Represents a single undo state.

| Property | Type | Description |
|----------|------|-------------|
|`ImageBytes`|`byte[]`| Image data at this state |
|`Description`|`string`| State description |
|`Timestamp`|`DateTime`| When state was saved |
|`Width`|`int`| Image width |
|`Height`|`int`| Image height |

---

## ViewModel Properties & Commands

### Session Properties

| Property | Type | Description |
|----------|------|-------------|
|`CurrentSession`|`ImageEditorSession`| Active session |
|`AvailableSessions`|`ObservableCollection<SessionSummary>`| All sessions for dropdown |
|`SelectedSessionSummary`|`SessionSummary`| Selected item in dropdown |
|`HasCurrentSession`|`bool`| Has active session |
|`CurrentSessionDisplayName`|`string`| Display name or "No Session" |
|`HasMultipleSessions`|`bool`| More than one session exists |

### Session Commands

| Command | Description |
|---------|-------------|
|`NewSessionCommand`| Creates new empty session (prompts to save if unsaved changes) |
|`SwitchSessionCommand`| Loads selected session |
|`DeleteSessionCommand`| Deletes current session |
|`SaveSessionCommand`| Saves current session immediately |
|`RenameSessionCommand`| Opens dialog to rename current session |

### Undo History Properties

| Property | Type | Description |
|----------|------|-------------|
|`CanUndo`|`bool`| Can undo to previous state |
|`CanRedo`|`bool`| Can redo forward |
|`UndoHistoryCount`|`int`| Number of undo states |
|`HasUnsavedChanges`|`bool`| Undo history exists (image modified) |
|`UndoTooltip`|`string`| Tooltip showing undo count |
|`RedoTooltip`|`string`| Tooltip showing redo count |

### Undo Commands

| Command | Description |
|---------|-------------|
|`UndoCommand`| Reverts to previous state |
|`RedoCommand`| Moves forward in history |
|`RevertToOriginalCommand` | Restores pristine original image |

---

## Session Lifecycle

### Initialization (`InitializeSessionAsync`)

Called when the tool loads:

1. Load session index from disk
2. Populate `AvailableSessions`(sorted by`LastModifiedAt`descending)
3. Try to load`LastActiveSessionId` from settings
4. If not found, load most recent session
5. If no sessions exist, create new session

### Creating New Session (`CreateNewSessionAsync`)

1. Check for unsaved image changes (`CheckUnsavedChangesAsync`)
2. If user cancels, abort
3. Save current session metadata if dirty
4. Clear workspace (`ClearWorkspaceInternal`)
5. Create new `ImageEditorSession`with fresh GUID
6. Save to disk immediately
7. Add to`AvailableSessions`at top
8. Update`LastActiveSessionId` in settings

### Loading Session (`LoadSessionAsync`)

1. Check for unsaved changes if switching to different session
2. Save current session if dirty
3. Load session metadata from disk
4. Clear workspace
5. Restore workspace image from `WorkspaceImageFileName`6. Restore thumbnails from`Thumbnails` list
7. Restore AI chat history and connection
8. Update selection in dropdown
9. Clear undo history (fresh start)

### Saving Session (`SaveCurrentSessionAsync`)

1. Save workspace image bytes to `workspace_{name}.{ext}`2. Save original image if present
3. Save all thumbnails to`thumbnails/`folder
4. Update session metadata (thumbnails list, chat, connection)
5. Save`session.json`
6. Update session index

### Deleting Session (`DeleteCurrentSessionAsync`)

1. Remove from `AvailableSessions`2. Delete session folder recursively
3. Update session index
4. Switch to another session or create new

---

## Undo/Redo System

### Design Principles

-**In-memory only**: Undo history is NOT persisted to disk
-**Per-image**: History is cleared when switching images
-**Max 10 states**: Oldest states are discarded when limit reached
-**Prompt on switch**: User prompted to save/discard when switching with unsaved changes

### State Management

-`_undoHistory`: List of `UndoHistoryItem`(index 0 = most recent)
-`_undoHistoryIndex`: Current position (-1 = at tip, >0 = in history)

### Operations**`PushUndoState(description)`**- Called BEFORE destructive operations:

1. If in middle of history, truncate forward states
2. Clone current `WorkspaceFile.RawBytes`
3. Insert at position 0
4. Trim to max size**`ExecuteUndo()`**:
1. If at tip, save current state first (for redo)
2. Increment index
3. Restore from history**`ExecuteRedo()`**:
1. Decrement index
2. Restore from history**`ExecuteRevertToOriginal()`**:
1. Push current state to history
2. Load `OriginalImageFileName`from session storage
3. Restore workspace

### Unsaved Changes Prompt

When`HasUnsavedChanges`is true and user tries to:

- Create new session
- Switch to different session
- Load different image

The`PromptSaveChangesAction`is invoked:

- Returns`true`: Save the image first
- Returns `false`: Discard changes
- Returns `null`: Cancel the operation

---

## Auto-Save Behavior

### Configuration (`ImageEditorSessionSettings`)

| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| `LastActiveSessionId`|`string`| null | Session to restore on startup |
|`AutoSaveSessions`|`bool`| true | Enable auto-save |
|`AutoSaveDelayMs`|`int`| 5000 | Debounce delay in ms |
|`ThumbnailStripCollapsed`|`bool`| false | UI state |

### Trigger Points`MarkSessionDirty()`is called when:

- Workspace image loaded/changed
- Thumbnail added/removed
- AI chat text changed
- AI connection changed

### Debounce Logic

1. Cancel any pending auto-save
2. Create new`CancellationTokenSource`3. Wait`AutoSaveDelayMs`milliseconds
4. If not cancelled and still dirty, call`SaveCurrentSessionAsync()`

### Cleanup

On tool deactivation (`Cleanup()`):

1. Save session if dirty
2. Cancel pending auto-save

---

## UI Components

### Session Selector (AI Sidebar)

Located in the AI category panel:

```
┌─────────────────────────────────┐
│ AI Assistant                    │
├─────────────────────────────────┤
│ Session    [+] [💾] [✏️] [🗑] │
│ ┌─────────────────────────────┐ │
│ │ 20251127-1430  (3 img)     ▼│ │
│ └─────────────────────────────┘ │
│ [   Load Selected Session    ]  │
├─────────────────────────────────┤
│ Connection                      │
│ ┌─────────────────────────────┐ │
│ │ My GPT-4                   ▼│ │
│ └─────────────────────────────┘ │
└─────────────────────────────────┘```### Session Dropdown

- Shows`DisplayName`and`ThumbnailCount`- Sorted by`LastModifiedAt`(most recent first)
- Two-way binding to`SelectedSessionSummary`### Session Renaming

Triggered via`RenameSessionCommand` (pencil icon button in toolbar).**Validation Rules:**- Maximum 50 characters (`MaxSessionNameLength`constant)

- Only Windows-compatible filename characters allowed
- Invalid characters:`< > : " / \ | ?*`
- Cannot end with dot or space
- Cannot be empty or whitespace-only**Dialog Features:**- Pre-filled with current session name

- Live character counter (`X/50`)
- Real-time validation with error messages
- Rename button disabled when validation fails**Implementation:**- `ShowRenameSessionDialogAction`: `Func<string, Task<string>>`delegate wired from View

-`ValidateSessionName(string)`: Static validation method (public for dialog use)

- `RenameCurrentSessionAsync()`: Updates session, refreshes ComboBox, saves immediately

---

## Error Handling

### Graceful Degradation

- If session file missing: Create new session
- If image file missing: Skip (partial restore)
- If index corrupted: Return empty index
- All errors logged to console

### File Operations

- All async operations use `Task.Run()` for .NET 4.7.2 compatibility
- Directories created automatically as needed
- Session deletion is recursive

---

## Integration Points

### View Code-Behind (`ImageConverterToolView.axaml.cs`)

```csharp
protected override void OnDataContextChanged(EventArgs e)
{
    // ... other wiring ...

    // Unsaved changes prompt
    vm.PromptSaveChangesAction = PromptSaveChanges;

    // Session rename dialog
    vm.ShowRenameSessionDialogAction = ShowRenameSessionDialog;

    // Initialize session management_ = vm.InitializeSessionAsync();
}
```### Settings Persistence

Session settings stored in`settings.json`:

```json
{
  "ImageEditorSessions": {
    "LastActiveSessionId": "a1b2c3d4e5f6g7h8",
    "AutoSaveSessions": true,
    "AutoSaveDelayMs": 5000,
    "ThumbnailStripCollapsed": false
  }
}
```

---

## Future Considerations

1.**Session Cleanup**: Option to delete sessions older than X days

2.**Storage Size Display**: Show total session storage size
3.**Export/Import**: Export session as archive, import on another machine
4.**Cloud Sync**: Optional sync to cloud storage
