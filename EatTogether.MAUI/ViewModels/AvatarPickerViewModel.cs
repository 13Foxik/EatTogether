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
        public ObservableCollection<AvatarColorChoice> Colors { get; }

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasCurrentAvatar))]
        [NotifyPropertyChangedFor(nameof(HasNoCurrentAvatar))]
        private string _currentAvatarKey;

        [ObservableProperty]
        private string _currentAvatarColor;

        public bool HasCurrentAvatar => !string.IsNullOrEmpty(CurrentAvatarKey);
        public bool HasNoCurrentAvatar => string.IsNullOrEmpty(CurrentAvatarKey);

        public AvatarPickerViewModel(CurrentUserService currentUserService, ICloudStoreService cloudStoreService)
        {
            _currentUserService = currentUserService;
            _cloudStoreService = cloudStoreService;

            var currentKey = _currentUserService.CurrentUser?.Avatar;
            var currentColor = _currentUserService.CurrentUser?.AvatarColor ?? "#1F744D";

            CurrentAvatarKey = currentKey ?? string.Empty;
            CurrentAvatarColor = currentColor;

            Avatars = new ObservableCollection<AvatarChoice>(
                AvatarPresets.All.Select(p => new AvatarChoice
                {
                    Key = p.Key,
                    FileName = p.FileName,
                    DisplayName = p.DisplayName,
                    IsSelected = string.Equals(p.Key, currentKey, StringComparison.OrdinalIgnoreCase),
                }));

            Colors = new ObservableCollection<AvatarColorChoice>(
                AvatarColors.All.Select(c => new AvatarColorChoice
                {
                    HexColor = c.Hex,
                    DisplayName = c.Name,
                    IsSelected = string.Equals(c.Hex, currentColor, StringComparison.OrdinalIgnoreCase),
                }));
        }

        [RelayCommand]
        private async Task GoBack()
        {
            if (Application.Current?.MainPage is Views.Main.MainPage mainPage &&
                mainPage.CurrentPage is NavigationPage navPage)
            {
                await navPage.Navigation.PopAsync();
            }
        }

        [RelayCommand]
        private void SelectAvatar(AvatarChoice? choice)
        {
            if (choice == null) return;

            foreach (var a in Avatars)
                a.IsSelected = string.Equals(a.Key, choice.Key, StringComparison.OrdinalIgnoreCase);

            CurrentAvatarKey = choice.Key;
        }

        [RelayCommand]
        private void SelectColor(AvatarColorChoice? choice)
        {
            if (choice == null) return;

            foreach (var c in Colors)
                c.IsSelected = string.Equals(c.HexColor, choice.HexColor, StringComparison.OrdinalIgnoreCase);

            CurrentAvatarColor = choice.HexColor;
        }

        [RelayCommand]
        private async Task Save()
        {
            var user = _currentUserService.CurrentUser;
            if (user == null || IsBusy) return;

            try
            {
                IsBusy = true;

                user.Avatar = CurrentAvatarKey;
                user.AvatarColor = CurrentAvatarColor;
                _currentUserService.SetCurrentUser(user);

                await _cloudStoreService.UpdateUserAvatar(user);

                // Возвращаемся назад после сохранения
                await GoBack();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения аватара: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ClearAvatar()
        {
            foreach (var a in Avatars) a.IsSelected = false;
            CurrentAvatarKey = string.Empty;
        }
    }

    public partial class AvatarChoice : ObservableObject
    {
        [ObservableProperty] private string _key = string.Empty;
        [ObservableProperty] private string _fileName = string.Empty;
        [ObservableProperty] private string _displayName = string.Empty;
        [ObservableProperty] private bool _isSelected;
    }

    public partial class AvatarColorChoice : ObservableObject
    {
        [ObservableProperty] private string _hexColor = string.Empty;
        [ObservableProperty] private string _displayName = string.Empty;
        [ObservableProperty] private bool _isSelected;
    }
}
