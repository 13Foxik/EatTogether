using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.MenuService.Interfaces;
using EatTogether.MAUI.Views.Main;
using EatTogether.MAUI.Views.Main.FamilyPages;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using EatTogether.MAUI.Messages;
using System.Numerics;

namespace EatTogether.MAUI.ViewModels;

public partial class FamilyViewModel : ObservableObject
{
    private readonly CurrentUserService _currentUserService;
    private readonly CreateFamilyViewModel _createFamilyViewModel;
    private readonly ICurrentFamilyService _currentFamilyService;
    private readonly IMembershipService _membershipService;
    private readonly IFamilyService _familyService;
    private readonly IPlateService _plateService;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private TabItem _currentTab;

    [ObservableProperty]
    private bool _hasFamily;

    [ObservableProperty]
    private double _scrollPosition;

    [ObservableProperty]
    private bool _isScrolledDown;

    [ObservableProperty]
    private ObservableCollection<MembershipRequest> _pendingRequests = new();

    [ObservableProperty]
    private bool _hasPendingRequests;

    [ObservableProperty]
    private ObservableCollection<FamilyMember> _familyMembers = new();

    // Для тарелок
    [ObservableProperty]
    private ObservableCollection<Plate> _plates = new();

    [ObservableProperty]
    private bool _hasPlates;

    [ObservableProperty]
    private bool _isLoadingPlates;

    [ObservableProperty]
    private FamilyMember _currentUserMember;

    [ObservableProperty]
    private string _membersCount;

    [ObservableProperty]
    private string _familyName;

    [ObservableProperty]
    private string _familyDescription;

    [ObservableProperty]
    private string _familyAvatar;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private double _currentTabHeight = 400; // НОВОЕ: высота текущей вкладки

    // Словарь для имен пользователей
    private Dictionary<string, string> _userNames = new();

    // Коллекция вкладок с настройкой высоты
    private ObservableCollection<TabItem> _tabs = new();
    public ObservableCollection<TabItem> Tabs
    {
        get => _tabs;
        private set => SetProperty(ref _tabs, value);
    }

    public FamilyViewModel(
    CurrentUserService currentUserService,
    CreateFamilyViewModel createFamilyViewModel,
    ICurrentFamilyService currentFamilyService,
    IMembershipService membershipService,
    IFamilyService familyService,
    IPlateService plateService)
    {
        _currentUserService = currentUserService;
        _createFamilyViewModel = createFamilyViewModel;
        _currentFamilyService = currentFamilyService;
        _membershipService = membershipService;
        _familyService = familyService;
        _plateService = plateService;

        _currentUserService.UserChanged += OnUserChanged;
        _currentFamilyService.FamilyChanged += OnFamilyChanged;

        // Инициализация вкладок с разной начальной высотой
        InitializeTabs();

        CurrentTab = Tabs.FirstOrDefault() ?? Tabs[0];

        // РЕГИСТРАЦИЯ СООБЩЕНИЯ ДЛЯ ОБНОВЛЕНИЯ ТАРЕЛОК
        WeakReferenceMessenger.Default.Register<PlateUpdatedMessage>(
            this,
            (recipient, message) =>
            {
                // Обновляем в основном потоке UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadPlates(); // Обновляем список тарелок
                });
            });

        LoadFamilyData();
        LoadPendingRequests();
    }

    public FamilyViewModel() : this(
        Application.Current.Handler.MauiContext.Services.GetService<CurrentUserService>(),
        Application.Current.Handler.MauiContext.Services.GetService<CreateFamilyViewModel>(),
        Application.Current.Handler.MauiContext.Services.GetService<ICurrentFamilyService>(),
        Application.Current.Handler.MauiContext.Services.GetService<IMembershipService>(),
        Application.Current.Handler.MauiContext.Services.GetService<IFamilyService>(),
        Application.Current.Handler.MauiContext.Services.GetService<IPlateService>())
    {
    }
    ~FamilyViewModel()
    {
        // Отписываемся от сообщений при уничтожении
        WeakReferenceMessenger.Default.Unregister<PlateUpdatedMessage>(this);
    }

    // Или добавьте метод для очистки
    public void Cleanup()
    {
        WeakReferenceMessenger.Default.Unregister<PlateUpdatedMessage>(this);
    }


    private void InitializeTabs()
    {
        Tabs = new ObservableCollection<TabItem>
        {
            new TabItem { Type = TabType.Activity, Height = 400 },
            new TabItem { Type = TabType.Members, Height = 300 },
            new TabItem { Type = TabType.Requests, Height = 350 }
        };
    }

    private void OnFamilyChanged(object sender, FamilyChangedEventArgs e)
    {
        LoadFamilyData();
        LoadPlates();
    }

    private void OnUserChanged(object sender, UserChangedEventArgs e)
    {
        UpdateUserInfo();
        LoadPendingRequests();
    }

    private void UpdateUserInfo()
    {
        HasFamily = _currentUserService.CurrentUser?.UserFamilies?.Count > 0;
    }

    private async void LoadFamilyData()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            FamilyMembers.Clear();

            Family? currentFamily = _currentFamilyService?.GetCurrentFamily();

            if (currentFamily != null)
            {
                FamilyName = currentFamily.Name ?? "Моя семья";
                FamilyDescription = currentFamily.Description ?? "Описание семьи пока не добавлено";
                MembersCount = $"{currentFamily.CountUsers} учатников";

                if (currentFamily.Members != null && currentFamily.Members.Any())
                {
                    var currentUserId = _currentUserService.CurrentUser?.Uid;

                    foreach (var member in currentFamily.Members)
                    {
                        member.IsCurrentUser = member.UserId == currentUserId;
                        FamilyMembers.Add(member);

                        if (member.IsCurrentUser)
                        {
                            CurrentUserMember = member;
                        }

                        // Сохраняем имя пользователя
                        _userNames[member.UserId] = member.DisplayName;
                    }

                    MembersCount = $"{FamilyMembers.Count} Участников";
                }
                else
                {
                    await LoadMembersFromMemberships(currentFamily);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке данных семьи: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadMembersFromMemberships(Family currentFamily)
    {
        if (currentFamily.Memberships != null)
        {
            var acceptedMembers = currentFamily.Memberships
                .Where(m => m.Status == RequestStatus.Accepted)
                .ToList();

            var currentUserId = _currentUserService.CurrentUser?.Uid;

            foreach (var membership in acceptedMembers)
            {
                var member = new FamilyMember
                {
                    UserId = membership.UserId,
                    DisplayName = membership.UserDisplayName ?? "Участник",
                    Email = "",
                    AvatarUrl = membership.UserAvatarUrl,
                    Role = FamilyRole.Member,
                    JoinedAt = membership.CreatedAt,
                    IsCurrentUser = membership.UserId == currentUserId
                };

                FamilyMembers.Add(member);

                if (member.IsCurrentUser)
                {
                    CurrentUserMember = member;
                }

                _userNames[member.UserId] = member.DisplayName;
            }

            MembersCount = $"{FamilyMembers.Count} Участников";
        }
    }

    // ========== МЕТОДЫ ДЛЯ ТАРЕЛОК ==========

    [RelayCommand]
    private async void LoadPlates()
    {
        if (IsLoadingPlates) return;

        try
        {
            IsLoadingPlates = true;

            var currentFamily = _currentFamilyService?.GetCurrentFamily();
            if (currentFamily == null)
            {
                HasPlates = false;
                IsLoadingPlates = false;
                return;
            }

            var plates = await _plateService.GetFamilyPlates(currentFamily.Id);

            if (plates != null && plates.Any())
            {
                // Создаем новую коллекцию для избежания проблем с потоками
                var newPlates = new ObservableCollection<Plate>();

                foreach (var plate in plates)
                {
                    // Заполняем UI свойства
                    plate.UserName = GetUserName(plate.UserId);
                    plate.UserInitial = GetInitial(plate.UserName);
                    plate.StatusText = GetStatusText(plate.Status);
                    plate.StatusColor = GetStatusColor(plate.Status);
                    plate.IsExpanded = false;

                    newPlates.Add(plate);
                }

                // Сортируем по ID (новые первые)
                var sortedPlates = newPlates.OrderByDescending(p => p.Id).ToList();

                // Обновляем коллекцию
                Plates.Clear();
                foreach (var plate in sortedPlates)
                {
                    Plates.Add(plate);
                }
            }
            else
            {
                // Если тарелок нет, очищаем коллекцию
                Plates.Clear();
            }

            HasPlates = Plates.Any();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке тарелок: {ex.Message}");
            HasPlates = false;
        }
        finally
        {
            IsLoadingPlates = false;

            // Пересчитываем высоту для вкладки Активность
            if (SelectedTabIndex == 0)
            {
                UpdateTabHeight();
            }
        }
    }

    [RelayCommand]
    private async void TogglePlate(string plateId)
    {
        var plate = Plates.FirstOrDefault(p => p.Id == plateId);
        if (plate != null)
        {
            plate.IsExpanded = !plate.IsExpanded;

            // Если раскрыли и еще не загрузили блюда - загружаем
            if (plate.IsExpanded && (!plate.Dishes.Any() || plate.Dishes == null))
            {
                await LoadDishesForPlate(plate);
            }

            // Пересчитываем высоту вкладки
            UpdateTabHeight();
        }
    }

    private async Task LoadDishesForPlate(Plate plate)
    {
        try
        {
            var dishes = await _plateService.GetDishesOnPlate(plate.Id);
            if (dishes != null)
            {
                plate.Dishes = new List<Dish>(dishes);

                // Уведомляем об изменении
                OnPropertyChanged(nameof(Plates));

                // Пересчитываем высоту
                UpdateTabHeight();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке блюд: {ex.Message}");
        }
    }

    // Вспомогательные методы (только для ViewModel)
    private string GetUserName(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return "Участник";

        if (_userNames.ContainsKey(userId))
            return _userNames[userId];

        var member = FamilyMembers.FirstOrDefault(m => m.UserId == userId);
        if (member != null)
        {
            _userNames[userId] = member.DisplayName;
            return member.DisplayName;
        }

        return "Участник";
    }

    private string GetInitial(string name)
    {
        if (string.IsNullOrEmpty(name)) return "?";
        return name.Substring(0, 1).ToUpper();
    }

    private string GetStatusText(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Pending => "Ожидает",
            RequestStatus.Accepted => "Принята",
            RequestStatus.Rejected => "Отклонена",
            _ => "Неизвестно"
        };
    }

    private Color GetStatusColor(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Pending => Color.FromArgb("#FFA500"), // Orange
            RequestStatus.Accepted => Color.FromArgb("#4CAF50"), // Green
            RequestStatus.Rejected => Color.FromArgb("#F44336"), // Red
            _ => Colors.Gray
        };
    }

    // Команды для блюд (заглушки)
    [RelayCommand]
    private void AcceptDish(string parameters)
    {
        System.Diagnostics.Debug.WriteLine($"Блюдо принято: {parameters}");
    }

    [RelayCommand]
    private void RejectDish(string parameters)
    {
        System.Diagnostics.Debug.WriteLine($"Блюдо отклонено: {parameters}");
    }

    // Команды для тарелок
    [RelayCommand]
    private void AcceptPlate(string plateId)
    {
        var plate = Plates.FirstOrDefault(p => p.Id == plateId);
        if (plate != null)
        {
            plate.Status = RequestStatus.Accepted;
            plate.StatusText = "Принята";
            plate.StatusColor = Color.FromArgb("#4CAF50");

            System.Diagnostics.Debug.WriteLine($"Тарелка {plateId} принята");
        }
    }

    [RelayCommand]
    private void RejectPlate(string plateId)
    {
        var plate = Plates.FirstOrDefault(p => p.Id == plateId);
        if (plate != null)
        {
            plate.Status = RequestStatus.Rejected;
            plate.StatusText = "Отклонена";
            plate.StatusColor = Color.FromArgb("#F44336");

            System.Diagnostics.Debug.WriteLine($"Тарелка {plateId} отклонена");
        }
    }

    // ========== КОМАНДЫ ДЛЯ ВКЛАДОК С ДИНАМИЧЕСКОЙ ВЫСОТОЙ ==========

    [RelayCommand]
    private async void SelectTab(object parameter)
    {
        if (parameter is string indexStr && int.TryParse(indexStr, out int index) && index >= 0 && index < Tabs.Count)
        {
            SelectedTabIndex = index;
            CurrentTab = Tabs[index];

            // Загружаем данные для вкладки
            await LoadTabDataAsync(index);

            // Устанавливаем высоту для новой вкладки
            UpdateTabHeight();
        }
    }

    private async Task LoadTabDataAsync(int tabIndex)
    {
        switch (tabIndex)
        {
            case 0: // Активность
                if (Plates == null || !Plates.Any())
                {
                    LoadPlates();
                }
                break;

            case 1: // Участники
                if (FamilyMembers == null || !FamilyMembers.Any())
                {
                    await LoadFamilyMembersAsync();
                }
                break;

            case 2: // Запросы
                if (PendingRequests == null || !PendingRequests.Any())
                {
                    LoadPendingRequests();
                }
                break;
        }
    }

    private async Task LoadFamilyMembersAsync()
    {
        IsLoading = true;
        await Task.Delay(800);
        IsLoading = false;
    }

    // Метод для расчета высоты текущей вкладки
    private void UpdateTabHeight()
    {
        if (CurrentTab == null) return;

        double newHeight = CalculateTabHeight(CurrentTab.Type);
        CurrentTab.Height = newHeight;
        CurrentTabHeight = newHeight;
    }

    private double CalculateTabHeight(TabType tabType)
    {
        switch (tabType)
        {
            case TabType.Activity:
                if (!HasPlates) return 300;

                int plateCount = Plates?.Count ?? 0;
                int expandedCount = Plates?.Count(p => p.IsExpanded) ?? 0;
                int dishCount = Plates?.Sum(p => p.Dishes?.Count ?? 0) ?? 0;

                // Базовая высота: заголовок + подсказка + отступы
                double height = 200;

                // Тарелки без раскрытия (~100px)
                height += (plateCount - expandedCount) * 100;

                // Раскрытые тарелки (~250px + блюда)
                height += expandedCount * 250;
                height += dishCount * 50;

                return Math.Min(height, 800); // Максимум 800px

            case TabType.Members:
                if (FamilyMembers == null || !FamilyMembers.Any()) return 200;

                // ~80px на участника + заголовок
                double membersHeight = 100 + (FamilyMembers.Count * 80);
                return Math.Min(membersHeight, 500);

            case TabType.Requests:
                if (!HasPendingRequests) return 250;

                // ~140px на запрос + история
                double requestsHeight = 100 + (PendingRequests.Count * 140);
                return Math.Min(requestsHeight, 500);

            default:
                return 400;
        }
    }

    [RelayCommand]
    private void SwipeLeft()
    {
        if (IsScrolledDown && SelectedTabIndex < Tabs.Count - 1)
        {
            SelectedTabIndex++;
            CurrentTab = Tabs[SelectedTabIndex];
            UpdateTabHeight();
        }
    }

    [RelayCommand]
    private void SwipeRight()
    {
        if (IsScrolledDown && SelectedTabIndex > 0)
        {
            SelectedTabIndex--;
            CurrentTab = Tabs[SelectedTabIndex];
            UpdateTabHeight();
        }
    }

    [RelayCommand]
    private void ScrollPositionChanged(object parameter)
    {
        if (parameter is double position)
        {
            ScrollPosition = position;
            IsScrolledDown = position > 50;
        }
    }

    [RelayCommand]
    private async void CreateFamily()
    {
        if (Application.Current?.MainPage is MainPage mainPage)
        {
            var currentNavigation = mainPage.CurrentPage as NavigationPage;
            if (currentNavigation != null)
            {
                await currentNavigation.Navigation.PushAsync(new CreateFamilyPage(_createFamilyViewModel));
            }
        }
    }

    [RelayCommand]
    private async void JoinFamily()
    {
        if (Application.Current?.MainPage is MainPage mainPage)
        {
            var currentNavigation = mainPage.CurrentPage as NavigationPage;
            if (currentNavigation != null)
            {
                await currentNavigation.Navigation.PushAsync(new JoinFamilyPage());
            }
        }
    }

    [RelayCommand]
    private async void ViewMemberProfile(FamilyMember member)
    {
        if (member == null) return;

        try
        {
            await Shell.Current.DisplayAlert("Профиль",
                $"Профиль участника: {member.DisplayName}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка",
                $"Не удалось открыть профиль: {ex.Message}", "OK");
        }
    }

    // Команды для работы с запросами
    [RelayCommand]
    private async void AcceptRequest(MembershipRequest request)
    {
        if (request == null) return;

        try
        {
            var newMember = new FamilyMember
            {
                UserId = request.UserId,
                DisplayName = request.UserDisplayName,
                Email = "",
                AvatarUrl = request.UserAvatarUrl,
                Role = FamilyRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            await _familyService.AcceptMember(request);
            await _membershipService.UpdateRequestStatus(request, RequestStatus.Accepted);

            request.Status = RequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;
            request.RespondedBy = _currentUserService.CurrentUser?.Uid;

            PendingRequests.Remove(request);

            FamilyMembers.Add(newMember);
            MembersCount = $"{FamilyMembers.Count} Участников";

            _userNames[request.UserId] = request.UserDisplayName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при принятии запроса: {ex.Message}");
        }
    }

    [RelayCommand]
    private async void RejectRequest(MembershipRequest request)
    {
        if (request == null) return;

        try
        {
            await _membershipService.UpdateRequestStatus(request, RequestStatus.Rejected);

            request.Status = RequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;
            request.RespondedBy = _currentUserService.CurrentUser?.Uid;

            PendingRequests.Remove(request);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при отклонении запроса: {ex.Message}");
        }
    }

    private void LoadPendingRequests()
    {
        try
        {
            PendingRequests.Clear();

            Family? currentFamily = _currentFamilyService?.GetCurrentFamily();

            if (currentFamily?.Memberships != null)
            {
                var pending = currentFamily.Memberships
                    .Where(r => r.Status == RequestStatus.Pending)
                    .ToList();

                foreach (var request in pending)
                {
                    PendingRequests.Add(request);
                }
            }

            HasPendingRequests = PendingRequests.Any();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке запросов: {ex.Message}");
        }
    }

    // Частичные методы
    partial void OnCurrentTabChanged(TabItem value)
    {
        if (value != null)
        {
            SelectedTabIndex = Tabs.IndexOf(value);

            if (value.Type == TabType.Members)
            {
                LoadFamilyData();
            }
            else if (value.Type == TabType.Requests)
            {
                LoadPendingRequests();
            }
            else if (value.Type == TabType.Activity)
            {
                LoadPlates();
            }

            // Устанавливаем высоту для новой вкладки
            CurrentTabHeight = value.Height;
        }
    }

    partial void OnIsScrolledDownChanged(bool value)
    {
        if (App.Current?.MainPage is MainPage mainPage)
        {
            mainPage.SetSwipeEnabled(!value);
        }
    }

    partial void OnPendingRequestsChanged(ObservableCollection<MembershipRequest> value)
    {
        HasPendingRequests = value?.Any() == true;

        // Пересчитываем высоту если это активная вкладка
        if (CurrentTab?.Type == TabType.Requests)
        {
            UpdateTabHeight();
        }
    }

    partial void OnPlatesChanged(ObservableCollection<Plate> value)
    {
        HasPlates = value?.Any() == true;

        // Пересчитываем высоту если это активная вкладка
        if (CurrentTab?.Type == TabType.Activity)
        {
            UpdateTabHeight();
        }
    }

    public void RefreshFamilyData()
    {
        LoadFamilyData();
        LoadPendingRequests();
        LoadPlates();
    }
}

public class TabItem : ObservableObject
{
    public TabType Type { get; set; }
    public bool IsActivity => Type == TabType.Activity;
    public bool IsMembers => Type == TabType.Members;
    public bool IsRequests => Type == TabType.Requests;

    private double _height = 400;
    public double Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }
}

public enum TabType
{
    Activity,
    Members,
    Requests
}