using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _displayName;
        public ProfileViewModel(CurrentUserService currentUserService, IAuthService authService)
        {
            _currentUserService = currentUserService;
            _authService = authService;
            DisplayName = _currentUserService.CurrentUser.DisplayName ?? "Гость";
        }

        [RelayCommand]
        private async Task SignOutAsync()
        {
            await _authService.SignOutAsync();
            await Shell.Current.GoToAsync("//SignInPage");
        }

    }
}
