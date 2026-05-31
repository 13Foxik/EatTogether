using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;
using System.Globalization;

namespace EatTogether.MAUI.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;
        private readonly IUserService _userService;

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _nickName = string.Empty;

        [ObservableProperty]
        private DateTime _dateOfBirth = new DateTime(2000, 1, 1);

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public SettingsViewModel(CurrentUserService currentUserService, IUserService userService)
        {
            _currentUserService = currentUserService;
            _userService = userService;
            LoadFromCurrentUser();
        }

        private void LoadFromCurrentUser()
        {
            var user = _currentUserService.CurrentUser;
            if (user == null) return;

            FirstName = user.FirstName ?? string.Empty;
            LastName = user.LastName ?? string.Empty;
            NickName = user.DisplayName ?? string.Empty;

            // Если дата не задана — ставим дефолт, чтобы DatePicker не взрывался
            DateOfBirth = user.DateOfBirthday == DateTime.MinValue
                ? new DateTime(2000, 1, 1)
                : user.DateOfBirthday;
        }

        [RelayCommand]
        private async Task Save()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
                && string.IsNullOrWhiteSpace(NickName))
            {
                ErrorMessage = "Заполните хотя бы одно поле";
                return;
            }

            try
            {
                IsBusy = true;

                var user = _currentUserService.CurrentUser;
                if (user == null) return;

                user.FirstName = FirstName.Trim();
                user.LastName = LastName.Trim();
                user.DisplayName = string.IsNullOrWhiteSpace(NickName)
                    ? $"{user.FirstName} {user.LastName}".Trim()
                    : NickName.Trim();
                user.DateOfBirthday = DateTime.SpecifyKind(DateOfBirth.Date, DateTimeKind.Utc);

                await _userService.UpdateProfile(user);

                // Уведомляем всех подписчиков об изменении
                _currentUserService.SetCurrentUser(user);

                await GoBack();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Ошибка при сохранении. Попробуйте ещё раз.";
                System.Diagnostics.Debug.WriteLine($"SettingsViewModel.Save: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            if (Application.Current?.MainPage is Views.Main.MainPage mainPage &&
                mainPage.CurrentPage is NavigationPage nav)
            {
                await nav.Navigation.PopAsync();
            }
        }
    }
}
