using CommunityToolkit.Mvvm.Input;
using Flowery.Extensions;
using OpenSourceToolkit.Documents;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class PdfToolViewModel : ToolViewModel
    {
        public override int Id => 20;
        public override string Name => ToolkitLocalization.GetString("Tool_Pdf_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Pdf_Description");
        // Document icon
        public override string IconKey => "PdfIcon";

        // --- Notification callback (wired by View) ---
        /// <summary>
        /// Action to show a toast notification. Parameters: message, isError.
        /// </summary>
        public Action<string, bool> ShowNotificationAction { get; set; }

        private void ShowNotification(string message, bool isError = false)
        {
            ShowNotificationAction?.Invoke(message, isError);
        }

        // --- Merge ---
        public ObservableCollection<string> MergeInputFiles { get; } = new ObservableCollection<string>();

        private string _mergeOutputFile;
        public string MergeOutputFile
        {
            get => _mergeOutputFile;
            set => SetProperty(ref _mergeOutputFile, value);
        }

        public ICommand MergeCommand { get; }
        public ICommand ClearMergeInputsCommand { get; }

        // --- Split ---
        private string _splitInputFile;
        public string SplitInputFile
        {
            get => _splitInputFile;
            set => SetProperty(ref _splitInputFile, value);
        }

        private string _splitOutputDir;
        public string SplitOutputDir
        {
            get => _splitOutputDir;
            set => SetProperty(ref _splitOutputDir, value);
        }

        public ICommand SplitCommand { get; }

        // --- Watermark ---
        private string _watermarkInputFile;
        public string WatermarkInputFile
        {
            get => _watermarkInputFile;
            set => SetProperty(ref _watermarkInputFile, value);
        }

        private string _watermarkOutputFile;
        public string WatermarkOutputFile
        {
            get => _watermarkOutputFile;
            set => SetProperty(ref _watermarkOutputFile, value);
        }

        private string _watermarkText = "DRAFT";
        public string WatermarkText
        {
            get => _watermarkText;
            set { if (SetProperty(ref _watermarkText, value)) OnWatermarkSettingChanged(); }
        }

        private double _watermarkFontSize = 48;
        public double WatermarkFontSize
        {
            get => _watermarkFontSize;
            set { if (SetProperty(ref _watermarkFontSize, value)) OnWatermarkSettingChanged(); }
        }

        private double _watermarkRotation = -45;
        public double WatermarkRotation
        {
            get => _watermarkRotation;
            set { if (SetProperty(ref _watermarkRotation, value)) OnWatermarkSettingChanged(); }
        }

        private int _watermarkOpacity = 30;
        public int WatermarkOpacity
        {
            get => _watermarkOpacity;
            set { if (SetProperty(ref _watermarkOpacity, value)) OnWatermarkSettingChanged(); }
        }

        private PdfWatermarkPosition _watermarkPosition = PdfWatermarkPosition.MiddleCenter;
        public PdfWatermarkPosition WatermarkPosition
        {
            get => _watermarkPosition;
            set { if (SetProperty(ref _watermarkPosition, value)) OnWatermarkSettingChanged(); }
        }

        private decimal _watermarkColor = 16711680m; // #FF0000
        public decimal WatermarkColor
        {
            get => _watermarkColor;
            set { if (SetProperty(ref _watermarkColor, value)) OnWatermarkSettingChanged(); }
        }

        private int _watermarkPadding = 20;
        public int WatermarkPadding
        {
            get => _watermarkPadding;
            set { if (SetProperty(ref _watermarkPadding, value)) OnWatermarkSettingChanged(); }
        }

        private bool _watermarkBold = false;
        public bool WatermarkBold
        {
            get => _watermarkBold;
            set { if (SetProperty(ref _watermarkBold, value)) OnWatermarkSettingChanged(); }
        }

        private bool _watermarkItalic = false;
        public bool WatermarkItalic
        {
            get => _watermarkItalic;
            set { if (SetProperty(ref _watermarkItalic, value)) OnWatermarkSettingChanged(); }
        }

        // Position options for dropdown (display name -> enum value)
        public List<WatermarkPositionItem> WatermarkPositionOptions { get; } = new List<WatermarkPositionItem>
        {
            new WatermarkPositionItem("Top Left", PdfWatermarkPosition.TopLeft),
            new WatermarkPositionItem("Top Center", PdfWatermarkPosition.TopCenter),
            new WatermarkPositionItem("Top Right", PdfWatermarkPosition.TopRight),
            new WatermarkPositionItem("Middle Left", PdfWatermarkPosition.MiddleLeft),
            new WatermarkPositionItem("Center", PdfWatermarkPosition.MiddleCenter),
            new WatermarkPositionItem("Middle Right", PdfWatermarkPosition.MiddleRight),
            new WatermarkPositionItem("Bottom Left", PdfWatermarkPosition.BottomLeft),
            new WatermarkPositionItem("Bottom Center", PdfWatermarkPosition.BottomCenter),
            new WatermarkPositionItem("Bottom Right", PdfWatermarkPosition.BottomRight),
        };

        private WatermarkPositionItem _selectedWatermarkPosition;
        public WatermarkPositionItem SelectedWatermarkPosition
        {
            get => _selectedWatermarkPosition;
            set
            {
                if (SetProperty(ref _selectedWatermarkPosition, value) && value != null)
                {
                    WatermarkPosition = value.Value;
                }
            }
        }

        public ICommand WatermarkCommand { get; }

        // --- Watermark Presets ---
        public ObservableCollection<WatermarkPreset> WatermarkPresets { get; } = new ObservableCollection<WatermarkPreset>();

        private WatermarkPreset _selectedPreset;
        public WatermarkPreset SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (SetProperty(ref _selectedPreset, value) && value != null)
                {
                    LoadPresetValues(value);
                }
            }
        }

        private string _newPresetName = "";
        public string NewPresetName
        {
            get => _newPresetName;
            set => SetProperty(ref _newPresetName, value);
        }

        public ICommand SavePresetCommand { get; }
        public ICommand DeletePresetCommand { get; }

        // Action for prompting preset name (wired by View)
        public Func<string, string> PromptPresetNameAction { get; set; }


        public PdfToolViewModel()
        {
            MergeCommand = new RelayCommand(async () => await MergePdfs());
            ClearMergeInputsCommand = new RelayCommand(() => MergeInputFiles.Clear());
            SplitCommand = new RelayCommand(async () => await SplitPdf());
            WatermarkCommand = new RelayCommand(async () => await AddWatermark());
            SavePresetCommand = new RelayCommand(SaveCurrentAsPreset);
            DeletePresetCommand = new RelayCommand(DeleteSelectedPreset, () => SelectedPreset != null);

            // Initialize selected position to match default
            SelectedWatermarkPosition = WatermarkPositionOptions.Find(p => p.Value == WatermarkPosition);

            // Load saved settings and presets
            LoadSavedSettings();
            LoadPresets();
        }

        private void LoadSavedSettings()
        {
            // Load last-used watermark settings
            var saved = GetSetting<WatermarkSettingsData>("WatermarkSettings");
            if (saved != null)
            {
                _watermarkText = saved.Text ?? "DRAFT";
                _watermarkFontSize = saved.FontSize;
                _watermarkRotation = saved.Rotation;
                _watermarkOpacity = saved.Opacity;
                _watermarkPosition = saved.Position;
                _watermarkColor = saved.Color;
                _watermarkPadding = saved.Padding;
                _watermarkBold = saved.IsBold;
                _watermarkItalic = saved.IsItalic;

                // Update selected position item
                SelectedWatermarkPosition = WatermarkPositionOptions.Find(p => p.Value == _watermarkPosition);
            }
        }

        private void SaveCurrentSettings()
        {
            var data = new WatermarkSettingsData
            {
                Text = WatermarkText,
                FontSize = WatermarkFontSize,
                Rotation = WatermarkRotation,
                Opacity = WatermarkOpacity,
                Position = WatermarkPosition,
                Color = WatermarkColor,
                Padding = WatermarkPadding,
                IsBold = WatermarkBold,
                IsItalic = WatermarkItalic
            };
            SetSetting("WatermarkSettings", data);
        }

        private void LoadPresets()
        {
            var presets = GetSetting<List<WatermarkPreset>>("WatermarkPresets");
            if (presets != null)
            {
                foreach (var preset in presets)
                {
                    WatermarkPresets.Add(preset);
                }
            }
        }

        private void SavePresets()
        {
            SetSetting("WatermarkPresets", WatermarkPresets.ToList());
        }

        private void SaveCurrentAsPreset()
        {
            // Get preset name from user via dialog
            var name = PromptPresetNameAction?.Invoke(NewPresetName);
            if (!string.IsNullOrWhiteSpace(name))
            {
                SavePresetWithName(name);
            }
        }

        /// <summary>
        /// Saves the current watermark settings as a preset with the given name.
        /// Called by the View after showing the name dialog.
        /// </summary>
        public void SavePresetWithName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowNotification("Please enter a watermark preset name.", true);
                return;
            }

            // Check for duplicate name
            if (WatermarkPresets.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                ShowNotification($"A watermark preset named '{name}' already exists.", true);
                return;
            }

            var preset = new WatermarkPreset
            {
                Name = name,
                Text = WatermarkText,
                FontSize = WatermarkFontSize,
                Rotation = WatermarkRotation,
                Opacity = WatermarkOpacity,
                Position = WatermarkPosition,
                Color = WatermarkColor,
                Padding = WatermarkPadding,
                IsBold = WatermarkBold,
                IsItalic = WatermarkItalic
            };

            WatermarkPresets.Add(preset);
            SavePresets();
            NewPresetName = "";
            ShowNotification($"Watermark preset '{name}' saved.");
        }

        private void DeleteSelectedPreset()
        {
            if (SelectedPreset == null) return;

            var name = SelectedPreset.Name;
            WatermarkPresets.Remove(SelectedPreset);
            SelectedPreset = null;
            SavePresets();
            ShowNotification($"Watermark preset '{name}' deleted.");
        }

        private void LoadPresetValues(WatermarkPreset preset)
        {
            WatermarkText = preset.Text;
            WatermarkFontSize = preset.FontSize;
            WatermarkRotation = preset.Rotation;
            WatermarkOpacity = preset.Opacity;
            WatermarkPosition = preset.Position;
            WatermarkColor = preset.Color;
            WatermarkPadding = preset.Padding;
            WatermarkBold = preset.IsBold;
            WatermarkItalic = preset.IsItalic;

            // Update dropdown selection
            SelectedWatermarkPosition = WatermarkPositionOptions.Find(p => p.Value == preset.Position);
        }

        // Override property setters to auto-save settings
        private void OnWatermarkSettingChanged()
        {
            SaveCurrentSettings();
        }

        private async Task MergePdfs()
        {
            if (MergeInputFiles.Count < 2)
            {
                ShowNotification("Please select at least 2 PDF files to merge.", true);
                return;
            }
            if (string.IsNullOrWhiteSpace(MergeOutputFile))
            {
                ShowNotification("Please select an output file for the merge.", true);
                return;
            }

            try
            {
                await Task.Run(() => PdfToolkit.MergePdfs(MergeInputFiles, MergeOutputFile));
                ShowNotification("Merge completed successfully!");
            }
            catch (Exception ex)
            {
                ShowNotification($"Error merging PDFs: {ex.Message}", true);
            }
        }

        private async Task SplitPdf()
        {
            if (string.IsNullOrWhiteSpace(SplitInputFile))
            {
                ShowNotification("Please select a PDF file to split.", true);
                return;
            }
            if (string.IsNullOrWhiteSpace(SplitOutputDir))
            {
                ShowNotification("Please select an output directory.", true);
                return;
            }

            try
            {
                await Task.Run(() => PdfToolkit.SplitPdf(SplitInputFile, SplitOutputDir));
                ShowNotification("Split completed successfully!");
            }
            catch (Exception ex)
            {
                ShowNotification($"Error splitting PDF: {ex.Message}", true);
            }
        }

        private async Task AddWatermark()
        {
             if (string.IsNullOrWhiteSpace(WatermarkInputFile))
            {
                ShowNotification("Please select a PDF file to watermark.", true);
                return;
            }
            if (string.IsNullOrWhiteSpace(WatermarkOutputFile))
            {
                ShowNotification("Please select an output file for the result.", true);
                return;
            }
             if (string.IsNullOrWhiteSpace(WatermarkText))
            {
                ShowNotification("Please enter watermark text.", true);
                return;
            }

            try
            {
                var options = new PdfWatermarkOptions
                {
                    Text = WatermarkText,
                    FontSize = WatermarkFontSize,
                    Rotation = WatermarkRotation,
                    Opacity = WatermarkOpacity,
                    Position = WatermarkPosition,
                    Color = WatermarkColor.ToColorHexString(),
                    Padding = WatermarkPadding,
                    IsBold = WatermarkBold,
                    IsItalic = WatermarkItalic
                };

                await Task.Run(() => PdfToolkit.AddWatermark(WatermarkInputFile, WatermarkOutputFile, options));
                ShowNotification("Watermark added successfully!");
            }
            catch (Exception ex)
            {
                ShowNotification($"Error adding watermark: {ex.Message}", true);
            }
        }
    }

    /// <summary>
    /// Helper class for position dropdown binding.
    /// </summary>
    public class WatermarkPositionItem
    {
        public string DisplayName { get; }
        public PdfWatermarkPosition Value { get; }

        public WatermarkPositionItem(string displayName, PdfWatermarkPosition value)
        {
            DisplayName = displayName;
            Value = value;
        }

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// A saved watermark configuration preset.
    /// </summary>
    public class WatermarkPreset
    {
        public string Name { get; set; }
        public string Text { get; set; } = "DRAFT";
        public double FontSize { get; set; } = 48;
        public double Rotation { get; set; } = -45;
        public int Opacity { get; set; } = 30;
        public PdfWatermarkPosition Position { get; set; } = PdfWatermarkPosition.MiddleCenter;
        public decimal Color { get; set; } = 16711680m; // #FF0000
        public int Padding { get; set; } = 20;
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }

        public override string ToString() => Name;
    }

    /// <summary>
    /// Internal class for persisting current watermark settings.
    /// </summary>
    public class WatermarkSettingsData
    {
        public string Text { get; set; }
        public double FontSize { get; set; }
        public double Rotation { get; set; }
        public int Opacity { get; set; }
        public PdfWatermarkPosition Position { get; set; }
        public decimal Color { get; set; }
        public int Padding { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
    }
}
