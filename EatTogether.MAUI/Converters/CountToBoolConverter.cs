using System.Globalization;
using System.Collections;

namespace EatTogether.MAUI.Converters
{
    public class CountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count > 0;

            if (value is ICollection collection)
                return collection.Count > 0;

            if (value is IEnumerable enumerable)
            {
                // Для любых коллекций через перечисление
                var countFromEnum = enumerable.Cast<object>().Count();
                return countFromEnum > 0;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CountToInvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count == 0;

            if (value is ICollection collection)
                return collection.Count == 0;

            if (value is IEnumerable enumerable)
            {
                // Для любых коллекций через перечисление
                var countFromEnum = enumerable.Cast<object>().Count();
                return countFromEnum == 0;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsEqualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(parameter) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}