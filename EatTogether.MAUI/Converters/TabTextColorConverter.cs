using System.Globalization;

namespace EatTogether.MAUI.Converters;

public class TabTextColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int selectedIndex && parameter is string paramStr && int.TryParse(paramStr, out int tabIndex))
        {
            return selectedIndex == tabIndex ?
                Application.Current.Resources["PrimaryColor"] :
                Application.Current.Resources["SecondaryTextColor"];
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}