using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Helpers;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.ViewModels
{
    public partial class AvatarPickerViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;
        private readonly ICloudStoreService _cloudStoreService;

        public ObservableCollection<AvatarChoice> Avatars { get; }

        [ObservableProperty]
        private bool _isBusy;

        public AvatarPickerViewModel(CurrentUserService currentUserService, ICloudStoreService cloudStoreService)
        {
            _currentUserService = currentUserService;
            _cloudStoreService = cloudStoreService;

            var currentKey = _currentUserService.CurrentUser?.Avatar;
            Avatars = new ObservableCollection<AvatarChoice>(
                AvatarPresets.All.Select(p => new AvatarChoice
                {
                    Key = p.Key,
                    FileName = p.FileName,
                    DisplayName = p.DisplayName,
                    IsSelected = string.Equals(p.Key, currentKey, StringComparison.OrdinalIgnoreCase),
                }));
        }

        [RelayCommand]
        private async Task SelectAvatar(AvatarChoice? choice)
        {
            if (choice == null || IsBusy) return;
            var user = _currentUserService.CurrentUser;
            if (user == null) return;

            try
            {
                IsBusy = true;

                foreach (var a in Avatars)
                    a.IsSelected = string.Equals(a.Key, choice.Key, StringComparison.OrdinalIgnoreCase);

                user.Avatar = choice.Key;
                _currentUserService.SetCurrentUser(user);

                await _cloudStoreService.UpdateUserAvatar(user);

                if (Application.Current?.MainPage?.Navigation is { } nav && nav.NavigationStack.Count > 1)
                    await nav.PopAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка смены аватара: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ClearAvatar()
        {
            var user = _currentUserService.CurrentUser;
            if (user == null || IsBusy) return;

            try
            {
                IsBusy = true;
                foreach (var a in Avatars) a.IsSelected = false;

                user.Avatar = string.Empty;
                _currentUserService.SetCurrentUser(user);

                await _cloudStoreService.UpdateUserAvatar(user);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сброса аватара: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public partial class AvatarChoice : ObservableObject
    {
        [ObservableProperty]
        private string _key = string.Empty;

        [ObservableProperty]
        private string _fileName = string.Empty;

        [ObservableProperty]
        private string _displayName = string.Empty;

        [ObservableProperty]
        private bool _isSelected;
    }
}
