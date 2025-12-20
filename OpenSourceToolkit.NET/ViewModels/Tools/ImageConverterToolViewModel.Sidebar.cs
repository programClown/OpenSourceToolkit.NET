using System;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Services;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class ImageConverterToolViewModel
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // Tool Category Selection (Left Toolbar)
        // ═══════════════════════════════════════════════════════════════════════════

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        // Default to pan mode (no category selected = -1)
        private ImageToolCategory _selectedToolCategory = (ImageToolCategory)(-1);
        public ImageToolCategory SelectedToolCategory
        {
            get => _selectedToolCategory;
            set
            {
                if (SetProperty(ref _selectedToolCategory, value))
                    NotifyAllCategoryProperties();
            }
        }

        public bool IsOutputCategorySelected => SelectedToolCategory == ImageToolCategory.Output;
        public bool IsTransformCategorySelected => SelectedToolCategory == ImageToolCategory.Transform;
        public bool IsResizeCategorySelected => SelectedToolCategory == ImageToolCategory.Resize;
        public bool IsAdjustCategorySelected => SelectedToolCategory == ImageToolCategory.Adjust;
        public bool IsFiltersCategorySelected => SelectedToolCategory == ImageToolCategory.Filters;
        public bool IsBlurSharpenCategorySelected => SelectedToolCategory == ImageToolCategory.BlurSharpen;
        public bool IsEffectsCategorySelected => SelectedToolCategory == ImageToolCategory.Effects;
        public bool IsCropCategorySelected => SelectedToolCategory == ImageToolCategory.Crop;
        public bool IsWatermarkCategorySelected => SelectedToolCategory == ImageToolCategory.Watermark;
        public bool IsBackgroundCategorySelected => SelectedToolCategory == ImageToolCategory.Background;
        public bool IsMetadataCategorySelected => SelectedToolCategory == ImageToolCategory.Metadata;
        public bool IsAiCategorySelected => SelectedToolCategory == ImageToolCategory.AI;
        public bool IsAnyCategorySelected => (int)_selectedToolCategory >= 0;
        // Pan mode and AI can coexist - this excludes AI from "tool" categories
        public bool IsAnyToolCategorySelected => (int)_selectedToolCategory >= 0 && _selectedToolCategory != ImageToolCategory.AI;
        // Horizontal toolbar visible for non-AI tool categories
        public bool IsHorizontalToolbarVisible => IsAnyToolCategorySelected;

        private GridLength _sidebarWidth = new GridLength(0);
        public GridLength SidebarWidth
        {
            get => _sidebarWidth;
            set
            {
                if (SetProperty(ref _sidebarWidth, value))
                {
                    // When user drags the splitter, save the proportional width
                    if (value.IsAbsolute && value.Value > 10 && _availableWidth > 0)
                    {
                        _sidebarWidthPercent = value.Value / _availableWidth;
                        SaveSidebarWidthPercent();
                    }
                }
            }
        }

        private double _sidebarMinWidth = 0;
        public double SidebarMinWidth
        {
            get => _sidebarMinWidth;
            set => SetProperty(ref _sidebarMinWidth, value);
        }

        // Proportional sidebar width (0.0-1.0), loaded from settings
        private double _sidebarWidthPercent;
        private double _availableWidth;

        private const double SidebarMinWidthConstant = 250;

        /// <summary>
        /// Called by the View when the available width changes (e.g., window resize).
        /// Updates the sidebar width proportionally (only for AI category).
        /// </summary>
        public void UpdateAvailableWidth(double availableWidth)
        {
            if (availableWidth <= 0) return;

            _availableWidth = availableWidth;

            // Sidebar only visible for AI category, update its width proportionally
            if (IsAiCategorySelected && _sidebarWidthPercent > 0)
            {
                // Enforce minimum 250px, max 60% of available
                var newWidth = Math.Max(SidebarMinWidthConstant, availableWidth * _sidebarWidthPercent);
                newWidth = Math.Min(newWidth, availableWidth * 0.6);
                _sidebarWidth = new GridLength(newWidth);
                OnPropertyChanged(nameof(SidebarWidth));
            }
        }

        private void SaveSidebarWidthPercent()
        {
            // Clamp to reasonable range (10% to 60%)
            _sidebarWidthPercent = Math.Max(0.1, Math.Min(0.6, _sidebarWidthPercent));

            if (AppSettings.Current.ImageEditorSessions != null)
            {
                AppSettings.Current.ImageEditorSessions.SidebarWidthPercent = _sidebarWidthPercent;
                AppSettings.Save();
            }
        }

        public ICommand SelectToolCategoryCommand { get; private set; }
        public ICommand SelectPanModeCommand { get; private set; }

        private void InitializeSidebarCommands()
        {
            SelectToolCategoryCommand = new RelayCommand<ImageToolCategory>(SelectToolCategory);
            SelectPanModeCommand = new RelayCommand(SelectPanMode);
        }

        private void SelectToolCategory(ImageToolCategory category)
        {
            if (SelectedToolCategory == category)
            {
                SelectPanMode();
            }
            else
            {
                var previousCategory = SelectedToolCategory;
                SelectedToolCategory = category;

                // Auto-enable/disable feature toggles based on category selection
                if (category == ImageToolCategory.Resize)
                    Workspace.ResizeEnabled = true;
                else if (previousCategory == ImageToolCategory.Resize)
                    Workspace.ResizeEnabled = false;

                if (category == ImageToolCategory.Crop)
                    Workspace.CropEnabled = true;
                else if (previousCategory == ImageToolCategory.Crop)
                    Workspace.CropEnabled = false;

                if (category == ImageToolCategory.Watermark)
                    Workspace.WatermarkEnabled = true;
                else if (previousCategory == ImageToolCategory.Watermark)
                    Workspace.WatermarkEnabled = false;
            }
        }

        private void SelectPanMode()
        {
            var previousCategory = _selectedToolCategory;
            _selectedToolCategory = (ImageToolCategory)(-1);

            // Auto-disable feature toggles when leaving their category
            if (previousCategory == ImageToolCategory.Resize)
                Workspace.ResizeEnabled = false;
            if (previousCategory == ImageToolCategory.Crop)
                Workspace.CropEnabled = false;
            if (previousCategory == ImageToolCategory.Watermark)
                Workspace.WatermarkEnabled = false;

            OnPropertyChanged(nameof(SelectedToolCategory));
            NotifyAllCategoryProperties();
        }

        /// <summary>
        /// Re-applies category-controlled flags after ResetAdjustments clears them.
        /// Called when undo or other operations reset the workspace state.
        /// </summary>
        private void ReapplyCategoryFlags()
        {
            if (SelectedToolCategory == ImageToolCategory.Resize)
                Workspace.ResizeEnabled = true;
            if (SelectedToolCategory == ImageToolCategory.Crop)
                Workspace.CropEnabled = true;
            if (SelectedToolCategory == ImageToolCategory.Watermark)
                Workspace.WatermarkEnabled = true;
        }

        private void NotifyAllCategoryProperties()
        {
            OnPropertyChanged(nameof(IsAnyCategorySelected));
            OnPropertyChanged(nameof(IsAnyToolCategorySelected));
            OnPropertyChanged(nameof(IsHorizontalToolbarVisible));
            OnPropertyChanged(nameof(IsOutputCategorySelected));
            OnPropertyChanged(nameof(IsTransformCategorySelected));
            OnPropertyChanged(nameof(IsResizeCategorySelected));
            OnPropertyChanged(nameof(IsAdjustCategorySelected));
            OnPropertyChanged(nameof(IsFiltersCategorySelected));
            OnPropertyChanged(nameof(IsBlurSharpenCategorySelected));
            OnPropertyChanged(nameof(IsEffectsCategorySelected));
            OnPropertyChanged(nameof(IsCropCategorySelected));
            OnPropertyChanged(nameof(IsWatermarkCategorySelected));
            OnPropertyChanged(nameof(IsBackgroundCategorySelected));
            OnPropertyChanged(nameof(IsMetadataCategorySelected));
            OnPropertyChanged(nameof(IsAiCategorySelected));

            UpdateSidebarLayout();
        }

        private void UpdateSidebarLayout()
        {
            // Sidebar only shows for AI category (non-AI categories use horizontal toolbar)
            if (IsAiCategorySelected)
            {
                // If opening (was previously closed/0), restore proportional width
                if (SidebarWidth.Value == 0)
                {
                    if (_availableWidth > 0 && _sidebarWidthPercent > 0)
                    {
                        // Enforce minimum 250px, max 60% of available
                        var newWidth = Math.Max(SidebarMinWidthConstant, _availableWidth * _sidebarWidthPercent);
                        newWidth = Math.Min(newWidth, _availableWidth * 0.6);
                        _sidebarWidth = new GridLength(newWidth);
                        OnPropertyChanged(nameof(SidebarWidth));
                    }
                    else
                    {
                        // Fallback: use default 280px (above minimum)
                        _sidebarWidth = new GridLength(280);
                        OnPropertyChanged(nameof(SidebarWidth));
                    }
                }
                SidebarMinWidth = SidebarMinWidthConstant;
            }
            else
            {
                // Closing - proportional width is already saved in SidebarWidth setter
                _sidebarWidth = new GridLength(0);
                OnPropertyChanged(nameof(SidebarWidth));
                SidebarMinWidth = 0;
            }
        }

        private void LoadSidebarWidthPercent()
        {
            _sidebarWidthPercent = AppSettings.Current.ImageEditorSessions?.SidebarWidthPercent ?? 0.35;
        }
    }
}
