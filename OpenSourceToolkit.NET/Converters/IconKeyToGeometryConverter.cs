using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using OpenSourceToolkit.NET.ViewModels;

namespace OpenSourceToolkit.NET.Converters
{
    public class IconKeyToGeometryConverter : IValueConverter
    {
        public static readonly IconKeyToGeometryConverter Instance = new IconKeyToGeometryConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var (iconKey, iconPath) = ExtractIcon(value);

            var geometry = TryGetResourceGeometry(iconKey);
            if (geometry != null)
            {
                return geometry;
            }

            if (!string.IsNullOrWhiteSpace(iconPath))
            {
                try
                {
                    return StreamGeometry.Parse(iconPath);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        private static (string iconKey, string iconPath) ExtractIcon(object value)
        {
            switch (value)
            {
                case ToolViewModel tool:
                    return (tool.IconKey, tool.IconPath);
                case QuickActionItem quickAction:
                    return (quickAction.IconKey, quickAction.IconPath);
                case ToolGroup group:
                    return (null, group.IconPath);
                case string key:
                    return (key, null);
                default:
                    return (null, null);
            }
        }

        private static StreamGeometry TryGetResourceGeometry(string iconKey)
        {
            var app = Application.Current;
            if (string.IsNullOrWhiteSpace(iconKey) || app?.Resources is null)
            {
                return null;
            }

            if (app.Resources.TryGetResource(iconKey, app.ActualThemeVariant, out var resource))
            {
                switch (resource)
                {
                    case StreamGeometry geometry:
                        return geometry;
                    case string data:
                        try
                        {
                            return StreamGeometry.Parse(data);
                        }
                        catch
                        {
                            return null;
                        }
                }
            }

            return null;
        }
    }
}
