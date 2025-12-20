using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenSourceToolkit.NET.Converters
{
    public class PeakMeterWidthConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count >= 2 && values[0] is double peakLevel && values[1] is double parentWidth)
            {
                return Math.Max(0, Math.Min(1, peakLevel)) * parentWidth;
            }
            return 0.0;
        }
    }
}
