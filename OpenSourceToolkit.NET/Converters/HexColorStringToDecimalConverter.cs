using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OpenSourceToolkit.NET.Converters
{
    /// <summary>
    /// Converts between hex color string (#RRGGBB) and decimal value for DaisyNumericUpDown.
    /// </summary>
    public class HexColorStringToDecimalConverter : IValueConverter
    {
        public static HexColorStringToDecimalConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // String to Decimal (for binding to NumericUpDown.Value)
            if (value is string hexString)
            {
                try
                {
                    var hex = hexString.TrimStart('#');
                    if (long.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
                    {
                        return (decimal)result;
                    }
                }
                catch
                {
                    // Return default red if parsing fails
                }
                return 16711680m; // #FF0000 = red
            }
            return 16711680m;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Decimal to String (when NumericUpDown changes value)
            if (value is decimal decValue)
            {
                var longValue = (long)Math.Max(0, Math.Min(16777215, decValue)); // Clamp to 0x000000-0xFFFFFF
                return $"#{longValue:X6}";
            }
            return "#FF0000";
        }
    }
}
