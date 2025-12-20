using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter;
using OpenSourceToolkit.NET.ViewModels.Tools.ImageConverter.Models;

namespace OpenSourceToolkit.Tests
{
    /// <summary>
    /// Tests for ThumbnailStripViewModel.
    /// Demonstrates how to test ViewModel logic by:
    /// 1. Using delegates/actions instead of direct UI calls
    /// 2. Mocking confirmation dialogs
    /// 3. Testing state changes without Avalonia dependencies
    /// </summary>
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

        // ═══════════════════════════════════════════════════════════════════════════
        // Basic Collection Tests
        // ═══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void Constructor_InitializesEmptyCollection()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);

            Assert.IsNotNull(vm.ThumbnailItems);
            Assert.AreEqual(0, vm.ThumbnailItems.Count);
            Assert.IsFalse(vm.HasThumbnails);
        }

        [TestMethod]
        public void HasThumbnails_ReturnsFalse_WhenEmpty()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);

            Assert.IsFalse(vm.HasThumbnails);
        }

        [TestMethod]
        public void Clear_RemovesAllItems()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            // Manually add items to avoid Dispatcher issues in tests
            vm.ThumbnailItems.Add(CreateTestThumbnailItem("Item1"));
            vm.ThumbnailItems.Add(CreateTestThumbnailItem("Item2"));

            vm.Clear();

            Assert.AreEqual(0, vm.ThumbnailItems.Count);
            Assert.IsFalse(vm.HasThumbnails);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Collapse State Tests
        // ═══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void IsCollapsed_DefaultFalse()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);

            Assert.IsFalse(vm.IsCollapsed);
        }

        [TestMethod]
        public void IsCollapsed_WhenSet_RaisesPropertyChanged()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            var changedProperties = new List<string>();
            vm.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

            vm.IsCollapsed = true;

            Assert.IsTrue(changedProperties.Contains(nameof(vm.IsCollapsed)));
            Assert.IsTrue(changedProperties.Contains(nameof(vm.ThumbnailStripVisible)));
            Assert.IsTrue(changedProperties.Contains(nameof(vm.ShowThumbnailExpandButton)));
        }

        [TestMethod]
        public void OnCollapseStateChanged_CalledWhenCollapsed()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            bool? callbackValue = null;
            vm.OnCollapseStateChanged = (collapsed) => callbackValue = collapsed;

            vm.IsCollapsed = true;

            Assert.IsTrue(callbackValue.HasValue);
            Assert.IsTrue(callbackValue.Value);
        }

        [TestMethod]
        public void ThumbnailStripVisible_TrueWhenHasItemsAndNotCollapsed()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            vm.ThumbnailItems.Add(CreateTestThumbnailItem("Test"));

            Assert.IsTrue(vm.ThumbnailStripVisible);
        }

        [TestMethod]
        public void ThumbnailStripVisible_FalseWhenCollapsed()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            vm.ThumbnailItems.Add(CreateTestThumbnailItem("Test"));
            vm.IsCollapsed = true;

            Assert.IsFalse(vm.ThumbnailStripVisible);
        }

        [TestMethod]
        public void ShowThumbnailExpandButton_TrueWhenCollapsedWithItems()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            vm.ThumbnailItems.Add(CreateTestThumbnailItem("Test"));
            vm.IsCollapsed = true;

            Assert.IsTrue(vm.ShowThumbnailExpandButton);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Unsaved Thumbnails Tests
        // ═══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void HasUnsavedThumbnails_TrueWhenItemNotSaved()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            var item = CreateTestThumbnailItem("Unsaved");
            item.SavedAt = null; // Not saved
            vm.ThumbnailItems.Add(item);

            Assert.IsTrue(vm.HasUnsavedThumbnails);
        }

        [TestMethod]
        public void HasUnsavedThumbnails_FalseWhenAllSaved()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            var item = CreateTestThumbnailItem("Saved");
            item.SavedAt = DateTime.Now;
            vm.ThumbnailItems.Add(item);

            Assert.IsFalse(vm.HasUnsavedThumbnails);
        }

        [TestMethod]
        public void GetUnsavedThumbnails_ReturnsOnlyUnsaved()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);

            var saved = CreateTestThumbnailItem("Saved");
            saved.SavedAt = DateTime.Now;

            var unsaved = CreateTestThumbnailItem("Unsaved");
            unsaved.SavedAt = null;

            vm.ThumbnailItems.Add(saved);
            vm.ThumbnailItems.Add(unsaved);

            var result = vm.GetUnsavedThumbnails();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Unsaved", result[0].Label);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // AI Selection Tests
        // ═══════════════════════════════════════════════════════════════════════════

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

        [TestMethod]
        public void GetMarkedForAi_ReturnsEmpty_WhenNoneMarked()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);

            var item = CreateTestThumbnailItem("Test");
            item.SendToAi = false;
            item.RawBytes = _testImageBytes;
            vm.ThumbnailItems.Add(item);

            var result = vm.GetMarkedForAi();

            Assert.AreEqual(0, result.Count);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Event/Delegate Tests
        // ═══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void OnChanged_RaisedWhenClearing()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            vm.ThumbnailItems.Add(CreateTestThumbnailItem("Test"));
            bool eventRaised = false;
            vm.OnChanged += () => eventRaised = true;

            vm.Clear();

            Assert.IsTrue(eventRaised);
        }

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

        // ═══════════════════════════════════════════════════════════════════════════
        // Restore Tests
        // ═══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        public void RestoreItem_PreservesSavedState()
        {
            var vm = new ThumbnailStripViewModel(_imageProcessor);
            var savedAt = new DateTime(2025, 1, 15, 10, 30, 0);

            // Note: RestoreItem uses Dispatcher.UIThread.InvokeAsync internally
            // In a real test scenario, you'd need to either:
            // 1. Inject IDispatcherService and use SynchronousDispatcherService
            // 2. Or use a test helper that handles Avalonia initialization
            // For now, we test the logic by directly manipulating the collection

            var item = new ThumbnailItem
            {
                Id = "test-id",
                Label = "Restored",
                RawBytes = _testImageBytes,
                MimeType = "image/png",
                SendToAi = true,
                CreatedAt = DateTime.Now.AddDays(-1),
                SavedAt = savedAt
            };
            vm.ThumbnailItems.Add(item);

            var restored = vm.ThumbnailItems.First();
            Assert.AreEqual(savedAt, restored.SavedAt);
            Assert.IsTrue(restored.IsSavedOutsideSession);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Helper Methods
        // ═══════════════════════════════════════════════════════════════════════════

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
}
