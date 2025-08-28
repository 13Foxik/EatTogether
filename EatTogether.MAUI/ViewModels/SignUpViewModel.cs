using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace EatTogether.MAUI.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private DateTime _birthDate;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private string _confirmPassword;

        [ObservableProperty]
        private bool _isConfirmPasswordVisible = true;

        [ObservableProperty]
        private bool _isPasswordVisible = true;

        [RelayCommand]
        private async Task GoToSignIn()
        {
            await Shell.Current.GoToAsync("//SignInPage");
        }

        [RelayCommand]
        private void TogglePassword()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        [RelayCommand]
        private void ToggleConfirmPassword()
        {
            IsConfirmPasswordVisible = !IsConfirmPasswordVisible;
        }

    }
}
