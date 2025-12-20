using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace OpenSourceToolkit.NET.Converters
{
    public class SidebarWidthConverter : IValueConverter
    {
        public static readonly SidebarWidthConverter Instance = new SidebarWidthConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? 260.0 : 52.0;
            }
            return 260.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ToggleIconConverter : IValueConverter
    {
        public static readonly ToggleIconConverter Instance = new ToggleIconConverter();

        // Chevron Left when expanded, Chevron Right when collapsed
        private const string ChevronLeft = "M15.41,16.58L10.83,12L15.41,7.41L14,6L8,12L14,18L15.41,16.58Z";
        private const string ChevronRight = "M8.59,16.58L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.58Z";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? ChevronLeft : ChevronRight;
            }
            return ChevronLeft;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FavoriteIconConverter : IValueConverter
    {
        public static readonly FavoriteIconConverter Instance = new FavoriteIconConverter();

        // Star filled vs outline
        private const string StarFilled = "M12,17.27L18.18,21L16.54,13.97L22,9.24L14.81,8.62L12,2L9.19,8.62L2,9.24L7.45,13.97L5.82,21L12,17.27Z";
        private const string StarOutline = "M12,15.39L8.24,17.66L9.23,13.38L5.91,10.5L10.29,10.13L12,6.09L13.71,10.13L18.09,10.5L14.77,13.38L15.76,17.66M22,9.24L14.81,8.63L12,2L9.19,8.63L2,9.24L7.45,13.97L5.82,21L12,17.27L18.18,21L16.54,13.97L22,9.24Z";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                return isFavorite ? StarFilled : StarOutline;
            }
            return StarOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FavoriteTooltipConverter : IValueConverter
    {
        public static readonly FavoriteTooltipConverter Instance = new FavoriteTooltipConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFavorite)
            {
                return isFavorite ? "Remove from favorites" : "Add to favorites";
            }
            return "Add to favorites";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
