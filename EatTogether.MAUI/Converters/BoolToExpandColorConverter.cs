using System.Globalization;

namespace EatTogether.MAUI.Converters
{
    /// <summary>
    /// true (раскрыто) → зелёный PrimaryColor, false (закрыто) → серый
    /// </summary>
    public class BoolToExpandColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded && isExpanded)
                return Color.FromArgb("#1F744D"); // PrimaryColor — раскрыто
            return Color.FromArgb("#9E9E9E");     // серый — закрыто
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// true → ▲ (стрелка вверх), false → ▼ (стрелка вниз)
    /// </summary>
    public class ExpandedArrowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded && isExpanded)
                return "▲";
            return "▼";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
