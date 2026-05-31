using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EatTogether.MAUI.Messages;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
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
        private readonly ICurrentFamilyService _currentFamilyService;
        private readonly IUserService _userService;

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

        [ObservableProperty]
        private string _platesCount = "0";

        // Внутренний счётчик
        private int _platesCountInt = 0;

        public bool HasAvatar => !string.IsNullOrEmpty(Avatar);
        public bool HasNoAvatar => string.IsNullOrEmpty(Avatar);

        public ProfileViewModel(
            CurrentUserService currentUserService,
            IAuthService authService,
            ICurrentFamilyService currentFamilyService,
            IUserService userService)
        {
            _currentUserService = currentUserService;
            _authService = authService;
            _currentFamilyService = currentFamilyService;
            _userService = userService;

            _currentUserService.UserChanged += OnUserChanged;
            _currentFamilyService.FamilyChanged += OnFamilyChanged;

            // При каждой отправке тарелки — инкрементируем и сохраняем в БД
            WeakReferenceMessenger.Default.Register<PlateUpdatedMessage>(this, (r, msg) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    if (msg.Action == PlateUpdateAction.Added)
                        _platesCountInt++;
                    else if (msg.Action == PlateUpdateAction.Removed && _platesCountInt > 0)
                        _platesCountInt--;

                    PlatesCount = _platesCountInt.ToString();

                    // Сохраняем актуальное значение в БД
                    var user = _currentUserService.CurrentUser;
                    if (user != null)
                    {
                        user.TotalPlatesCount = _platesCountInt;
                        await _userService.UpdateProfile(user);
                    }
                });
            });

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

        [RelayCommand]
        private async Task OpenSettings()
        {
            if (Application.Current?.MainPage is MainPage mainPage &&
                mainPage.CurrentPage is NavigationPage navPage)
            {
                await navPage.Navigation.PushAsync(App.Services.GetService<SettingsPage>());
            }
        }

        private void OnUserChanged(object sender, UserChangedEventArgs e)
        {
            UpadateUserInfo();
        }

        private void OnFamilyChanged(object sender, FamilyChangedEventArgs e) { }

        private void UpadateUserInfo()
        {
            CheckFirstLastName();
            CreatedAt = _currentUserService.CurrentUser?.CreatedAt ?? DateTime.MinValue;
            DateOfBirth = _currentUserService.CurrentUser?.DateOfBirthday ?? DateTime.MinValue;
            Avatar = _currentUserService.CurrentUser?.Avatar ?? string.Empty;
            AvatarColor = _currentUserService.CurrentUser?.DisplayColor ?? "#1F744D";

            // Восстанавливаем счётчик тарелок из профиля пользователя
            _platesCountInt = _currentUserService.CurrentUser?.TotalPlatesCount ?? 0;
            PlatesCount = _platesCountInt.ToString();
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
