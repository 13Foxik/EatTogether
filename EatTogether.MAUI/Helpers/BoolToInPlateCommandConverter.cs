// BoolToInPlateCommandConverter.cs
using System;
using System.Globalization;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace EatTogether.MAUI.Helpers
{
    public class BoolToInPlateCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isInPlate)
            {
                return isInPlate ? "RemoveFromPlateCommand" : "AddToPlateCommand";
            }
            return "AddToPlateCommand";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // BoolToInPlateTextConverter.cs
    public class BoolToInPlateTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isInPlate)
            {
                return isInPlate ? "✓" : "+";
            }
            return "+";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // BoolToInPlateBackgroundColorConverter.cs
    public class BoolToInPlateBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isInPlate)
            {
                return isInPlate ?
                    Application.Current.RequestedTheme == AppTheme.Light ?
                        Color.FromArgb("#E8F5E9") : // Light green background
                        Color.FromArgb("#1B5E20") : // Dark green background
                    Application.Current.RequestedTheme == AppTheme.Light ?
                        Color.FromArgb("#F5F5F5") : // Light gray background
                        Color.FromArgb("#1E1E1E");  // Dark gray background
            }
            return Application.Current.RequestedTheme == AppTheme.Light ?
                Color.FromArgb("#F5F5F5") :
                Color.FromArgb("#1E1E1E");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // BoolToInPlateButtonBackgroundColorConverter.cs
    public class BoolToInPlateButtonBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isInPlate)
            {
                return isInPlate ?
                    Application.Current.RequestedTheme == AppTheme.Light ?
                        Color.FromArgb("#C8E6C9") : // Light green
                        Color.FromArgb("#388E3C") : // Dark green
                    Application.Current.RequestedTheme == AppTheme.Light ?
                        Color.FromArgb("#E3F2FD") : // Light blue
                        Color.FromArgb("#0D47A1");  // Dark blue
            }
            return Application.Current.RequestedTheme == AppTheme.Light ?
                Color.FromArgb("#E3F2FD") :
                Color.FromArgb("#0D47A1");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // BoolToInPlateButtonBorderColorConverter.cs
    public class BoolToInPlateButtonBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isInPlate)
            {
                return isInPlate ?
                    Application.Current.RequestedTheme == AppTheme.Light ?
                        Color.FromArgb("#81C784") : // Light green border
                        Color.FromArgb("#66BB6A") : // Dark green border
                    Application.Current.RequestedTheme == AppTheme.Light ?
                        Color.FromArgb("#64B5F6") : // Light blue border
                        Color.FromArgb("#1976D2");  // Dark blue border
            }
            return Application.Current.RequestedTheme == AppTheme.Light ?
                Color.FromArgb("#64B5F6") :
                Color.FromArgb("#1976D2");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // BoolToInPlateButtonTextColorConverter.cs
    public class BoolToInPlateButtonTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isInPlate)
            {
                return isInPlate ?
                    Application.Current.RequestedTheme == AppTheme.Light ?
                        Color.FromArgb("#1B5E20") : // Dark green text
                        Color.FromArgb("#C8E6C9") : // Light green text
                    Application.Current.RequestedTheme == AppTheme.Light ?
                        Color.FromArgb("#0D47A1") : // Dark blue text
                        Color.FromArgb("#90CAF9");  // Light blue text
            }
            return Application.Current.RequestedTheme == AppTheme.Light ?
                Color.FromArgb("#0D47A1") :
                Color.FromArgb("#90CAF9");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}