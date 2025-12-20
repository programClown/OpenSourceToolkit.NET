# ViewModel Testing Strategy

## Overview

This document outlines how to make ViewModels in the Image Converter module (and other UI components) testable without requiring Avalonia UI infrastructure.

## Current Challenges

The ViewModels have several dependencies that make testing difficult:

1. **Avalonia Dispatcher** - `Dispatcher.UIThread.InvokeAsync()` requires Avalonia initialization
2. **Avalonia Bitmap** - Creating `Bitmap` objects requires Avalonia rendering infrastructure
3. **File I/O** - Direct `File.WriteAllBytes()` calls make tests dependent on file system
4. **Concrete Dependencies** - `ImageProcessor` is instantiated directly

## Solution: Dependency Injection + Interfaces

### 1. IDispatcherService

Abstracts UI thread dispatching:

```csharp
public interface IDispatcherService
{
    void Post(Action action);
    Task InvokeAsync(Action action);
    Task<T> InvokeAsync<T>(Func<T> func);
}

// Production: AvaloniaDispatcherService
// Testing: SynchronousDispatcherService (executes immediately on calling thread)
```

### 2. IFileService

Abstracts file system operations:

```csharp
public interface IFileService
{
    byte[] ReadAllBytes(string path);
    void WriteAllBytes(string path, byte[] bytes);
    bool FileExists(string path);
}

// Production: FileService (uses System.IO)
// Testing: MockFileService (in-memory dictionary)
```

### 3. IImageProcessingService

Abstracts image processing:

```csharp
public interface IImageProcessingService
{
    byte[] ProcessImage(byte[] inputBytes, ImageProcessingOptions options);
    byte[] ConvertToPreviewPng(byte[] inputBytes);
    byte[] CreateThumbnail(byte[] inputBytes, int maxWidth, int maxHeight);
}

// Production: ImageProcessingService (wraps ImageProcessor)
// Testing: MockImageProcessingService (returns test data)
```

## Delegate Pattern (Already Implemented!)

The current codebase already uses a good pattern for UI interactions:

```csharp
// In ViewModel
public Func<string, Task<string>> SaveFullImageAction { get; set; }
public Func<List<string>, string, Task<bool>> ConfirmDestructiveActionAsync { get; set; }

// In View code-behind
vm.SaveFullImageAction = SaveFullImage;
vm.ConfirmDestructiveActionAsync = ConfirmDestructiveAction;

// In Tests
vm.SaveFullImageAction = (name) => Task.FromResult(@"C:\test\output.png");
vm.ConfirmDestructiveActionAsync = (names, action) => Task.FromResult(true);
```

This pattern is excellent for testability! Continue using it for all UI interactions.

## Refactoring Example: ThumbnailStripViewModel

### Before (Hard to Test)

```csharp
public void Add(byte[] imageBytes, string label, ...)
{
    byte[] thumbnailBytes = _imageProcessor.ProcessImage(imageBytes, options);

    // Problem: Requires Avalonia initialization
    global::Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
    {
        // Problem: Bitmap requires Avalonia rendering
        using (var ms = new MemoryStream(thumbnailBytes))
        {
            var item = new ThumbnailItem
            {
                Thumbnail = new Bitmap(ms),  // Avalonia dependency!
                ...
            };
            ThumbnailItems.Add(item);
        }
    });
}
```

### After (Testable)

```csharp
public class ThumbnailStripViewModel
{
    private readonly IImageProcessingService _imageService;
    private readonly IDispatcherService _dispatcher;

    // Constructor injection
    public ThumbnailStripViewModel(
        IImageProcessingService imageService,
        IDispatcherService dispatcher = null)
    {
        _imageService = imageService;
        _dispatcher = dispatcher ?? new AvaloniaDispatcherService();
    }

    public void Add(byte[] imageBytes, string label, ...)
    {
        byte[] thumbnailBytes = _imageService.CreateThumbnail(imageBytes, 80, 80);

        _dispatcher.InvokeAsync(() =>
        {
            var item = new ThumbnailItem
            {
                ThumbnailBytes = thumbnailBytes,  // Store bytes, not Bitmap
                ...
            };
            ThumbnailItems.Add(item);
        });
    }
}
```

### In Tests

```csharp
[TestMethod]
public void Add_CreatesItemWithCorrectLabel()
{
    var mockImageService = new MockImageProcessingService();
    var syncDispatcher = new SynchronousDispatcherService();
    var vm = new ThumbnailStripViewModel(mockImageService, syncDispatcher);

    vm.Add(testBytes, "Test Label", "image/png");

    Assert.AreEqual(1, vm.ThumbnailItems.Count);
    Assert.AreEqual("Test Label", vm.ThumbnailItems[0].Label);
}
```

## Handling Bitmap in Tests

The `ThumbnailItem.Thumbnail` property is `Bitmap`, which requires Avalonia. Options:

### Option A: Store bytes, create Bitmap lazily in View

```csharp
public class ThumbnailItem
{
    public byte[] ThumbnailBytes { get; set; }

    // Bitmap is created only when needed in View
    private Bitmap _thumbnail;
    public Bitmap Thumbnail
    {
        get
        {
            if (_thumbnail == null && ThumbnailBytes != null)
            {
                using (var ms = new MemoryStream(ThumbnailBytes))
                    _thumbnail = new Bitmap(ms);
            }
            return _thumbnail;
        }
    }
}
```

### Option B: Use IBitmapFactory interface

```csharp
public interface IBitmapFactory
{
    object CreateBitmap(byte[] bytes);  // Returns Bitmap in prod, null in tests
}
```

### Option C: Accept that Thumbnail is null in tests

Tests focus on business logic; UI binding is tested via integration tests.

## Test Categories

### Unit Tests (Fast, No UI)

- ViewModel state management
- Collection operations
- Delegate invocations
- Property change notifications
- Business logic validation

### Integration Tests (Slower, May Need UI)

- Full session save/restore cycle
- Image processing pipelines
- File system interactions (use temp directories)

## Example Test Patterns

### Testing State Changes

```csharp
[TestMethod]
public void IsCollapsed_WhenSet_RaisesPropertyChanged()
{
    var vm = CreateTestViewModel();
    var changedProperties = new List<string>();
    vm.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

    vm.IsCollapsed = true;

    Assert.IsTrue(changedProperties.Contains(nameof(vm.IsCollapsed)));
}
```

### Testing Delegate Invocation

```csharp
[TestMethod]
public async Task SaveFullImage_TracksPath_WhenSuccessful()
{
    var vm = CreateTestViewModel();
    string savedPath = null;
    vm.SaveFullImageAction = (name) =>
    {
        savedPath = $@"C:\test\{name}.png";
        return Task.FromResult(savedPath);
    };

    var item = CreateTestItem();
    await vm.SaveThumbnailImageCommand.ExecuteAsync(item);

    Assert.IsNotNull(item.SavedToPath);
    Assert.AreEqual(savedPath, item.SavedToPath);
}
```

### Testing Confirmation Flow

```csharp
[TestMethod]
public async Task ClearWithConfirmation_DoesNotClear_WhenCancelled()
{
    var vm = CreateTestViewModel();
    vm.ThumbnailItems.Add(CreateUnsavedItem());
    vm.ConfirmDestructiveActionAsync = (names, action) => Task.FromResult(false);

    vm.ClearAllCommand.Execute(null);

    Assert.AreEqual(1, vm.ThumbnailItems.Count);  // Not cleared
}

[TestMethod]
public async Task ClearWithConfirmation_Clears_WhenConfirmed()
{
    var vm = CreateTestViewModel();
    vm.ThumbnailItems.Add(CreateUnsavedItem());
    vm.ConfirmDestructiveActionAsync = (names, action) => Task.FromResult(true);

    vm.ClearAllCommand.Execute(null);

    Assert.AreEqual(0, vm.ThumbnailItems.Count);  // Cleared
}
```

## Migration Path

1. **Phase 1**: Create interfaces (`IDispatcherService`, `IFileService`, `IImageProcessingService`)
2. **Phase 2**: Add production implementations
3. **Phase 3**: Add constructor overloads that accept interfaces (backward compatible)
4. **Phase 4**: Write tests using mocks
5. **Phase 5**: Gradually refactor ViewModels to use injected services

## Files Created

- `Services/IDispatcherService.cs` - Dispatcher abstraction + implementations
- `Services/IFileService.cs` - File I/O abstraction + implementation
- `Services/IImageProcessingService.cs` - Image processing abstraction
- `Tests/ThumbnailStripViewModelTests.cs` - Example ViewModel tests
- `Tests/Mocks/MockFileService.cs` - Mock for file operations
- `Tests/Mocks/MockImageProcessingService.cs` - Mock for image processing

## Best Practices

1. **Keep ViewModels UI-agnostic** - No direct Avalonia references in business logic
2. **Use delegates for dialogs** - Already doing this well!
3. **Inject dependencies** - Makes testing and future changes easier
4. **Test behavior, not implementation** - Focus on what the code does, not how
5. **Use synchronous dispatcher in tests** - Avoids async complexity in unit tests
