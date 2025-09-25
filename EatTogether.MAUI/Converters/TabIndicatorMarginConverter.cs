using System.Globalization;

namespace EatTogether.MAUI.Converters;

public class TabIndicatorMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int selectedIndex)
        {
            // Используем относительное позиционирование вместо абсолютного
            var margin = selectedIndex * 33.3; // 33.3% на каждую вкладку
            return new Thickness(margin, 0, 0, 0);
        }
        return new Thickness(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}