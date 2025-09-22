using System.Globalization;

namespace EatTogether.MAUI.Converters
{
    public class SubscribeStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSubscribed)
            {
                return isSubscribed ?
                    Application.Current.Resources["PrimaryColor"] :
                    Application.Current.Resources["DarkRed"];
            }
            return Application.Current.Resources["DarkRed"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
