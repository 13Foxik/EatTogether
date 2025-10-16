using System.Globalization;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Converters
{
    public class EnumToDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FamilyRole role)
            {
                return role switch
                {
                    FamilyRole.Owner => "Создатель семьи",
                    FamilyRole.Admin => "Администратор",
                    FamilyRole.Member => "Участник",
                    _ => "Участник"
                };
            }

            return "Участник";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}