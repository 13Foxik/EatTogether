using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Views.Auth;
using EatTogether.MAUI.Views.Main;
using EatTogether.MAUI.Views.Main.ProfilePages;
using System.ComponentModel;
using System.Globalization;

namespace EatTogether.MAUI.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _displayName;

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                if (SetProperty(ref _createdAt, value))
                {
                    FormattedCreatedAt = _createdAt.ToString("dd MMMM, yyyy", new CultureInfo("ru-RU"));
                }
            }
        }

        private DateTime _dateOfBirth;
        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (SetProperty(ref _dateOfBirth, value))
                {
                    var utcDate = DateTime.SpecifyKind(_dateOfBirth, DateTimeKind.Utc);
                    var localDate = utcDate.ToLocalTime();
                    FormattedDateOfBirth = localDate.ToString("dd MMMM, yyyy", new CultureInfo("ru-RU"));
                }
            }
        }

        [ObservableProperty]
        private string _formattedCreatedAt;

        [ObservableProperty]
        private string _formattedDateOfBirth;

        [ObservableProperty]
        private string _firstName;

        [ObservableProperty]
        private string _lastName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasAvatar))]
        [NotifyPropertyChangedFor(nameof(HasNoAvatar))]
        private string _avatar;

        [ObservableProperty]
        private string _avatarColor = "#1F744D";

        public bool HasAvatar => !string.IsNullOrEmpty(Avatar);
        public bool HasNoAvatar => string.IsNullOrEmpty(Avatar);

        public ProfileViewModel(CurrentUserService currentUserService, IAuthService authService)
        {
            _currentUserService = currentUserService;
            _authService = authService;

            _currentUserService.UserChanged += OnUserChanged;
            UpadateUserInfo();
        }

        [RelayCommand]
        private async Task SignOutAsync()
        {
            await _authService.SignOutAsync();
            Application.Current.MainPage = new AppShell(_currentUserService);
            await Shell.Current.GoToAsync("//SignInPage");
        }

        [RelayCommand]
        private async Task OpenAvatarPicker()
        {
            if (Application.Current?.MainPage is MainPage mainPage &&
                mainPage.CurrentPage is NavigationPage navPage)
            {
                await navPage.Navigation.PushAsync(App.Services.GetService<AvatarPickerPage>());
            }
        }

        private void OnUserChanged(object sender, UserChangedEventArgs e)
        {
            UpadateUserInfo();
        }

        private void UpadateUserInfo()
        {
            CheckFirstLastName();
            CreatedAt = _currentUserService.CurrentUser?.CreatedAt ?? DateTime.MinValue;
            DateOfBirth = _currentUserService.CurrentUser?.DateOfBirthday ?? DateTime.MinValue;
            Avatar = _currentUserService.CurrentUser?.Avatar ?? string.Empty;
            AvatarColor = _currentUserService.CurrentUser?.DisplayColor ?? "#1F744D";
        }
        private void CheckFirstLastName()
        {
            FirstName = _currentUserService.CurrentUser?.FirstName ?? "";
            LastName = _currentUserService.CurrentUser?.LastName ?? "";

            if (FirstName == "" && LastName == "")
            {
                FirstName = _currentUserService.CurrentUser?.DisplayName ?? "Гость";
                DisplayName = "Личные данные скрыты";
            }
            else
            {
                DisplayName = _currentUserService.CurrentUser?.DisplayName ?? "Гость";
                FirstName = _currentUserService.CurrentUser?.FirstName ?? " ";
                LastName = _currentUserService.CurrentUser?.LastName ?? " ";
            }
        }
    }
}
