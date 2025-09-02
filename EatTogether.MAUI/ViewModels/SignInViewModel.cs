using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private string _emaill;

        [ObservableProperty]
        private string _password;

        [ObservableProperty]
        private bool _isPasswordVisible = true;

        [RelayCommand]
        private async Task SignInAsync()
        {
            try
            {
                await _authService.SignInAsync(Emaill, Password);

                await Shell.Current.GoToAsync("//ProfilePage");
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
    }
}
