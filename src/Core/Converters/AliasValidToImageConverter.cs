using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Hummingbird.Core.Converters
{
    /// <summary>
    /// Converts the validity of a group alias into an image representation of the state.
    /// </summary>
    class AliasValidToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (!(bool) value) return new BitmapImage(new Uri("/Images/Deny.png", UriKind.Relative));
            return new BitmapImage(new Uri("/Images/Checkmark.png", UriKind.Relative)); ;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
