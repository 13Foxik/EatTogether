using System.Globalization;

namespace EatTogether.MAUI.Converters
{
    public class SubscribeStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSubscribed)
            {
                return isSubscribed ? "PRO" : "отсутствует";
            }
            return "отсутствует";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
