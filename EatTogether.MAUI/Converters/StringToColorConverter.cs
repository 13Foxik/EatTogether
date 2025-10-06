using System.Globalization;

namespace EatTogether.MAUI.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorHex && !string.IsNullOrEmpty(colorHex))
            {
                return Color.FromArgb(colorHex);
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}