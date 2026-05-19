using System.Globalization;
using EatTogether.MAUI.Helpers;

namespace EatTogether.MAUI.Converters
{
    public class AvatarKeyToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var key = value as string;
            var fileName = AvatarPresets.FileNameForKey(key);
            if (fileName == null)
                return null;

            return ImageSource.FromFile(fileName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
