using System.Globalization;

namespace EatTogether.MAUI.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public static InvertedBoolConverter Instance { get; } = new InvertedBoolConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue ? !boolValue : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue ? !boolValue : value;
        }
    }
}