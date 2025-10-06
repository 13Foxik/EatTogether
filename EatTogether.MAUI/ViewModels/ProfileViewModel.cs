using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Views.Auth;
using System.ComponentModel;
using System.Globalization;

namespace EatTogether.MAUI.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly CurrentUserService _currentUserService;
        private readonly IAuthService _authService;

        private bool _firstSignIn = true;

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
        private bool _stateOfSubscribe;
        public ProfileViewModel(CurrentUserService currentUserService, IAuthService authService)
        {
            _currentUserService = currentUserService;
            _authService = authService;

            if (_firstSignIn)
            {
                _currentUserService.UserChanged += OnUserChanged;
                _firstSignIn = false;
            }
            else
            {
            }
            UpadateUserInfo();
        }

        [RelayCommand]
        private async Task SignOutAsync()
        {
            await _authService.SignOutAsync();
            Application.Current.MainPage = new AppShell(_currentUserService);
            await Shell.Current.GoToAsync("//SignInPage");
        }

        private void OnUserChanged(object sender, UserChangedEventArgs e)
        {
            UpadateUserInfo();
            _currentUserService.UserChanged -= OnUserChanged;
        }

        private void UpadateUserInfo()
        {
            CheckFirstLastName();
            CreatedAt = _currentUserService.CurrentUser?.CreatedAt ?? DateTime.MinValue;
            DateOfBirth = _currentUserService.CurrentUser?.DateOfBitrhDay ?? DateTime.MinValue;
            StateOfSubscribe = _currentUserService.CurrentUser?.stateOfSubscribe ?? false;
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
