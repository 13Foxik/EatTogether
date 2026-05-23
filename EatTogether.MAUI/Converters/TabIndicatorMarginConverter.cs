using System.Globalization;

namespace EatTogether.MAUI.Converters;

public class TabIndicatorMarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int selectedIndex)
        {
            // Фиксированные отступы, рассчитанные для центрирования
            // под каждой вкладкой
            switch (selectedIndex)
            {
                case 0: return new Thickness(25, 0, 0, 0);   // Центр под "Активность"
                case 1: return new Thickness(137, 0, 0, 0);  // Центр под "Участники"  
                case 2: return new Thickness(250, 0, 0, 0);  // Центр под "Запросы"
                default: return new Thickness(25, 0, 0, 0);
            }
        }
        return new Thickness(25, 0, 0, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}