using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _displayName;

        [ObservableProperty]
        private string _emaill;

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

        private readonly IEmailAuth _emailAuthService;

        public SignUpViewModel(IEmailAuth emailAuthService)
        {
            _emailAuthService = emailAuthService;
        }

        [RelayCommand]
        private async Task SignUpAsync()
        {
            try
            {
                await _emailAuthService.SignUpAsync(Emaill, Password, ConfirmPassword, DisplayName);
                await Shell.Current.GoToAsync("//ProfilePage");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

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
