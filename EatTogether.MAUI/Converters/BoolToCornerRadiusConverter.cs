using System.Globalization;

namespace EatTogether.MAUI.Converters
{
    public class BoolToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasFamily && hasFamily)
            {
                // Закругленные нижние углы когда есть семья
                return new CornerRadius(0, 0, 30, 30);
            }

            // Прямые углы когда нет семьи
            return new CornerRadius(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToMenuTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasFamily && hasFamily)
            {
                return "Меню семьи";
            }

            return "Меню";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToTitleSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool hasFamily && hasFamily)
            {
                return 28; // Большой размер когда есть семья
            }

            return 24; // Меньший размер когда нет семьи
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}