# ViewModel Testing Guide

## Table of Contents

1. [Introduction](#introduction)
2. [Architecture Overview](#architecture-overview)
3. [The Problem: Why ViewModels Are Hard to Test](#the-problem-why-viewmodels-are-hard-to-test)
4. [The Solution: Dependency Injection & Abstractions](#the-solution-dependency-injection--abstractions)
5. [Service Interfaces](#service-interfaces)
6. [Mock Implementations](#mock-implementations)
7. [Testing Patterns](#testing-patterns)
8. [Complete Examples](#complete-examples)
9. [Migration Guide](#migration-guide)
10. [Best Practices](#best-practices)

---

## Introduction

This document provides a comprehensive guide to testing ViewModels in the OpenSourceToolkit.NET application. The Image Converter module serves as the primary example, but these patterns apply to all ViewModels in the application.

### Goals

- **Unit testable ViewModels** - Test business logic without UI infrastructure
- **Fast test execution** - No Avalonia initialization required
- **Isolated tests** - Each test runs independently with controlled dependencies
- **High coverage** - Test all state transitions, commands, and edge cases

---

## Architecture Overview

### Current Architecture

```diagram
┌─────────────────────────────────────────────────────────────────────────────┐
│                              APPLICATION LAYERS                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                         VIEW LAYER (XAML)                           │    │
│  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐      │    │
│  │  │ ImageConverter  │  │  Batch Convert  │  │   AI Assistant  │      │    │
│  │  │   ToolView      │  │     Panel       │  │     Panel       │      │    │
│  │  └────────┬────────┘  └────────┬────────┘  └────────┬────────┘      │    │
│  │           │                    │                    │               │    │
│  │           └────────────────────┼────────────────────┘               │    │
│  │                                │                                    │    │
│  │                    ┌───────────▼───────────┐                        │    │
│  │                    │   Code-Behind (.cs)   │                        │    │
│  │                    │  - Wire Actions       │                        │    │
│  │                    │  - File Dialogs       │                        │    │
│  │                    │  - Clipboard          │                        │    │
│  │                    └───────────┬───────────┘                        │    │
│  └────────────────────────────────┼────────────────────────────────────┘    │
│                                   │                                         │
│                                   │ Delegates/Actions                       │
│                                   ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                      VIEWMODEL LAYER                                │    │
│  │                                                                     │    │
│  │  ┌─────────────────────────────────────────────────────────────┐    │    │
│  │  │              ImageConverterToolViewModel                    │    │    │
│  │  │                    (Orchestrator)                           │    │    │
│  │  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐        │    │    │
│  │  │  │Workspace │ │Thumbnails│ │  Batch   │ │    AI    │        │    │    │
│  │  │  │ Editor   │ │  Strip   │ │Conversion│ │Assistant │        │    │    │
│  │  │  │   VM     │ │   VM     │ │   VM     │ │   VM     │        │    │    │
│  │  │  └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘        │    │    │
│  │  │       │            │            │            │              │    │    │
│  │  │       └────────────┼────────────┼────────────┘              │    │    │
│  │  │                    │            │                           │    │    │
│  │  │            ┌───────▼────────────▼───────┐                   │    │    │
│  │  │            │    SessionController       │                   │    │    │
│  │  │            │  (Persistence/Autosave)    │                   │    │    │
│  │  │            └────────────────────────────┘                   │    │    │
│  │  └─────────────────────────────────────────────────────────────┘    │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                   │                                         │
│                                   │ Direct Dependencies                     │
│                                   ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                       SERVICE LAYER                                 │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │    │
│  │  │ImageProcessor│  │  AiService   │  │SessionStorage│               │    │
│  │  │  (Concrete)  │  │  (Concrete)  │  │   Service    │               │    │
│  │  └──────────────┘  └──────────────┘  └──────────────┘               │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Testable Architecture (Target)

```diagram
┌─────────────────────────────────────────────────────────────────────────────┐
│                         TESTABLE ARCHITECTURE                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    VIEW LAYER (Production Only)                     │    │
│  │                                                                     │    │
│  │    Wires concrete implementations:                                  │    │
│  │    - AvaloniaDispatcherService                                      │    │
│  │    - FileService                                                    │    │
│  │    - ImageProcessingService                                         │    │
│  │    - Dialog delegates (file pickers, confirmations)                 │    │
│  │                                                                     │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                   │                                         │
│                                   │ Interfaces                              │
│                                   ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                      VIEWMODEL LAYER                                │    │
│  │                                                                     │    │
│  │    Dependencies injected via constructor:                           │    │
│  │    - IDispatcherService                                             │    │
│  │    - IFileService                                                   │    │
│  │    - IImageProcessingService                                        │    │
│  │    - ISessionStorageService                                         │    │
│  │    - IAiService                                                     │    │
│  │                                                                     │    │
│  │    UI interactions via delegates:                                   │    │
│  │    - Func<string, Task<string>> SaveFileAction                      │    │
│  │    - Func<List<string>, string, Task<bool>> ConfirmAction           │    │
│  │    - Action<string> ShowErrorAction                                 │    │
│  │                                                                     │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                   │                                         │
│                                   │ Interfaces                              │
│                                   ▼                                         │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    SERVICE INTERFACES                               │    │
│  │  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐   │    │
│  │  │IImageProcessing  │  │  IAiService      │  │ISessionStorage   │   │    │
│  │  │    Service       │  │                  │  │    Service       │   │    │
│  │  └──────────────────┘  └──────────────────┘  └──────────────────┘   │    │
│  │  ┌──────────────────┐  ┌──────────────────┐                         │    │
│  │  │IDispatcherService│  │  IFileService    │                         │    │
│  │  └──────────────────┘  └──────────────────┘                         │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                           TEST ENVIRONMENT                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                         TEST CLASS                                  │    │
│  │                                                                     │    │
│  │    Injects mock implementations:                                    │    │
│  │    - SynchronousDispatcherService                                   │    │
│  │    - MockFileService                                                │    │
│  │    - MockImageProcessingService                                     │    │
│  │    - Lambda delegates for confirmations                             │    │
│  │                                                                     │    │
│  │    ┌─────────────────────────────────────────────────────────────┐  │    │
│  │    │                    ViewModel Under Test                     │  │    │
│  │    │                                                             │  │    │
│  │    │  All Avalonia dependencies replaced with synchronous mocks  │  │    │
│  │    │  All file I/O replaced with in-memory operations            │  │    │
│  │    │  All dialogs replaced with predetermined responses          │  │    │
│  │    │                                                             │  │    │
│  │    └─────────────────────────────────────────────────────────────┘  │    │
│  │                                                                     │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## The Problem: Why ViewModels Are Hard to Test

### Problem 1: Avalonia Dispatcher Dependency

```csharp
// ❌ PROBLEM: Requires Avalonia initialization
public void Add(byte[] imageBytes, string label, ...)
{
    byte[] thumbnailBytes = _imageProcessor.ProcessImage(imageBytes, options);

    // This fails in tests - Dispatcher.UIThread is null without Avalonia
    global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
    {
        ThumbnailItems.Add(item);
    });
}
```

**Error in tests:**

```csharp
System.NullReferenceException: Object reference not set to an instance of an object.
   at Avalonia.Threading.Dispatcher.get_UIThread()
```

### Problem 2: Avalonia Bitmap Dependency

```csharp
// ❌ PROBLEM: Bitmap requires Avalonia rendering infrastructure
var item = new ThumbnailItem
{
    // Creating Bitmap throws without Avalonia initialization
    Thumbnail = new Bitmap(memoryStream),
    ...
};
```

**Error in tests:**

```csharp
Avalonia.Platform.PlatformNotSupportedException: No rendering subsystem initialized
```

### Problem 3: Direct File I/O

```csharp
// ❌ PROBLEM: Creates actual files, tests are not isolated
private async Task SaveFullImageAsync(ThumbnailItem item)
{
    File.WriteAllBytes(outputPath, item.RawBytes);
}
```

**Issues:**

- Tests modify file system
- Tests depend on specific paths existing
- Tests are slow due to I/O
- Cleanup required after tests

### Problem 4: Concrete Dependencies

```csharp
// ❌ PROBLEM: Cannot substitute with mock
public ThumbnailStripViewModel(ImageProcessor imageProcessor)
{
    _imageProcessor = imageProcessor;  // Concrete class, not interface
}
```

**Issues:**

- Cannot verify method calls
- Cannot return controlled test data
- Actual image processing runs (slow)

---

## The Solution: Dependency Injection & Abstractions

### Solution Diagram

```diagram
┌─────────────────────────────────────────────────────────────────────────────┐
│                        DEPENDENCY INJECTION FLOW                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   PRODUCTION                              TESTING                           │
│   ══════════                              ═══════                           │
│                                                                             │
│   ┌─────────────────────┐                 ┌─────────────────────┐           │
│   │   View Code-Behind  │                 │     Test Class      │           │
│   └──────────┬──────────┘                 └──────────┬──────────┘           │
│              │                                       │                      │
│              │ Creates                               │ Creates              │
│              ▼                                       ▼                      │
│   ┌─────────────────────┐                 ┌─────────────────────┐           │
│   │AvaloniaDispatcher   │                 │SynchronousDispatcher│           │
│   │    Service          │                 │      Service        │           │
│   └──────────┬──────────┘                 └──────────┬──────────┘           │
│              │                                       │                      │
│              │ Implements                            │ Implements           │
│              ▼                                       ▼                      │
│   ┌─────────────────────────────────────────────────────────────┐           │
│   │                    IDispatcherService                       │           │
│   │  ┌─────────────────────────────────────────────────────┐    │           │
│   │  │  void Post(Action action)                           │    │           │
│   │  │  Task InvokeAsync(Action action)                    │    │           │
│   │  │  Task<T> InvokeAsync<T>(Func<T> func)               │    │           │
│   │  └─────────────────────────────────────────────────────┘    │           │
│   └─────────────────────────────────────────────────────────────┘           │
│                              │                                              │
│                              │ Injected into                                │
│                              ▼                                              │
│   ┌──────────────────────────────────────────────────────────────┐          │
│   │                       ViewModel                              │          │
│   │                                                              │          │
│   │  public ThumbnailStripViewModel(                             │          │
│   │      IImageProcessingService imageService,                   │          │
│   │      IDispatcherService dispatcher)                          │          │
│   │  {                                                           │          │
│   │      _imageService = imageService;                           │          │
│   │      _dispatcher = dispatcher;                               │          │
│   │  }                                                           │          │
│   │                                                              │          │
│   └──────────────────────────────────────────────────────────────┘          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### The Delegate Pattern (Already Implemented!)

The codebase already uses an excellent pattern for UI interactions:

```diagram
┌─────────────────────────────────────────────────────────────────────────────┐
│                         DELEGATE PATTERN FLOW                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌─────────────────────────────────────────────────────────────────────┐   │
│   │                         ViewModel                                   │   │
│   │                                                                     │   │
│   │  // Delegate property - no implementation                           │   │
│   │  public Func<List<string>, string, Task<bool>>                      │   │
│   │      ConfirmDestructiveActionAsync { get; set; }                    │   │
│   │                                                                     │   │
│   │  // Usage in business logic                                         │   │
│   │  private async void ClearWithConfirmation()                         │   │
│   │  {                                                                  │   │
│   │      var unsaved = GetUnsavedThumbnails();                          │   │
│   │      if (unsaved.Count > 0 && ConfirmDestructiveActionAsync != null)│   │
│   │      {                                                              │   │
│   │          var names = unsaved.Select(t => t.Label).ToList();         │   │
│   │          bool confirmed = await ConfirmDestructiveActionAsync(      │   │
│   │              names, "Clear All");                                   │   │
│   │          if (!confirmed) return;                                    │   │
│   │      }                                                              │   │
│   │      Clear();                                                       │   │
│   │  }                                                                  │   │
│   │                                                                     │   │
│   └─────────────────────────────────────────────────────────────────────┘   │
│                              │                                              │
│              ┌───────────────┴───────────────┐                              │
│              │                               │                              │
│              ▼                               ▼                              │
│   ┌──────────────────────┐         ┌──────────────────────┐                 │
│   │   PRODUCTION         │         │      TESTING         │                 │
│   │                      │         │                      │                 │
│   │ // View code-behind  │         │ // Test setup        │                 │
│   │ vm.ConfirmDestructive│         │ vm.ConfirmDestructive│                 │
│   │   ActionAsync =      │         │   ActionAsync =      │                 │
│   │   ConfirmDestructive │         │   (names, action) => │                 │
│   │   Action;            │         │   Task.FromResult(   │                 │
│   │                      │         │       true);         │                 │
│   │ // Shows dialog,     │         │                      │                 │
│   │ // waits for user    │         │ // Returns           │                 │
│   │                      │         │ // immediately       │                 │
│   └──────────────────────┘         └──────────────────────┘                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Service Interfaces

### IDispatcherService

**File:** `Services/IDispatcherService.cs`

```csharp
/// <summary>
/// Abstraction for UI thread dispatching.
/// Allows ViewModels to be tested without Avalonia dependency.
/// </summary>
public interface IDispatcherService
{
    /// <summary>
    /// Posts an action to the UI thread asynchronously (fire and forget).
    /// </summary>
    void Post(Action action);

    /// <summary>
    /// Invokes an action on the UI thread and waits for completion.
    /// </summary>
    Task InvokeAsync(Action action);

    /// <summary>
    /// Invokes a function on the UI thread and returns the result.
    /// </summary>
    Task<T> InvokeAsync<T>(Func<T> func);
}
```

**Implementations:**

| Class | Environment | Behavior |
|-------|-------------|----------|
| `AvaloniaDispatcherService` | Production | Marshals to `Dispatcher.UIThread` |
| `SynchronousDispatcherService` | Testing | Executes immediately on calling thread |

### IFileService

**File:** `Services/IFileService.cs`

```csharp
/// <summary>
/// Abstraction for file system operations.
/// Allows ViewModels to be tested without actual file I/O.
/// </summary>
public interface IFileService
{
    byte[] ReadAllBytes(string path);
    void WriteAllBytes(string path, byte[] bytes);
    bool FileExists(string path);
    FileInfo GetFileInfo(string path);
}
```

**Implementations:**

| Class | Environment | Behavior |
|-------|-------------|----------|
| `FileService` | Production | Uses `System.IO.File` |
| `MockFileService` | Testing | In-memory dictionary storage |

### IImageProcessingService

**File:** `Services/IImageProcessingService.cs`

```csharp
/// <summary>
/// Abstraction for image processing operations.
/// </summary>
public interface IImageProcessingService
{
    byte[] ProcessImage(byte[] inputBytes, ImageProcessingOptions options);
    byte[] ConvertToPreviewPng(byte[] inputBytes);
    byte[] ConvertToAiPng(byte[] inputBytes);
    byte[] CreateThumbnail(byte[] inputBytes, int maxWidth, int maxHeight);
}
```

**Implementations:**

| Class | Environment | Behavior |
|-------|-------------|----------|
| `ImageProcessingService` | Production | Wraps `ImageProcessor` |
| `MockImageProcessingService` | Testing | Returns predefined test bytes |

---

## Mock Implementations

### MockFileService

**File:** `Tests/Mocks/MockFileService.cs`

```csharp
public class MockFileService : IFileService
{
    private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();

    /// <summary>
    /// Gets all files that were written during the test.
    /// </summary>
    public IReadOnlyDictionary<string, byte[]> WrittenFiles => _files;

    /// <summary>
    /// Pre-populates a file for reading.
    /// </summary>
    public void SetupFile(string path, byte[] content)
    {
        _files[path] = content;
    }

    public byte[] ReadAllBytes(string path)
    {
        if (_files.TryGetValue(path, out var bytes))
            return bytes;
        throw new FileNotFoundException($"File not found: {path}");
    }

    public void WriteAllBytes(string path, byte[] bytes)
    {
        _files[path] = bytes;
    }

    public bool FileExists(string path) => _files.ContainsKey(path);
}
```

**Usage in tests:**

```csharp
[TestMethod]
public void SaveImage_WritesToCorrectPath()
{
    var mockFileService = new MockFileService();
    var vm = CreateViewModel(fileService: mockFileService);

    vm.SaveImageToPath(@"C:\test\image.png", testBytes);

    Assert.IsTrue(mockFileService.WrittenFiles.ContainsKey(@"C:\test\image.png"));
    CollectionAssert.AreEqual(testBytes, mockFileService.WrittenFiles[@"C:\test\image.png"]);
}
```

### MockImageProcessingService

**File:** `Tests/Mocks/MockImageProcessingService.cs`

```csharp
public class MockImageProcessingService : IImageProcessingService
{
    private readonly byte[] _defaultOutput;

    /// <summary>
    /// Tracks all ProcessImage calls for verification.
    /// </summary>
    public List<(byte[] Input, ImageProcessingOptions Options)> ProcessImageCalls { get; }
        = new List<(byte[], ImageProcessingOptions)>();

    public MockImageProcessingService(byte[] defaultOutput = null)
    {
        _defaultOutput = defaultOutput ?? new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
    }

    public byte[] ProcessImage(byte[] inputBytes, ImageProcessingOptions options)
    {
        ProcessImageCalls.Add((inputBytes, options));
        return _defaultOutput;
    }

    // ... other methods return _defaultOutput
}
```

**Usage in tests:**

```csharp
[TestMethod]
public void Add_ProcessesImageWithCorrectOptions()
{
    var mockImageService = new MockImageProcessingService();
    var vm = CreateViewModel(imageService: mockImageService);

    vm.Add(testBytes, "Test", "image/png");

    Assert.AreEqual(1, mockImageService.ProcessImageCalls.Count);
    var call = mockImageService.ProcessImageCalls[0];
    Assert.AreEqual(80, call.Options.Width);  // Thumbnail size
    Assert.AreEqual(80, call.Options.Height);
}
```

---

## Testing Patterns

### Pattern 1: Testing Property Changes

```diagram
┌─────────────────────────────────────────────────────────────────────────────┐
│                    PROPERTY CHANGE NOTIFICATION TEST                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   TEST SETUP                           EXECUTION                            │
│   ══════════                           ═════════                            │
│                                                                             │
│   ┌─────────────────┐                  ┌─────────────────┐                  │
│   │ Create ViewModel│                  │ Change Property │                  │
│   └────────┬────────┘                  └────────┬────────┘                  │
│            │                                    │                           │
│            ▼                                    ▼                           │
│   ┌─────────────────┐                  ┌─────────────────┐                  │
│   │ Subscribe to    │                  │ PropertyChanged │                  │
│   │ PropertyChanged │                  │ Event Fires     │                  │
│   └────────┬────────┘                  └────────┬────────┘                  │
│            │                                    │                           │
│            ▼                                    ▼                           │
│   ┌─────────────────┐                  ┌─────────────────┐                  │
│   │ Track property  │                  │ Handler adds    │                  │
│   │ names in List   │                  │ name to List    │                  │
│   └─────────────────┘                  └────────┬────────┘                  │
│                                                 │                           │
│                                                 ▼                           │
│                                        ┌─────────────────┐                  │
│                                        │ ASSERT: List    │                  │
│                                        │ contains        │                  │
│                                        │ expected names  │                  │
│                                        └─────────────────┘                  │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Code:**

```csharp
[TestMethod]
public void IsCollapsed_WhenSet_RaisesPropertyChanged()
{
    // Arrange
    var vm = new ThumbnailStripViewModel(_imageProcessor);
    var changedProperties = new List<string>();
    vm.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

    // Act
    vm.IsCollapsed = true;

    // Assert
    Assert.IsTrue(changedProperties.Contains(nameof(vm.IsCollapsed)));
    Assert.IsTrue(changedProperties.Contains(nameof(vm.ThumbnailStripVisible)));
    Assert.IsTrue(changedProperties.Contains(nameof(vm.ShowThumbnailExpandButton)));
}
```

### Pattern 2: Testing Commands

```diagram
┌──────────────────────────────────────────────────────────────────────────────┐
│                         COMMAND EXECUTION TEST                               │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   ┌──────────────────────────────────────────────────────────────────────┐   │
│   │                           TEST FLOW                                  │   │
│   │                                                                      │   │
│   │    ┌──────────┐    ┌──────────┐    ┌──────────┐     ┌──────────┐     │   │
│   │    │  Setup   │──▶│ Execute  │───▶│  Verify  │───▶│  Assert  │     │   │
│   │    │ViewModel │    │ Command  │    │  State   │     │ Results  │     │   │
│   │    └──────────┘    └──────────┘    └──────────┘     └──────────┘     │   │
│   │                                                                      │   │
│   └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│   EXAMPLE: LoadThumbnailToWorkspaceCommand                                   │
│                                                                              │
│   ┌─────────────────┐                                                        │
│   │ 1. Create VM    │                                                        │
│   │ 2. Subscribe to │                                                        │
│   │    LoadRequested│                                                        │
│   │    event        │                                                        │
│   └────────┬────────┘                                                        │
│            │                                                                 │
│            ▼                                                                 │
│   ┌─────────────────┐                                                        │
│   │ 3. Execute      │                                                        │
│   │    command with │                                                        │
│   │    test item    │                                                        │
│   └────────┬────────┘                                                        │
│            │                                                                 │
│            ▼                                                                 │
│   ┌─────────────────┐                                                        │
│   │ 4. Assert event │                                                        │
│   │    was raised   │                                                        │
│   │    with correct │                                                        │
│   │    item         │                                                        │
│   └─────────────────┘                                                        │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Code:**

```csharp
[TestMethod]
public void LoadThumbnailToWorkspaceCommand_RaisesLoadRequested()
{
    // Arrange
    var vm = new ThumbnailStripViewModel(_imageProcessor);
    var item = CreateTestThumbnailItem("Test");
    ThumbnailItem loadedItem = null;
    vm.LoadRequested += (i) => loadedItem = i;

    // Act
    vm.LoadThumbnailToWorkspaceCommand.Execute(item);

    // Assert
    Assert.IsNotNull(loadedItem);
    Assert.AreEqual("Test", loadedItem.Label);
}
```

### Pattern 3: Testing Async Commands with Delegates

```diagram
┌──────────────────────────────────────────────────────────────────────────────┐
│                    ASYNC COMMAND WITH DELEGATE TEST                          │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   ┌──────────────────────────────────────────────────────────────────────┐   │
│   │                                                                      │   │
│   │   SETUP                                                              │   │
│   │   ┌──────────────────────────────────────────────────────────────┐   │   │
│   │   │ 1. Create ViewModel                                          │   │   │
│   │   │ 2. Wire delegate with controlled response                    │   │   │
│   │   │    vm.SaveFullImageAction = (name) =>                        │   │   │
│   │   │        Task.FromResult(@"C:\test\saved.png");                │   │   │
│   │   └──────────────────────────────────────────────────────────────┘   │   │
│   │                                                                      │   │
│   │   EXECUTION                                                          │   │
│   │   ┌──────────────────────────────────────────────────────────────┐   │   │
│   │   │ 3. Execute async command                                     │   │   │
│   │   │    await vm.SaveThumbnailImageCommand.ExecuteAsync(item);    │   │   │
│   │   └──────────────────────────────────────────────────────────────┘   │   │
│   │                                                                      │   │
│   │   VERIFICATION                                                       │   │
│   │   ┌──────────────────────────────────────────────────────────────┐   │   │
│   │   │ 4. Assert state changes                                      │   │   │
│   │   │    - item.SavedToPath == @"C:\test\saved.png"                │   │   │
│   │   │    - item.SavedAt != null                                    │   │   │
│   │   │    - item.IsSavedOutsideSession == true                      │   │   │
│   │   └──────────────────────────────────────────────────────────────┘   │   │
│   │                                                                      │   │
│   └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Code:**

```csharp
[TestMethod]
public async Task SaveThumbnailImage_UpdatesSavedState_WhenSuccessful()
{
    // Arrange
    var vm = new ThumbnailStripViewModel(_imageProcessor);
    var item = CreateTestThumbnailItem("Test");
    item.RawBytes = _testImageBytes;

    string savedPath = @"C:\test\saved.png";
    vm.SaveFullImageAction = (name) => Task.FromResult(savedPath);

    // Act
    await vm.SaveThumbnailImageCommand.ExecuteAsync(item);

    // Assert
    Assert.AreEqual(savedPath, item.SavedToPath);
    Assert.IsNotNull(item.SavedAt);
    Assert.IsTrue(item.IsSavedOutsideSession);
}
```

### Pattern 4: Testing Confirmation Flows

```diagram
┌──────────────────────────────────────────────────────────────────────────────┐
│                      CONFIRMATION FLOW TEST                                  │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   SCENARIO A: User Confirms                  SCENARIO B: User Cancels        │
│   ═════════════════════════                  ════════════════════════        │
│                                                                              │
│   ┌─────────────────┐                        ┌─────────────────┐             │
│   │ Setup delegate  │                        │ Setup delegate  │             │
│   │ returns TRUE    │                        │ returns FALSE   │             │
│   └────────┬────────┘                        └────────┬────────┘             │
│            │                                          │                      │
│            ▼                                          ▼                      │
│   ┌─────────────────┐                        ┌─────────────────┐             │
│   │ Execute command │                        │ Execute command │             │
│   └────────┬────────┘                        └────────┬────────┘             │
│            │                                          │                      │
│            ▼                                          ▼                      │
│   ┌─────────────────┐                        ┌─────────────────┐             │
│   │ Action PROCEEDS │                        │ Action BLOCKED  │             │
│   │ (items cleared) │                        │ (items remain)  │             │
│   └─────────────────┘                        └─────────────────┘             │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Code:**

```csharp
[TestMethod]
public void ClearWithConfirmation_DoesNotClear_WhenUserCancels()
{
    // Arrange
    var vm = new ThumbnailStripViewModel(_imageProcessor);
    vm.ThumbnailItems.Add(CreateUnsavedItem());
    vm.ConfirmDestructiveActionAsync = (names, action) => Task.FromResult(false);

    // Act
    vm.ClearAllCommand.Execute(null);

    // Assert - items NOT cleared
    Assert.AreEqual(1, vm.ThumbnailItems.Count);
}

[TestMethod]
public void ClearWithConfirmation_Clears_WhenUserConfirms()
{
    // Arrange
    var vm = new ThumbnailStripViewModel(_imageProcessor);
    vm.ThumbnailItems.Add(CreateUnsavedItem());
    vm.ConfirmDestructiveActionAsync = (names, action) => Task.FromResult(true);

    // Act
    vm.ClearAllCommand.Execute(null);

    // Assert - items cleared
    Assert.AreEqual(0, vm.ThumbnailItems.Count);
}
```

---

## Complete Examples

### Example 1: ThumbnailStripViewModel Tests

**File:** `Tests/ThumbnailStripViewModelTests.cs`

```csharp
[TestClass]
public class ThumbnailStripViewModelTests
{
    private ImageProcessor _imageProcessor;
    private byte[] _testImageBytes;

    [TestInitialize]
    public void Setup()
    {
        _imageProcessor = new ImageProcessor();

        // Create a simple 100x100 red test image
        using (var image = new MagickImage(MagickColors.Red, 100, 100))
        {
            image.Format = MagickFormat.Png;
            _testImageBytes = image.ToByteArray();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Basic Collection Tests
    // ═══════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Constructor_InitializesEmptyCollection()
    {
        var vm = new ThumbnailStripViewModel(_imageProcessor);

        Assert.IsNotNull(vm.ThumbnailItems);
        Assert.AreEqual(0, vm.ThumbnailItems.Count);
        Assert.IsFalse(vm.HasThumbnails);
    }

    [TestMethod]
    public void Clear_RemovesAllItems()
    {
        var vm = new ThumbnailStripViewModel(_imageProcessor);
        vm.ThumbnailItems.Add(CreateTestThumbnailItem("Item1"));
        vm.ThumbnailItems.Add(CreateTestThumbnailItem("Item2"));

        vm.Clear();

        Assert.AreEqual(0, vm.ThumbnailItems.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Unsaved State Tests
    // ═══════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void HasUnsavedThumbnails_TrueWhenItemNotSaved()
    {
        var vm = new ThumbnailStripViewModel(_imageProcessor);
        var item = CreateTestThumbnailItem("Unsaved");
        item.SavedToPath = null;
        vm.ThumbnailItems.Add(item);

        Assert.IsTrue(vm.HasUnsavedThumbnails);
    }

    [TestMethod]
    public void HasUnsavedThumbnails_FalseWhenAllSaved()
    {
        var vm = new ThumbnailStripViewModel(_imageProcessor);
        var item = CreateTestThumbnailItem("Saved");
        item.SavedToPath = @"C:\saved\image.png";
        item.SavedAt = DateTime.Now;
        vm.ThumbnailItems.Add(item);

        Assert.IsFalse(vm.HasUnsavedThumbnails);
    }

    [TestMethod]
    public void GetUnsavedThumbnails_ReturnsOnlyUnsaved()
    {
        var vm = new ThumbnailStripViewModel(_imageProcessor);

        var saved = CreateTestThumbnailItem("Saved");
        saved.SavedToPath = @"C:\saved\image.png";
        saved.SavedAt = DateTime.Now;

        var unsaved = CreateTestThumbnailItem("Unsaved");
        unsaved.SavedToPath = null;

        vm.ThumbnailItems.Add(saved);
        vm.ThumbnailItems.Add(unsaved);

        var result = vm.GetUnsavedThumbnails();

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Unsaved", result[0].Label);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // AI Selection Tests
    // ═══════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void GetMarkedForAi_ReturnsOnlyMarkedItems()
    {
        var vm = new ThumbnailStripViewModel(_imageProcessor);

        var markedItem = CreateTestThumbnailItem("Marked");
        markedItem.SendToAi = true;
        markedItem.RawBytes = _testImageBytes;

        var unmarkedItem = CreateTestThumbnailItem("Unmarked");
        unmarkedItem.SendToAi = false;
        unmarkedItem.RawBytes = _testImageBytes;

        vm.ThumbnailItems.Add(markedItem);
        vm.ThumbnailItems.Add(unmarkedItem);

        var result = vm.GetMarkedForAi();

        Assert.AreEqual(1, result.Count);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Event Tests
    // ═══════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void LoadRequested_RaisedWhenCommandExecuted()
    {
        var vm = new ThumbnailStripViewModel(_imageProcessor);
        var item = CreateTestThumbnailItem("Test");
        ThumbnailItem loadedItem = null;
        vm.LoadRequested += (i) => loadedItem = i;

        vm.LoadThumbnailToWorkspaceCommand.Execute(item);

        Assert.IsNotNull(loadedItem);
        Assert.AreEqual("Test", loadedItem.Label);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════

    private ThumbnailItem CreateTestThumbnailItem(string label)
    {
        return new ThumbnailItem
        {
            Id = Guid.NewGuid().ToString(),
            Label = label,
            RawBytes = _testImageBytes,
            MimeType = "image/png",
            CreatedAt = DateTime.Now
        };
    }
}
```

### Example 2: Testing with Fully Injected Dependencies

```csharp
[TestClass]
public class ThumbnailStripViewModelIntegrationTests
{
    private MockImageProcessingService _mockImageService;
    private SynchronousDispatcherService _syncDispatcher;
    private MockFileService _mockFileService;
    private byte[] _testImageBytes;

    [TestInitialize]
    public void Setup()
    {
        _testImageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        _mockImageService = new MockImageProcessingService(_testImageBytes);
        _syncDispatcher = new SynchronousDispatcherService();
        _mockFileService = new MockFileService();
    }

    // After refactoring ViewModel to accept interfaces:
    private ThumbnailStripViewModel CreateViewModel()
    {
        return new ThumbnailStripViewModel(
            _mockImageService,
            _syncDispatcher,
            _mockFileService);
    }

    [TestMethod]
    public void Add_ProcessesImageWithThumbnailOptions()
    {
        var vm = CreateViewModel();

        vm.Add(_testImageBytes, "Test", "image/png");

        // Verify image processing was called with correct options
        Assert.AreEqual(1, _mockImageService.ProcessImageCalls.Count);
        var call = _mockImageService.ProcessImageCalls[0];
        Assert.AreEqual(80, call.Options.Width);
        Assert.AreEqual(80, call.Options.Height);
        Assert.IsTrue(call.Options.MaintainAspectRatio);
    }

    [TestMethod]
    public async Task SaveFullImage_WritesFileToCorrectPath()
    {
        var vm = CreateViewModel();
        var item = new ThumbnailItem
        {
            Label = "Test",
            RawBytes = _testImageBytes
        };

        string savedPath = @"C:\output\test.png";
        vm.SaveFullImageAction = (name) => Task.FromResult(savedPath);

        await vm.SaveThumbnailImageCommand.ExecuteAsync(item);

        // Verify file was written
        Assert.IsTrue(_mockFileService.WrittenFiles.ContainsKey(savedPath));
        CollectionAssert.AreEqual(_testImageBytes, _mockFileService.WrittenFiles[savedPath]);
    }
}
```

---

## Migration Guide

### Step-by-Step Migration Process

```diagram
┌──────────────────────────────────────────────────────────────────────────────┐
│                         MIGRATION PHASES                                     │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   PHASE 1: Create Interfaces (Non-Breaking)                                  │
│   ═══════════════════════════════════════════                                │
│   ┌──────────────────────────────────────────────────────────────────────┐   │
│   │ 1. Create IDispatcherService interface                               │   │
│   │ 2. Create AvaloniaDispatcherService implementation                   │   │
│   │ 3. Create SynchronousDispatcherService for tests                     │   │
│   │ 4. Create IFileService interface                                     │   │
│   │ 5. Create FileService implementation                                 │   │
│   │ 6. Create IImageProcessingService interface                          │   │
│   │ 7. Create ImageProcessingService implementation                      │   │
│   └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│   PHASE 2: Add Constructor Overloads (Backward Compatible)                   │
│   ════════════════════════════════════════════════════════                   │
│   ┌──────────────────────────────────────────────────────────────────────┐   │
│   │ // Existing constructor (keeps working)                              │   │
│   │ public ThumbnailStripViewModel(ImageProcessor imageProcessor)        │   │
│   │     : this(                                                          │   │
│   │         new ImageProcessingService(imageProcessor),                  │   │
│   │         new AvaloniaDispatcherService())                             │   │
│   │ { }                                                                  │   │
│   │                                                                      │   │
│   │ // New testable constructor                                          │   │
│   │ public ThumbnailStripViewModel(                                      │   │
│   │     IImageProcessingService imageService,                            │   │
│   │     IDispatcherService dispatcher)                                   │   │
│   │ {                                                                    │   │
│   │     _imageService = imageService;                                    │   │
│   │     _dispatcher = dispatcher;                                        │   │
│   │ }                                                                    │   │
│   └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│   PHASE 3: Update Internal Code to Use Interfaces                            │
│   ═══════════════════════════════════════════════                            │
│   ┌──────────────────────────────────────────────────────────────────────┐   │
│   │ // Before                                                            │   │
│   │ byte[] thumbnailBytes = _imageProcessor.ProcessImage(...);           │   │
│   │ global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(...);     │   │
│   │                                                                      │   │
│   │ // After                                                             │   │
│   │ byte[] thumbnailBytes = _imageService.CreateThumbnail(...);          │   │
│   │ _dispatcher.InvokeAsync(...);                                        │   │
│   └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│   PHASE 4: Create Mock Implementations                                       │
│   ════════════════════════════════════                                       │
│   ┌──────────────────────────────────────────────────────────────────────┐   │
│   │ 1. Create MockFileService (in-memory storage)                        │   │
│   │ 2. Create MockImageProcessingService (returns test data)             │   │
│   │ 3. Add verification capabilities (call tracking)                     │   │
│   └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│   PHASE 5: Write Tests                                                       │
│   ═══════════════════                                                        │
│   ┌──────────────────────────────────────────────────────────────────────┐   │
│   │ 1. Unit tests for state management                                   │   │
│   │ 2. Unit tests for commands                                           │   │
│   │ 3. Unit tests for property change notifications                      │   │
│   │ 4. Integration tests for complex flows                               │   │
│   └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Handling the Bitmap Problem

```diagram
┌──────────────────────────────────────────────────────────────────────────────┐
│                         BITMAP SOLUTIONS                                     │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   OPTION A: Store Bytes, Create Bitmap Lazily                                │
│   ═══════════════════════════════════════════                                │
│                                                                              │
│   ┌──────────────────────────────────────────────────────────────────────┐   │
│   │ public class ThumbnailItem                                           │   │
│   │ {                                                                    │   │
│   │     public byte[] ThumbnailBytes { get; set; }                       │   │
│   │                                                                      │   │
│   │     private Bitmap _thumbnail;                                       │   │
│   │     public Bitmap Thumbnail                                          │   │
│   │     {                                                                │   │
│   │         get                                                          │   │
│   │         {                                                            │   │
│   │             if (_thumbnail == null && ThumbnailBytes != null)        │   │
│   │             {                                                        │   │
│   │                 using (var ms = new MemoryStream(ThumbnailBytes))    │   │
│   │                     _thumbnail = new Bitmap(ms);                     │   │
│   │             }                                                        │   │
│   │             return _thumbnail;                                       │   │
│   │         }                                                            │   │
│   │     }                                                                │   │
│   │ }                                                                    │   │
│   └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│   ✓ Tests work with ThumbnailBytes (no Avalonia needed)                     │
│   ✓ Production code uses Thumbnail property (creates Bitmap on demand)      │
│   ✓ XAML binding to Thumbnail works unchanged                               │
│                                                                              │
│   ─────────────────────────────────────────────────────────────────────      │
│                                                                              │
│   OPTION B: Accept Null in Tests                                             │
│   ══════════════════════════════                                             │
│                                                                              │
│   ┌──────────────────────────────────────────────────────────────────────┐   │
│   │ // In tests, Thumbnail is null but business logic still works        │   │
│   │ [TestMethod]                                                         │   │
│   │ public void Add_CreatesItemWithCorrectLabel()                        │   │
│   │ {                                                                    │   │
│   │     var vm = CreateTestViewModel();                                  │   │
│   │     var item = new ThumbnailItem { Label = "Test", RawBytes = ... }; │   │
│   │     vm.ThumbnailItems.Add(item);                                     │   │
│   │                                                                      │   │
│   │     // Thumbnail is null, but we're testing business logic           │   │
│   │     Assert.AreEqual("Test", vm.ThumbnailItems[0].Label);             │   │
│   │ }                                                                    │   │
│   └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│   ✓ Simple approach                                                         │
│   ✓ Tests focus on business logic only                                      │
│   ✗ Cannot test thumbnail generation                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Best Practices

### DO ✓

```diagram
┌─────────────────────────────────────────────────────────────────────────────┐
│                              BEST PRACTICES                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ✓ Use delegates for all UI interactions (dialogs, file pickers)           │
│     ─────────────────────────────────────────────────────────────────       │
│     public Func<string, Task<string>> SaveFileAction { get; set; }          │
│                                                                             │
│   ✓ Inject dependencies via constructor                                     │
│     ─────────────────────────────────────────────────────────────────       │
│     public ViewModel(IService service) { _service = service; }              │
│                                                                             │
│   ✓ Keep ViewModels UI-framework agnostic                                   │
│     ─────────────────────────────────────────────────────────────────       │
│     No direct Avalonia references in business logic                         │
│                                                                             │
│   ✓ Test behavior, not implementation                                       │
│     ─────────────────────────────────────────────────────────────────       │
│     Assert.AreEqual(1, vm.Items.Count);  // What it does                    │
│     // Not: Assert.IsTrue(addMethodWasCalled);  // How it does it           │
│                                                                             │
│   ✓ Use meaningful test names                                               │
│     ─────────────────────────────────────────────────────────────────       │
│     [TestMethod]                                                            │
│     public void Clear_WhenUnsavedImagesExist_ShowsConfirmation()            │
│                                                                             │
│   ✓ Follow Arrange-Act-Assert pattern                                       │
│     ─────────────────────────────────────────────────────────────────       │
│     // Arrange: Setup                                                       │
│     // Act: Execute                                                         │
│     // Assert: Verify                                                       │
│                                                                             │
│   ✓ Test edge cases                                                         │
│     ─────────────────────────────────────────────────────────────────       │
│     - Empty collections                                                     │
│     - Null inputs                                                           │
│     - Boundary values                                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### DON'T ✗

```diagram
┌─────────────────────────────────────────────────────────────────────────────┐
│                              ANTI-PATTERNS                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ✗ Don't use static dependencies                                          │
│     ─────────────────────────────────────────────────────────────────       │
│     // Bad: Can't mock                                                      │
│     File.WriteAllBytes(path, bytes);                                        │
│     // Good: Inject IFileService                                            │
│     _fileService.WriteAllBytes(path, bytes);                                │
│                                                                             │
│   ✗ Don't call UI thread directly                                          │
│     ─────────────────────────────────────────────────────────────────       │
│     // Bad: Requires Avalonia                                               │
│     Dispatcher.UIThread.InvokeAsync(...);                                   │
│     // Good: Inject IDispatcherService                                      │
│     _dispatcher.InvokeAsync(...);                                           │
│                                                                             │
│   ✗ Don't create UI objects in ViewModels                                   │
│     ─────────────────────────────────────────────────────────────────       │
│     // Bad: Requires UI infrastructure                                      │
│     var bitmap = new Bitmap(stream);                                        │
│     // Good: Store bytes, create bitmap in View or lazily                   │
│                                                                             │
│   ✗ Don't test private methods directly                                     │
│     ─────────────────────────────────────────────────────────────────       │
│     // Bad: Testing implementation details                                  │
│     var result = vm.GetType().GetMethod("PrivateMethod", ...)               │
│     // Good: Test through public API                                        │
│                                                                             │
│   ✗ Don't depend on test execution order                                    │
│     ─────────────────────────────────────────────────────────────────       │
│     // Each test should be independent                                      │
│     // Use [TestInitialize] for setup                                       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Test Categories Summary

```diagram
┌──────────────────────────────────────────────────────────────────────────────┐
│                          TEST CATEGORIES                                     │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│   UNIT TESTS (Fast, No UI)                                                   │
│   ════════════════════════                                                   │
│   │                                                                          │
│   ├── State Management                                                       │
│   │   ├── Property changes                                                   │
│   │   ├── Collection operations                                              │
│   │   └── Computed properties                                                │
│   │                                                                          │
│   ├── Commands                                                               │
│   │   ├── CanExecute logic                                                   │
│   │   ├── Execute behavior                                                   │
│   │   └── Parameter handling                                                 │
│   │                                                                          │
│   ├── Events                                                                 │
│   │   ├── PropertyChanged                                                    │
│   │   └── Custom events (LoadRequested, OnChanged)                           │
│   │                                                                          │
│   └── Business Logic                                                         │
│       ├── Validation                                                         │
│       ├── Calculations                                                       │
│       └── Data transformations                                               │
│                                                                              │
│   INTEGRATION TESTS (Slower, May Need Setup)                                 │
│   ══════════════════════════════════════════                                 │
│   │                                                                          │
│   ├── Session Persistence                                                    │
│   │   ├── Save/Load cycle                                                    │
│   │   └── Data integrity                                                     │
│   │                                                                          │
│   ├── Image Processing Pipeline                                              │
│   │   ├── Format conversions                                                 │
│   │   └── Filter applications                                                │
│   │                                                                          │
│   └── File Operations                                                        │
│       ├── Read/Write (using temp directories)                                │
│       └── Error handling                                                     │
│                                                                              │
│   UI TESTS (Requires Avalonia Headless or Manual)                            │
│   ═══════════════════════════════════════════════                            │
│   │                                                                          │
│   ├── XAML Bindings                                                          │
│   ├── Visual State                                                           │
│   └── User Interactions                                                      │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## Files Reference

| File | Purpose |
|------|---------|
| `Services/IDispatcherService.cs` | Dispatcher abstraction + implementations |
| `Services/IFileService.cs` | File I/O abstraction + implementation |
| `Services/IImageProcessingService.cs` | Image processing abstraction |
| `Tests/ThumbnailStripViewModelTests.cs` | Example ViewModel unit tests |
| `Tests/Mocks/MockFileService.cs` | In-memory file service mock |
| `Tests/Mocks/MockImageProcessingService.cs` | Image processing mock |

---

## Conclusion

By following these patterns:

1. **ViewModels become testable** without requiring Avalonia UI infrastructure
2. **Tests run fast** because they don't do actual I/O or image processing
3. **Tests are isolated** because each test controls its own dependencies
4. **Code is more maintainable** because dependencies are explicit

The delegate pattern already in use for dialogs is excellent—continue using it for all UI interactions. The main work is abstracting the Dispatcher and file I/O operations.
