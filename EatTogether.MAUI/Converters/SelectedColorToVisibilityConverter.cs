using System.Globalization;

namespace EatTogether.MAUI.Converters
{
    public class SelectedColorToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorHex && parameter is string selectedColorHex)
            {
                return colorHex == selectedColorHex;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
