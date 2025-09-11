using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Views.Auth;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;
        private readonly IAuthService _authService;
        private readonly SignInViewModel _signInViewModel;

        [ObservableProperty]
        private string _displayName;
        public ProfileViewModel(CurrentUserService currentUserService, IAuthService authService, SignInViewModel signInViewModel)
        {
            _currentUserService = currentUserService;
            _authService = authService;
            
            _signInViewModel = signInViewModel;

            DisplayName = _currentUserService.CurrentUser.DisplayName ?? "Гость";
        }

        [RelayCommand]
        private async Task SignOutAsync()
        {
            await _authService.SignOutAsync();
            Application.Current.MainPage = new AppShell(_currentUserService);
            await Shell.Current.GoToAsync("//SignInPage");
        }

    }
}
