using System.Globalization;

namespace EatTogether.MAUI.Converters
{
    public class ExpandedIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? "expand_less" : "expand_more";
            }
            return "expand_more";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}