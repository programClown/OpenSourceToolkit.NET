using Avalonia.Data.Converters;
using Avalonia.Media;
using DiffPlex.DiffBuilder.Model;
using System;
using System.Globalization;

namespace OpenSourceToolkit.NET.Converters
{
    public class DiffTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChangeType changeType)
            {
                switch (changeType)
                {
                    case ChangeType.Inserted:
                        return new SolidColorBrush(Color.Parse("#3300FF00")); // Light Green
                    case ChangeType.Deleted:
                        return new SolidColorBrush(Color.Parse("#33FF0000")); // Light Red
                    case ChangeType.Modified:
                        return new SolidColorBrush(Color.Parse("#33FFFF00")); // Light Yellow
                    case ChangeType.Imaginary:
                        return new SolidColorBrush(global::Avalonia.Media.Colors.LightGray);
                    case ChangeType.Unchanged:
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
