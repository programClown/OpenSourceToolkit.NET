using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using System;
using System.Globalization;

namespace OpenSourceToolkit.NET.Controls
{
    public partial class NumericEntry : UserControl
    {
        private bool _isUpdating;
        private string _lastValidText = string.Empty;

        public static readonly StyledProperty<double?> ValueProperty =
            AvaloniaProperty.Register<NumericEntry, double?>(nameof(Value), defaultBindingMode: BindingMode.TwoWay);

        public double? Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly StyledProperty<string> WatermarkProperty =
            AvaloniaProperty.Register<NumericEntry, string>(nameof(Watermark));

        public string Watermark
        {
            get => GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly StyledProperty<string> FormatStringProperty =
            AvaloniaProperty.Register<NumericEntry, string>(nameof(FormatString));

        public string FormatString
        {
            get => GetValue(FormatStringProperty);
            set => SetValue(FormatStringProperty, value);
        }

        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<NumericEntry, double>(nameof(Minimum), defaultValue: (double)decimal.MinValue);

        public double Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<NumericEntry, double>(nameof(Maximum), defaultValue: (double)decimal.MaxValue);

        public double Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly StyledProperty<double> IncrementProperty =
            AvaloniaProperty.Register<NumericEntry, double>(nameof(Increment), defaultValue: 1.0);

        public double Increment
        {
            get => GetValue(IncrementProperty);
            set => SetValue(IncrementProperty, value);
        }

        public static readonly StyledProperty<int> MaxLengthProperty =
            AvaloniaProperty.Register<NumericEntry, int>(nameof(MaxLength), defaultValue: 0);

        public int MaxLength
        {
            get => GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        public static readonly StyledProperty<int> MaxDecimalPlacesProperty =
            AvaloniaProperty.Register<NumericEntry, int>(nameof(MaxDecimalPlaces), defaultValue: 100);

        public int MaxDecimalPlaces
        {
            get => GetValue(MaxDecimalPlacesProperty);
            set => SetValue(MaxDecimalPlacesProperty, value);
        }

        public NumericEntry()
        {
            InitializeComponent();

            // Input is source-generated from x:Name in AXAML
            // Block obviously invalid keys (letters) before they reach the TextBox
            Input.KeyDown += InputOnKeyDown;
            Input.PropertyChanged += InputOnPropertyChanged;
            this.PropertyChanged += NumericEntryOnPropertyChanged;

            // Initialize from initial Value
            UpdateTextFromValue();
        }

        private void InputOnKeyDown(object sender, KeyEventArgs e)
        {
            // Allow navigation, backspace, delete etc.
            if (e.Key == Key.Back ||
                e.Key == Key.Delete ||
                e.Key == Key.Tab ||
                e.Key == Key.Left ||
                e.Key == Key.Right ||
                e.Key == Key.Home ||
                e.Key == Key.End)
            {
                return;
            }

            // Block alphabetic keys completely so letters never appear
            if (e.Key >= Key.A && e.Key <= Key.Z)
            {
                e.Handled = true;
            }
        }

        private void InputOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == TextBox.TextProperty && !_isUpdating)
            {
                var text = e.NewValue as string ?? string.Empty;
                ValidateAndUpdateValueFromText(text);
            }
        }

        private void NumericEntryOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ValueProperty && !_isUpdating)
            {
                UpdateTextFromValue();
            }
        }

        private void UpdateTextFromValue()
        {
            _isUpdating = true;

            if (Value.HasValue)
            {
                var v = Value.Value;
                if (!string.IsNullOrEmpty(FormatString))
                {
                    Input.Text = v.ToString(FormatString, CultureInfo.CurrentCulture);
                }
                else
                {
                    Input.Text = v.ToString(CultureInfo.CurrentCulture);
                }
            }
            else
            {
                Input.Text = string.Empty;
            }

            _lastValidText = Input.Text ?? string.Empty;
            _isUpdating = false;
        }

        private void ValidateAndUpdateValueFromText(string text)
        {
            // Allow empty as null
            if (string.IsNullOrWhiteSpace(text))
            {
                _isUpdating = true;
                Value = null;
                _lastValidText = string.Empty;
                _isUpdating = false;
                return;
            }

            double parsed;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed))
            {
                // Check decimal places
                string separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
                if (text.Contains(separator))
                {
                    var parts = text.Split(new[] { separator }, StringSplitOptions.None);
                    if (parts.Length > 1 && parts[1].Length > MaxDecimalPlaces)
                    {
                        // Too many decimal places: revert
                        _isUpdating = true;
                        Input.Text = _lastValidText;
                        _isUpdating = false;
                        return;
                    }
                }

                if (parsed < Minimum || parsed > Maximum)
                {
                    // Out of bounds: revert
                    _isUpdating = true;
                    Input.Text = _lastValidText;
                    _isUpdating = false;
                    return;
                }

                _isUpdating = true;
                Value = parsed;
                _lastValidText = text;
                _isUpdating = false;
            }
            else
            {
                // Invalid: revert to last valid text
                _isUpdating = true;
                Input.Text = _lastValidText;
                _isUpdating = false;
            }
        }
    }
}
