using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flowery.Controls;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.NET.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    /// <summary>
    /// Represents a DaisyUI theme with preview colors for the visual picker.
    /// Uses Flowery.NET's DaisyThemeManager for theme data.
    /// </summary>
    public class DaisyThemePreview : ObservableObject
    {
        public string Name { get; set; }
        public bool IsDark { get; set; }

        // Preview colors (from Flowery's theme palettes)
        public IBrush BackgroundBrush { get; set; }
        public IBrush PrimaryBrush { get; set; }
        public IBrush SecondaryBrush { get; set; }
        public IBrush AccentBrush { get; set; }
        public IBrush NeutralBrush { get; set; }
        public IBrush TextBrush { get; set; }

        // Content colors for swatch text
        public IBrush PrimaryContentBrush { get; set; }
        public IBrush SecondaryContentBrush { get; set; }
        public IBrush AccentContentBrush { get; set; }
        public IBrush NeutralContentBrush { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    /// <summary>
    /// Theme selector tool using Flowery.NET's DaisyThemeManager.
    /// Supports all 35 built-in DaisyUI themes.
    /// </summary>
    public partial class ThemeSelectionToolViewModel : ToolViewModel
    {
        public override int Id => 9999;
        public override string Name => ToolkitLocalization.GetString("Tool_ThemeSelection_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_ThemeSelection_Description");
        public override string IconKey => "PaletteIcon";

        private string _currentThemeName = "Dark";
        public string CurrentThemeName
        {
            get => _currentThemeName;
            set => SetProperty(ref _currentThemeName, value);
        }

        // Visual theme picker
        public ObservableCollection<DaisyThemePreview> DaisyThemePreviews { get; } = new ObservableCollection<DaisyThemePreview>();

        private DaisyThemePreview _selectedThemePreview;
        public DaisyThemePreview SelectedThemePreview
        {
            get => _selectedThemePreview;
            set
            {
                // Deselect previous
                if (_selectedThemePreview != null)
                    _selectedThemePreview.IsSelected = false;

                if (SetProperty(ref _selectedThemePreview, value) && value != null)
                {
                    value.IsSelected = true;
                    ApplyDaisyTheme(value);
                }
            }
        }

        public ICommand SelectThemeCommand { get; }

        public ThemeSelectionToolViewModel()
        {
            SelectThemeCommand = new RelayCommand<DaisyThemePreview>(SelectTheme);

            LoadDaisyThemePreviews();
            RestoreSavedDaisyTheme();
        }

        private void LoadDaisyThemePreviews()
        {
            try
            {
                // Use Flowery's DaisyThemeManager for available themes
                foreach (var themeInfo in DaisyThemeManager.AvailableThemes)
                {
                    // Load preview colors from the theme's palette
                    var preview = new DaisyThemePreview
                    {
                        Name = themeInfo.Name,
                        IsDark = themeInfo.IsDark,
                        BackgroundBrush = GetBrushFromPalette(themeInfo.Name, "DaisyBase100Brush", themeInfo.IsDark ? "#1f2937" : "#ffffff"),
                        PrimaryBrush = GetBrushFromPalette(themeInfo.Name, "DaisyPrimaryBrush", "#570df8"),
                        SecondaryBrush = GetBrushFromPalette(themeInfo.Name, "DaisySecondaryBrush", "#f000b8"),
                        AccentBrush = GetBrushFromPalette(themeInfo.Name, "DaisyAccentBrush", "#37cdbe"),
                        NeutralBrush = GetBrushFromPalette(themeInfo.Name, "DaisyNeutralBrush", "#3d4451"),
                        TextBrush = GetBrushFromPalette(themeInfo.Name, "DaisyBaseContentBrush", themeInfo.IsDark ? "#ffffff" : "#1f2937"),
                        PrimaryContentBrush = GetBrushFromPalette(themeInfo.Name, "DaisyPrimaryContentBrush", "#ffffff"),
                        SecondaryContentBrush = GetBrushFromPalette(themeInfo.Name, "DaisySecondaryContentBrush", "#ffffff"),
                        AccentContentBrush = GetBrushFromPalette(themeInfo.Name, "DaisyAccentContentBrush", "#ffffff"),
                        NeutralContentBrush = GetBrushFromPalette(themeInfo.Name, "DaisyNeutralContentBrush", "#ffffff"),
                    };

                    DaisyThemePreviews.Add(preview);
                }
            }
            catch
            {
                // Ignore errors loading theme previews
            }
        }

        private IBrush GetBrushFromPalette(string themeName, string resourceKey, string fallback)
        {
            try
            {
                var paletteUri = new Uri($"avares://Flowery.NET/Themes/Palettes/Daisy{themeName}.axaml");
                var palette = (Avalonia.Controls.ResourceDictionary)Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(paletteUri);
                
                if (palette.TryGetResource(resourceKey, null, out var resource) && resource is IBrush brush)
                    return brush;
            }
            catch
            {
                // Ignore - use fallback
            }
            
            return new SolidColorBrush(Color.Parse(fallback));
        }

        private void SelectTheme(DaisyThemePreview preview)
        {
            if (preview != null)
            {
                SelectedThemePreview = preview;
            }
        }

        /// <summary>
        /// Apply a DaisyUI theme using in-place resource updates.
        /// </summary>
        private void ApplyDaisyTheme(DaisyThemePreview preview)
        {
            try
            {
                // Use in-place resource updates for proper DynamicResource refreshing
                // ApplyThemeInPlace handles saving to settings automatically
                if (App.ApplyThemeInPlace(preview.Name))
                {
                    CurrentThemeName = preview.Name;
                }
            }
            catch
            {
                // Ignore errors applying theme
            }
        }

        /// <summary>
        /// Restore the saved DaisyUI theme selection on load.
        /// </summary>
        private void RestoreSavedDaisyTheme()
        {
            var savedTheme = AppSettings.Current.DaisyUiTheme;
            
            // Default to "Dark" if no theme is saved
            if (string.IsNullOrEmpty(savedTheme))
                savedTheme = "Dark";

            // Find and select the saved theme visually
            foreach (var preview in DaisyThemePreviews)
            {
                if (string.Equals(preview.Name, savedTheme, StringComparison.OrdinalIgnoreCase))
                {
                    preview.IsSelected = true;
                    _selectedThemePreview = preview;
                    CurrentThemeName = preview.Name;
                    OnPropertyChanged(nameof(SelectedThemePreview));
                    break;
                }
            }
        }
    }
}
