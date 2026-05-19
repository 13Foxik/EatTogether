using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Views.Main;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.ViewModels
{
    public partial class SignInViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        public SignInViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [ObservableProperty]
        private string _email;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isPasswordVisible = true;

        [RelayCommand]
        private async Task SignInWithEmailAsync()
        {
            try
            {
                await _authService.SignInWithEmailAsync(Email, Password);
                Application.Current.MainPage = new MainPage();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task GoToSignUp()
        {
            await Shell.Current.GoToAsync("//SignUpPage");
        }

        [RelayCommand]
        private void TogglePassword()
        {
            IsPasswordVisible = !IsPasswordVisible;
        }

        [RelayCommand]
        private void ChangeLanguage()
        {
            // TODO: реализовать переключение языка
        }

        [RelayCommand]
        private async Task SignInWithGoogleAsync()
        {
            try
            {
                await _authService.SignInWithGoogleAsync();
                Application.Current.MainPage = new MainPage();
            }
            catch (NotImplementedException)
            {
                await Shell.Current.DisplayAlert("Скоро", "Вход через Google будет доступен в следующей версии", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task SignInWithAppleAsync()
        {
            try
            {
                await _authService.SignInWithAppleAsync();
                Application.Current.MainPage = new MainPage();
            }
            catch (NotImplementedException)
            {
                await Shell.Current.DisplayAlert("Скоро", "Вход через Apple будет доступен в следующей версии", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
        }
    }
}
