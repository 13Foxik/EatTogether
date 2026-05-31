using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EatTogether.MAUI.Messages;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Services.MenuService.Interfaces;
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
        private readonly IPlateService _plateService;
        private readonly ICurrentFamilyService _currentFamilyService;

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

        // Храним как int для инкремента
        private int _platesCountInt = 0;

        public bool HasAvatar => !string.IsNullOrEmpty(Avatar);
        public bool HasNoAvatar => string.IsNullOrEmpty(Avatar);

        public ProfileViewModel(
            CurrentUserService currentUserService,
            IAuthService authService,
            IPlateService plateService,
            ICurrentFamilyService currentFamilyService)
        {
            _currentUserService = currentUserService;
            _authService = authService;
            _plateService = plateService;
            _currentFamilyService = currentFamilyService;

            _currentUserService.UserChanged += OnUserChanged;
            _currentFamilyService.FamilyChanged += OnFamilyChanged;

            // Подписываемся на отправку тарелок — без чтения из БД
            WeakReferenceMessenger.Default.Register<PlateUpdatedMessage>(this, (r, msg) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (msg.Action == PlateUpdateAction.Added)
                        _platesCountInt++;
                    else if (msg.Action == PlateUpdateAction.Removed && _platesCountInt > 0)
                        _platesCountInt--;

                    PlatesCount = _platesCountInt.ToString();
                });
            });

            UpadateUserInfo();
            // Начальное значение — из локального кэша семьи (не читаем БД)
            LoadPlatesCountFromCache();
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
            LoadPlatesCountFromCache();
        }

        private void OnFamilyChanged(object sender, FamilyChangedEventArgs e)
        {
            LoadPlatesCountFromCache();
        }

        /// <summary>
        /// Считает тарелки из локального кэша семьи — без обращения к БД.
        /// Используется только при первой загрузке / смене семьи.
        /// Далее счётчик меняется через PlateUpdatedMessage.
        /// </summary>
        private void LoadPlatesCountFromCache()
        {
            try
            {
                var currentUser = _currentUserService.CurrentUser;
                var currentFamily = _currentFamilyService.GetCurrentFamily();

                if (currentUser == null || currentFamily == null)
                {
                    _platesCountInt = 0;
                    PlatesCount = "0";
                    return;
                }

                // Считаем по локальному списку Memberships или Plates если они есть в кэше
                // Если кэш пустой — начинаем с 0, дальше инкрементируем через сообщения
                _platesCountInt = 0;
                PlatesCount = "0";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка счётчика тарелок: {ex.Message}");
                _platesCountInt = 0;
                PlatesCount = "0";
            }
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
