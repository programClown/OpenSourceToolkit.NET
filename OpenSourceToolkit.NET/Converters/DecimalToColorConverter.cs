using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OpenSourceToolkit.NET.Converters
{
    /// <summary>
    /// Converts decimal color value to Avalonia Color for visual display.
    /// </summary>
    public class DecimalToColorConverter : IValueConverter
    {
        public static DecimalToColorConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decValue)
            {
                var intValue = (uint)Math.Max(0, Math.Min(16777215, decValue));
                var color = Color.FromRgb(
                    (byte)((intValue >> 16) & 0xFF),
                    (byte)((intValue >> 8) & 0xFF),
                    (byte)(intValue & 0xFF));
                return new SolidColorBrush(color);
            }
            return new SolidColorBrush(Avalonia.Media.Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return (decimal)((color.R << 16) | (color.G << 8) | color.B);
            }
            return 16711680m; // #FF0000
        }
    }
}
