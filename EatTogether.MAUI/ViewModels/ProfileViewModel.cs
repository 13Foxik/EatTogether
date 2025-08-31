using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services;

namespace EatTogether.MAUI.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;

        [ObservableProperty]
        private string _displayName = string.Empty;

        public ProfileViewModel(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
            DisplayName = _currentUserService.CurrentUser.DisplayName ?? "Гость";
        }
    }
}
