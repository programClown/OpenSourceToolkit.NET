using Avalonia.Media;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Colors;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class PaletteItem
    {
        public string Name { get; }
        public string Hex { get; }
        public SolidColorBrush Brush { get; }

        public PaletteItem(string name, string hex, SolidColorBrush brush)
        {
            Name = name;
            Hex = hex;
            Brush = brush;
        }
    }

    public partial class ColorToolViewModel : ToolViewModel
    {
        public override int Id => 12;
        public override string Name => ToolkitLocalization.GetString("Tool_Color_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Color_Description");
        public override string IconKey => "ColorIcon";

        // --- Commands ---
        public IRelayCommand<string> UpdateFromHexInputCommand { get; }
        public IRelayCommand RandomizeColorCommand { get; }
        public IRelayCommand UpdatePaletteCommand { get; }
        public IRelayCommand CopyHexCommand { get; }
        public IRelayCommand CopyRgbCommand { get; }
        public IRelayCommand CopyHslCommand { get; }

        // --- Clipboard Action (set by View) ---
        public Action<string> CopyToClipboardAction { get; set; }

        // --- Formatted Strings for Display ---
        public string RgbFormatted => $"{R}, {G}, {B}";
        public string HslFormatted => $"{HslH}°, {HslS}%, {HslL}%";

        // --- Current Color State ---

        public Color CurrentColor
        {
            get => Color.FromRgb(R, G, B);
            set
            {
                if (R != value.R || G != value.G || B != value.B)
                {
                    UpdateFromRgb(value.R, value.G, value.B);
                    OnPropertyChanged(nameof(CurrentColor));
                }
            }
        }

        private byte _r;
        public byte R
        {
            get => _r;
            set { if (SetProperty(ref _r, value)) UpdateFromRgb(R, G, B); }
        }

        private byte _g;
        public byte G
        {
            get => _g;
            set { if (SetProperty(ref _g, value)) UpdateFromRgb(R, G, B); }
        }

        private byte _b;
        public byte B
        {
            get => _b;
            set { if (SetProperty(ref _b, value)) UpdateFromRgb(R, G, B); }
        }

        private string _hex;
        public string Hex
        {
            get => _hex;
            set => SetProperty(ref _hex, value);
        }

        private double _hslH;
        public double HslH
        {
            get => _hslH;
            set => SetProperty(ref _hslH, value);
        }

        private double _hslS;
        public double HslS
        {
            get => _hslS;
            set => SetProperty(ref _hslS, value);
        }

        private double _hslL;
        public double HslL
        {
            get => _hslL;
            set => SetProperty(ref _hslL, value);
        }

        private double _hsvH;
        public double HsvH
        {
            get => _hsvH;
            set => SetProperty(ref _hsvH, value);
        }

        private double _hsvS;
        public double HsvS
        {
            get => _hsvS;
            set => SetProperty(ref _hsvS, value);
        }

        private double _hsvV;
        public double HsvV
        {
            get => _hsvV;
            set => SetProperty(ref _hsvV, value);
        }

        private double _labL;
        public double LabL
        {
            get => _labL;
            set => SetProperty(ref _labL, value);
        }

        private double _labA;
        public double LabA
        {
            get => _labA;
            set => SetProperty(ref _labA, value);
        }

        private double _labB;
        public double LabB
        {
            get => _labB;
            set => SetProperty(ref _labB, value);
        }

        private int _cmykC;
        public int CmykC
        {
            get => _cmykC;
            set => SetProperty(ref _cmykC, value);
        }

        private int _cmykM;
        public int CmykM
        {
            get => _cmykM;
            set => SetProperty(ref _cmykM, value);
        }

        private int _cmykY;
        public int CmykY
        {
            get => _cmykY;
            set => SetProperty(ref _cmykY, value);
        }

        private int _cmykK;
        public int CmykK
        {
            get => _cmykK;
            set => SetProperty(ref _cmykK, value);
        }

        private SolidColorBrush _previewBrush;
        public SolidColorBrush PreviewBrush
        {
            get => _previewBrush;
            set => SetProperty(ref _previewBrush, value);
        }

        // --- Palette State ---
        private string _selectedHarmonyType = "Complementary";
        public string SelectedHarmonyType
        {
            get => _selectedHarmonyType;
            set => SetProperty(ref _selectedHarmonyType, value);
        }

        public ObservableCollection<string> HarmonyTypes { get; } = new ObservableCollection<string>
        {
            "Complementary", "Triadic", "Analogous", "Split-Complementary", "Tetradic"
        };
        public ObservableCollection<PaletteItem> Palette { get; } = new ObservableCollection<PaletteItem>();

        // --- Gradient State ---
        private Color _gradientStartColor;
        public Color GradientStartColor
        {
            get => _gradientStartColor;
            set { if (SetProperty(ref _gradientStartColor, value)) UpdateGradient(); }
        }

        private Color _gradientEndColor;
        public Color GradientEndColor
        {
            get => _gradientEndColor;
            set { if (SetProperty(ref _gradientEndColor, value)) UpdateGradient(); }
        }

        private LinearGradientBrush _gradientPreviewBrush;
        public LinearGradientBrush GradientPreviewBrush
        {
            get => _gradientPreviewBrush;
            set => SetProperty(ref _gradientPreviewBrush, value);
        }

        private string _gradientCss;
        public string GradientCss
        {
            get => _gradientCss;
            set => SetProperty(ref _gradientCss, value);
        }

        // --- Accessibility State ---
        private Color _accessibilityLeftColor;
        public Color AccessibilityLeftColor
        {
            get => _accessibilityLeftColor;
            set
            {
                if (SetProperty(ref _accessibilityLeftColor, value))
                {
                    AccessibilityLeftBrush = new SolidColorBrush(value);
                    OnPropertyChanged(nameof(AccessibilityLeftBrush));
                    UpdateAccessibility();
                }
            }
        }

        private SolidColorBrush _accessibilityLeftBrush;
        public SolidColorBrush AccessibilityLeftBrush
        {
            get => _accessibilityLeftBrush;
            set => SetProperty(ref _accessibilityLeftBrush, value);
        }

        private Color _accessibilityRightColor;
        public Color AccessibilityRightColor
        {
            get => _accessibilityRightColor;
            set
            {
                if (SetProperty(ref _accessibilityRightColor, value))
                {
                    AccessibilityRightBrush = new SolidColorBrush(value);
                    OnPropertyChanged(nameof(AccessibilityRightBrush));
                    UpdateAccessibility();
                }
            }
        }

        private SolidColorBrush _accessibilityRightBrush;
        public SolidColorBrush AccessibilityRightBrush
        {
            get => _accessibilityRightBrush;
            set => SetProperty(ref _accessibilityRightBrush, value);
        }

        private string _contrastRatioLeft;
        public string ContrastRatioLeft
        {
            get => _contrastRatioLeft;
            set => SetProperty(ref _contrastRatioLeft, value);
        }

        private string _contrastRatioRight;
        public string ContrastRatioRight
        {
            get => _contrastRatioRight;
            set => SetProperty(ref _contrastRatioRight, value);
        }

        private string _contrastLevelLeft;
        public string ContrastLevelLeft
        {
            get => _contrastLevelLeft;
            set => SetProperty(ref _contrastLevelLeft, value);
        }

        private string _contrastLevelRight;
        public string ContrastLevelRight
        {
            get => _contrastLevelRight;
            set => SetProperty(ref _contrastLevelRight, value);
        }

        private SolidColorBrush _contrastColorLeft;
        public SolidColorBrush ContrastColorLeft
        {
            get => _contrastColorLeft;
            set => SetProperty(ref _contrastColorLeft, value);
        }

        private SolidColorBrush _contrastColorRight;
        public SolidColorBrush ContrastColorRight
        {
            get => _contrastColorRight;
            set => SetProperty(ref _contrastColorRight, value);
        }

        // --- Shades State ---
        public ObservableCollection<PaletteItem> Shades { get; } = new ObservableCollection<PaletteItem>();

        private bool _isUpdating;

        public ColorToolViewModel()
        {
            // Initialize Commands manually to avoid source generator C# 8.0 req
            UpdateFromHexInputCommand = new RelayCommand<string>(UpdateFromHexInput);
            RandomizeColorCommand = new RelayCommand(RandomizeColor);
            UpdatePaletteCommand = new RelayCommand(UpdatePalette);
            CopyHexCommand = new RelayCommand(() => CopyToClipboardAction?.Invoke(Hex));
            CopyRgbCommand = new RelayCommand(() => CopyToClipboardAction?.Invoke(RgbFormatted));
            CopyHslCommand = new RelayCommand(() => CopyToClipboardAction?.Invoke(HslFormatted));

            // Default Blue-ish
            UpdateFromRgb(59, 130, 246);
            _gradientStartColor = Color.FromRgb(59, 130, 246);
            _gradientEndColor = Color.FromRgb(139, 92, 246);

            // Initialize Accessibility Colors
            _accessibilityLeftColor = global::Avalonia.Media.Colors.White;
            _accessibilityLeftBrush = new SolidColorBrush(global::Avalonia.Media.Colors.White);
            _accessibilityRightColor = global::Avalonia.Media.Colors.Black;
            _accessibilityRightBrush = new SolidColorBrush(global::Avalonia.Media.Colors.Black);

            UpdateGradient();
            UpdateAccessibility(); // Ensure initial calculation
        }

        public void UpdateFromHexInput(string hex)
        {
            var rgb = ColorConverter.HexToRgb(hex);
            // Deconstruct tuple explicitly for C# 7.3
            int r = rgb.R;
            int g = rgb.G;
            int b = rgb.B;

            if (r != 0 || g != 0 || b != 0 || hex == "#000000" || hex == "000000")
            {
                UpdateFromRgb((byte)r, (byte)g, (byte)b);
            }
        }

        public void RandomizeColor()
        {
            var rnd = new Random();
            UpdateFromRgb((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256));
        }

        public void UpdateFromRgb(byte r, byte g, byte b)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            R = r; G = g; B = b;
            Hex = ColorConverter.RgbToHex(r, g, b);
            PreviewBrush = new SolidColorBrush(Color.FromRgb(r, g, b));

            var hsl = ColorConverter.RgbToHsl(r, g, b);
            HslH = Math.Round(hsl.H); HslS = Math.Round(hsl.S * 100); HslL = Math.Round(hsl.L * 100);

            var hsv = ColorConverter.RgbToHsv(r, g, b);
            HsvH = Math.Round(hsv.H); HsvS = Math.Round(hsv.S * 100); HsvV = Math.Round(hsv.V * 100);

            var lab = ColorConverter.RgbToLab(r, g, b);
            LabL = Math.Round(lab.L); LabA = Math.Round(lab.A); LabB = Math.Round(lab.B);

            var cmyk = ColorConverter.RgbToCmyk(r, g, b);
            CmykC = cmyk.C; CmykM = cmyk.M; CmykY = cmyk.Y; CmykK = cmyk.K;

            UpdateAccessibility();
            UpdatePalette();
            UpdateShades();

            // Sync Gradient Start Color with current color
            GradientStartColor = Color.FromRgb(r, g, b);

            OnPropertyChanged(nameof(CurrentColor));
            OnPropertyChanged(nameof(RgbFormatted));
            OnPropertyChanged(nameof(HslFormatted));

            _isUpdating = false;
        }

        public void UpdateFromHsl(double h, double s, double l)
        {
            if (_isUpdating) return;
            var rgb = ColorConverter.HslToRgb(h, s/100.0, l/100.0);
            UpdateFromRgb((byte)rgb.R, (byte)rgb.G, (byte)rgb.B);
        }

        // --- Palette Logic ---

        public void UpdatePalette()
        {
            Palette.Clear();

            // Base
            Palette.Add(new PaletteItem("Base", Hex, new SolidColorBrush(Color.FromRgb(R, G, B))));

            double baseHue = HslH;

            switch (SelectedHarmonyType)
            {
                case "Complementary":
                    AddHarmonyColor("Complementary", (baseHue + 180) % 360);
                    break;
                case "Triadic":
                    AddHarmonyColor("Triadic 1", (baseHue + 120) % 360);
                    AddHarmonyColor("Triadic 2", (baseHue + 240) % 360);
                    break;
                case "Analogous":
                    AddHarmonyColor("Analogous 1", (baseHue + 330) % 360); // -30
                    AddHarmonyColor("Analogous 2", (baseHue + 30) % 360);
                    break;
                case "Split-Complementary":
                    AddHarmonyColor("Split 1", (baseHue + 150) % 360);
                    AddHarmonyColor("Split 2", (baseHue + 210) % 360);
                    break;
                case "Tetradic":
                    AddHarmonyColor("Tetradic 1", (baseHue + 90) % 360);
                    AddHarmonyColor("Tetradic 2", (baseHue + 180) % 360);
                    AddHarmonyColor("Tetradic 3", (baseHue + 270) % 360);
                    break;
            }
        }

        private void AddHarmonyColor(string name, double hue)
        {
            var rgb = ColorConverter.HslToRgb(hue, HslS / 100.0, HslL / 100.0);
            var hex = ColorConverter.RgbToHex(rgb.R, rgb.G, rgb.B);
            Palette.Add(new PaletteItem(name, hex, new SolidColorBrush(Color.FromRgb((byte)rgb.R, (byte)rgb.G, (byte)rgb.B))));
        }

        private void UpdateGradient()
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative),
                GradientStops =
                {
                    new GradientStop(GradientStartColor, 0),
                    new GradientStop(GradientEndColor, 1)
                }
            };
            GradientPreviewBrush = brush;

            var startHex = ColorConverter.RgbToHex(GradientStartColor.R, GradientStartColor.G, GradientStartColor.B);
            var endHex = ColorConverter.RgbToHex(GradientEndColor.R, GradientEndColor.G, GradientEndColor.B);
            GradientCss = $"background: linear-gradient(to right, {startHex} 0%, {endHex} 100%);";
        }

        // --- Accessibility Logic ---
        private void UpdateAccessibility()
        {
            // Contrast with Left
            double ratioLeft = CalculateContrastRatio(R, G, B, AccessibilityLeftColor.R, AccessibilityLeftColor.G, AccessibilityLeftColor.B);
            ContrastRatioLeft = $"{ratioLeft:F2}:1";
            ContrastLevelLeft = GetLevel(ratioLeft);
            ContrastColorLeft = GetLevelBrush(ratioLeft);

            // Contrast with Right
            double ratioRight = CalculateContrastRatio(R, G, B, AccessibilityRightColor.R, AccessibilityRightColor.G, AccessibilityRightColor.B);
            ContrastRatioRight = $"{ratioRight:F2}:1";
            ContrastLevelRight = GetLevel(ratioRight);
            ContrastColorRight = GetLevelBrush(ratioRight);
        }

        private string GetLevel(double ratio)
        {
            if (ratio >= 7) return "AAA";
            if (ratio >= 4.5) return "AA";
            if (ratio >= 3) return "AA Large";
            return "Fail";
        }

        private SolidColorBrush GetLevelBrush(double ratio)
        {
            // Explicitly use Avalonia.Media.Colors
            if (ratio >= 4.5) return new SolidColorBrush(global::Avalonia.Media.Colors.Green);
            if (ratio >= 3) return new SolidColorBrush(global::Avalonia.Media.Colors.Orange);
            return new SolidColorBrush(global::Avalonia.Media.Colors.Red);
        }

        private double CalculateContrastRatio(byte r1, byte g1, byte b1, byte r2, byte g2, byte b2)
        {
            double l1 = GetLuminance(r1, g1, b1);
            double l2 = GetLuminance(r2, g2, b2);
            return (Math.Max(l1, l2) + 0.05) / (Math.Min(l1, l2) + 0.05);
        }

        private double GetLuminance(byte r, byte g, byte b)
        {
            double CalculateComponent(byte c)
            {
                double v = c / 255.0;
                return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
            }
            return 0.2126 * CalculateComponent(r) + 0.7152 * CalculateComponent(g) + 0.0722 * CalculateComponent(b);
        }

        // --- Shades Logic ---
        private void UpdateShades()
        {
            Shades.Clear();
            for (int i = 0; i < 10; i++)
            {
                double l = 10 + i * 8; // 10% to 82%
                var rgb = ColorConverter.HslToRgb(HslH, HslS / 100.0, l / 100.0);
                var hex = ColorConverter.RgbToHex(rgb.R, rgb.G, rgb.B);
                Shades.Add(new PaletteItem($"{l}%", hex, new SolidColorBrush(Color.FromRgb((byte)rgb.R, (byte)rgb.G, (byte)rgb.B))));
            }
        }
    }
}
