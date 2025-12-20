using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace OpenSourceToolkit.NET.Converters
{
    /// <summary>
    /// Converts bool to GridLength: true = * (star), false = 0 (hidden).
    /// </summary>
    public class BoolToGridLengthConverter : IValueConverter
    {
        public static readonly BoolToGridLengthConverter Instance = new BoolToGridLengthConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isVisible && isVisible)
                return new GridLength(1, GridUnitType.Star);
            return new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts bool to Thickness margin: true = "0,0,5,0" (right margin for split view), false = "0".
    /// </summary>
    public class BoolToMarginConverter : IValueConverter
    {
        public static readonly BoolToMarginConverter Instance = new BoolToMarginConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasRightPanel && hasRightPanel)
                return new Thickness(0, 0, 5, 0);
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts bool to opacity: true = 1.0 (fully visible), false = 0.4 (dimmed).
    /// </summary>
    public class BoolToOpacityConverter : IValueConverter
    {
        public static readonly BoolToOpacityConverter Instance = new BoolToOpacityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isActive && isActive ? 1.0 : 0.4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
