using System.Globalization;

namespace PhotoMaker.Client.Infrastructure.Converters
{
    internal class EmptyStringToIntConverter : Base.BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "0";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value?.ToString())) return 0;

            return int.TryParse(value.ToString(), out int result) ? result : 0;
        }
    }
}
