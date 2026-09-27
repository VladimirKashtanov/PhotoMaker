using System.Globalization;
using System.Windows;

namespace PhotoMaker.Client.Infrastructure.Converters
{
    internal class ImageLayerOpacityConverter : Base.BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not byte opacity) return DependencyProperty.UnsetValue;

            return (int)(opacity * 100.0 / 255.0);
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double opacity) return DependencyProperty.UnsetValue;

            return (byte)(opacity * 255.0 / 100);
        }
    }
}
