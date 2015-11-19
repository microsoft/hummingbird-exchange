using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hummingbird.Core.Converters
{
    /// <summary>
    /// Converts the visibility state to the inverse of a boolean representation.
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (bool) value;
            return val ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}