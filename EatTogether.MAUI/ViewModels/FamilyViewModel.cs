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

namespace EatTogether.MAUI.ViewModels;

public partial class FamilyViewModel : ObservableObject
{
    private readonly CurrentUserService _currentUserService;
    private readonly CreateFamilyViewModel _createFamilyViewModel;
    private readonly ICurrentFamilyService _currentFamilyService;
    private readonly IMembershipService _membershipService;
    private readonly IFamilyService _familyService;
    private readonly IPlateService _plateService;
    private readonly IDishService _dishService;

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

    // Разделяем тарелки на ожидающие и обработанные
    [ObservableProperty]
    private ObservableCollection<Plate> _pendingPlates = new();

    [ObservableProperty]
    private ObservableCollection<Plate> _processedPlates = new();

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
    private double _currentTabHeight = 400;

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
    IPlateService plateService,
    IDishService dishService)
    {
        _currentUserService = currentUserService;
        _createFamilyViewModel = createFamilyViewModel;
        _currentFamilyService = currentFamilyService;
        _membershipService = membershipService;
        _familyService = familyService;
        _plateService = plateService;
        _dishService = dishService;

        _currentUserService.UserChanged += OnUserChanged;
        _currentFamilyService.FamilyChanged += OnFamilyChanged;

        InitializeTabs();
        CurrentTab = Tabs.FirstOrDefault() ?? Tabs[0];

        // Регистрация сообщений
        WeakReferenceMessenger.Default.Register<PlateUpdatedMessage>(
            this,
            (recipient, message) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadPlates();
                });
            });

        WeakReferenceMessenger.Default.Register<DishStatusUpdatedMessage>(
            this,
            (recipient, message) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Обновляем статус конкретного блюда
                    UpdateDishStatus(message.DishId, message.Status);
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
        Application.Current.Handler.MauiContext.Services.GetService<IPlateService>(),
        Application.Current.Handler.MauiContext.Services.GetService<IDishService>())
    {
    }

    ~FamilyViewModel()
    {
        WeakReferenceMessenger.Default.Unregister<PlateUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<DishStatusUpdatedMessage>(this);
    }

    public void Cleanup()
    {
        WeakReferenceMessenger.Default.Unregister<PlateUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<DishStatusUpdatedMessage>(this);
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
                MembersCount = $"{currentFamily.CountUsers} участников";

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

                        _userNames[member.UserId] = member.DisplayName;
                    }

                    MembersCount = $"{FamilyMembers.Count} участников";
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

            MembersCount = $"{FamilyMembers.Count} участников";
        }
    }

    // ========== МЕТОДЫ ДЛЯ ТАРЕЛОК И БЛЮД ==========

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
                // Очищаем коллекции
                PendingPlates.Clear();
                ProcessedPlates.Clear();

                // Загружаем данные для каждой тарелки
                var platesWithDishes = new List<Plate>();

                foreach (var plate in plates)
                {
                    // Заполняем UI свойства
                    plate.UserName = GetUserName(plate.UserId);
                    plate.UserInitial = GetInitial(plate.UserName);
                    plate.StatusText = GetStatusText(plate.Status);
                    plate.StatusColor = GetStatusColor(plate.Status);
                    plate.IsExpanded = false;

                    // Загружаем блюда для тарелки
                    await LoadDishesForPlate(plate);

                    platesWithDishes.Add(plate);
                }

                // Разделяем тарелки на ожидающие и обработанные
                foreach (var plate in platesWithDishes)
                {
                    if (IsPlateProcessed(plate))
                    {
                        ProcessedPlates.Add(plate);
                    }
                    else
                    {
                        PendingPlates.Add(plate);
                    }
                }

                // Сортируем обработанные тарелки: принятые вверху, отклоненные внизу
                var sortedProcessedPlates = ProcessedPlates
                    .OrderByDescending(p => p.HasAnyAcceptedDish) // Сначала с принятыми блюдами
                    .ThenByDescending(p => p.Status == RequestStatus.Accepted) // Затем принятые тарелки
                    .ThenByDescending(p => p.Id) // По новизне
                    .ToList();

                ProcessedPlates.Clear();
                foreach (var plate in sortedProcessedPlates)
                {
                    ProcessedPlates.Add(plate);
                }
            }
            else
            {
                PendingPlates.Clear();
                ProcessedPlates.Clear();
            }

            HasPlates = PendingPlates.Any() || ProcessedPlates.Any();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке тарелок: {ex.Message}");
            HasPlates = false;
        }
        finally
        {
            IsLoadingPlates = false;

            if (SelectedTabIndex == 0)
            {
                UpdateTabHeight();
            }
        }
    }

    private async Task LoadDishesForPlate(Plate plate)
    {
        try
        {
            var dishesOnPlate = await _plateService.GetDishesOnPlate(plate.Id);
            if (dishesOnPlate != null && dishesOnPlate.Any())
            {
                var dishes = new List<Dish>();

                foreach (var dishOnPlate in dishesOnPlate)
                {
                    var dish = await _dishService.GetDishAsync(dishOnPlate.Id);
                    if (dish != null)
                    {
                        // Устанавливаем правильные свойства для UI
                        dish.dishOnPlateId = dishOnPlate.dishOnPlateId;
                        dish.Status = dishOnPlate.Status;
                        dish.IsInPlate = true;

                        // Устанавливаем визуальные свойства
                        UpdateDishUIProperties(dish);

                        dishes.Add(dish);
                    }
                }

                plate.Dishes = dishes;

                // Проверяем статус тарелки
                UpdatePlateStatusBasedOnDishes(plate);
            }
            else
            {
                plate.Dishes = new List<Dish>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке блюд: {ex.Message}");
            plate.Dishes = new List<Dish>();
        }
    }

    private bool IsPlateProcessed(Plate plate)
    {
        if (plate == null || plate.Dishes == null || !plate.Dishes.Any())
            return false;

        // Тарелка считается обработанной, если все блюда имеют статус Accepted или Rejected
        return plate.Dishes.All(d => d.Status == RequestStatus.Accepted || d.Status == RequestStatus.Rejected);
    }

    private void UpdatePlateStatusBasedOnDishes(Plate plate)
    {
        if (plate == null || plate.Dishes == null || !plate.Dishes.Any())
            return;

        // Проверяем, есть ли хотя бы одно принятое блюдо
        bool hasAcceptedDish = plate.Dishes.Any(d => d.Status == RequestStatus.Accepted);
        bool hasRejectedDish = plate.Dishes.Any(d => d.Status == RequestStatus.Rejected);
        bool allProcessed = plate.Dishes.All(d => d.Status != RequestStatus.Pending);

        if (allProcessed)
        {
            // Если все блюда обработаны
            if (hasAcceptedDish)
            {
                plate.Status = RequestStatus.Accepted;
                plate.HasAnyAcceptedDish = true;
            }
            else if (hasRejectedDish)
            {
                plate.Status = RequestStatus.Rejected;
                plate.HasAnyAcceptedDish = false;
            }
        }
        else
        {
            // Если есть ожидающие блюда
            plate.Status = RequestStatus.Pending;
            plate.HasAnyAcceptedDish = false;
        }

        // Обновляем UI свойства тарелки
        plate.StatusText = GetStatusText(plate.Status);
        plate.StatusColor = GetStatusColor(plate.Status);
        plate.IsProcessed = allProcessed;
        plate.CanShowActions = !allProcessed;
    }

    [RelayCommand]
    private async void AcceptDish(Dish dish)
    {
        if (dish == null || dish.Status != RequestStatus.Pending) return;

        try
        {
            // Обновляем статус в UI
            dish.Status = RequestStatus.Accepted;
            UpdateDishUIProperties(dish);

            // Отправляем запрос на сервер
            await _dishService.EditDishStatus(dish.dishOnPlateId, dish.Status);

            // Находим родительскую тарелку
            var plate = FindPlateForDish(dish);
            if (plate != null)
            {
                UpdatePlateStatusBasedOnDishes(plate);

                // Если тарелка теперь обработана, перемещаем ее
                if (plate.IsProcessed && PendingPlates.Contains(plate))
                {
                    PendingPlates.Remove(plate);
                    ProcessedPlates.Insert(0, plate); // Добавляем в начало обработанных

                    // Сортируем обработанные тарелки
                    SortProcessedPlates();
                }
            }

            // Отправляем сообщение об обновлении
            WeakReferenceMessenger.Default.Send(new DishStatusUpdatedMessage(dish.Id, dish.Status));

            System.Diagnostics.Debug.WriteLine($"Блюдо {dish.Name} принято");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при принятии блюда: {ex.Message}");

            // Откатываем изменения в случае ошибки
            dish.Status = RequestStatus.Pending;
            UpdateDishUIProperties(dish);
        }
    }

    [RelayCommand]
    private async void RejectDish(Dish dish)
    {
        if (dish == null || dish.Status != RequestStatus.Pending) return;

        try
        {
            // Обновляем статус в UI
            dish.Status = RequestStatus.Rejected;
            UpdateDishUIProperties(dish);

            // Отправляем запрос на сервер
            await _dishService.EditDishStatus(dish.dishOnPlateId, dish.Status);

            // Находим родительскую тарелку
            var plate = FindPlateForDish(dish);
            if (plate != null)
            {
                UpdatePlateStatusBasedOnDishes(plate);

                // Если тарелка теперь обработана, перемещаем ее
                if (plate.IsProcessed && PendingPlates.Contains(plate))
                {
                    PendingPlates.Remove(plate);
                    ProcessedPlates.Add(plate);

                    // Сортируем обработанные тарелки
                    SortProcessedPlates();
                }
            }

            // Отправляем сообщение об обновлении
            WeakReferenceMessenger.Default.Send(new DishStatusUpdatedMessage(dish.Id, dish.Status));

            System.Diagnostics.Debug.WriteLine($"Блюдо {dish.Name} отклонено");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при отклонении блюда: {ex.Message}");

            // Откатываем изменения в случае ошибки
            dish.Status = RequestStatus.Pending;
            UpdateDishUIProperties(dish);
        }
    }

    private Plate FindPlateForDish(Dish dish)
    {
        // Ищем в ожидающих тарелках
        var plate = PendingPlates.FirstOrDefault(p => p.Dishes?.Any(d => d.Id == dish.Id) == true);
        if (plate != null) return plate;

        // Ищем в обработанных тарелках
        return ProcessedPlates.FirstOrDefault(p => p.Dishes?.Any(d => d.Id == dish.Id) == true);
    }

    private void SortProcessedPlates()
    {
        var sortedProcessedPlates = ProcessedPlates
            .OrderByDescending(p => p.HasAnyAcceptedDish) // Сначала с принятыми блюдами
            .ThenByDescending(p => p.Status == RequestStatus.Accepted) // Затем принятые тарелки
            .ThenByDescending(p => p.Id) // По новизне
            .ToList();

        ProcessedPlates.Clear();
        foreach (var plate in sortedProcessedPlates)
        {
            ProcessedPlates.Add(plate);
        }
    }

    private void UpdateDishUIProperties(Dish dish)
    {
        dish.StatusText = GetStatusText(dish.Status);
        dish.StatusColor = GetStatusColor(dish.Status);
        dish.ButtonBackgroundColor = GetButtonBackgroundColor(dish.Status);
        dish.ButtonTextColor = GetButtonTextColor(dish.Status);
        dish.IsStatusVisible = true;
        dish.CanShowActions = dish.Status == RequestStatus.Pending;
    }

    private void UpdateDishStatus(string dishId, RequestStatus status)
    {
        // Ищем блюдо во всех тарелках
        foreach (var plate in PendingPlates.Concat(ProcessedPlates))
        {
            var dish = plate.Dishes?.FirstOrDefault(d => d.Id == dishId);
            if (dish != null)
            {
                dish.Status = status;
                UpdateDishUIProperties(dish);
                UpdatePlateStatusBasedOnDishes(plate);
                break;
            }
        }
    }

    [RelayCommand]
    private async void TogglePlate(string plateId)
    {
        // Ищем в ожидающих тарелках
        var plate = PendingPlates.FirstOrDefault(p => p.Id == plateId);
        if (plate == null)
        {
            // Ищем в обработанных тарелках
            plate = ProcessedPlates.FirstOrDefault(p => p.Id == plateId);
        }

        if (plate != null)
        {
            plate.IsExpanded = !plate.IsExpanded;

            if (plate.IsExpanded && (!plate.Dishes.Any() || plate.Dishes == null))
            {
                await LoadDishesForPlate(plate);
            }

            UpdateTabHeight();
        }
    }

    [RelayCommand]
    private async void AcceptPlate(string plateId)
    {
        var plate = PendingPlates.FirstOrDefault(p => p.Id == plateId);
        if (plate != null)
        {
            try
            {
                // Принимаем все блюда в тарелке
                foreach (var dish in plate.Dishes.Where(d => d.Status == RequestStatus.Pending))
                {
                    dish.Status = RequestStatus.Accepted;
                    UpdateDishUIProperties(dish);
                    await _dishService.EditDishStatus(dish.dishOnPlateId, dish.Status);
                }

                UpdatePlateStatusBasedOnDishes(plate);

                // Перемещаем тарелку в обработанные
                if (plate.IsProcessed)
                {
                    PendingPlates.Remove(plate);
                    ProcessedPlates.Insert(0, plate);
                    SortProcessedPlates();
                }

                System.Diagnostics.Debug.WriteLine($"Тарелка {plateId} принята полностью");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при принятии тарелки: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async void RejectPlate(string plateId)
    {
        var plate = PendingPlates.FirstOrDefault(p => p.Id == plateId);
        if (plate != null)
        {
            try
            {
                // Отклоняем все блюда в тарелке
                foreach (var dish in plate.Dishes.Where(d => d.Status == RequestStatus.Pending))
                {
                    dish.Status = RequestStatus.Rejected;
                    UpdateDishUIProperties(dish);
                    await _dishService.EditDishStatus(dish.dishOnPlateId, dish.Status);
                }

                UpdatePlateStatusBasedOnDishes(plate);

                // Перемещаем тарелку в обработанные
                if (plate.IsProcessed)
                {
                    PendingPlates.Remove(plate);
                    ProcessedPlates.Add(plate);
                    SortProcessedPlates();
                }

                System.Diagnostics.Debug.WriteLine($"Тарелка {plateId} отклонена полностью");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при отклонении тарелки: {ex.Message}");
            }
        }
    }

    // Вспомогательные методы
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

    private Color GetButtonBackgroundColor(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Pending => Colors.Transparent,
            RequestStatus.Accepted => Color.FromArgb("#E8F5E9"), // Light Green
            RequestStatus.Rejected => Color.FromArgb("#FFEBEE"), // Light Red
            _ => Colors.Transparent
        };
    }

    private Color GetButtonTextColor(RequestStatus status)
    {
        return status switch
        {
            RequestStatus.Pending => Color.FromArgb("#FFA500"), // Orange
            RequestStatus.Accepted => Color.FromArgb("#2E7D32"), // Dark Green
            RequestStatus.Rejected => Color.FromArgb("#C62828"), // Dark Red
            _ => Colors.Gray
        };
    }

    // ========== КОМАНДЫ ДЛЯ ВКЛАДОК С ДИНАМИЧЕСКОЙ ВЫСОТОЙ ==========

    [RelayCommand]
    private async void SelectTab(object parameter)
    {
        if (parameter is string indexStr && int.TryParse(indexStr, out int index) && index >= 0 && index < Tabs.Count)
        {
            SelectedTabIndex = index;
            CurrentTab = Tabs[index];

            await LoadTabDataAsync(index);
            UpdateTabHeight();
        }
    }

    private async Task LoadTabDataAsync(int tabIndex)
    {
        switch (tabIndex)
        {
            case 0: // Активность
                if (!PendingPlates.Any() && !ProcessedPlates.Any())
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

                int pendingPlateCount = PendingPlates?.Count ?? 0;
                int processedPlateCount = ProcessedPlates?.Count ?? 0;

                int expandedPendingCount = PendingPlates?.Count(p => p.IsExpanded) ?? 0;
                int expandedProcessedCount = ProcessedPlates?.Count(p => p.IsExpanded) ?? 0;

                int dishCount = (PendingPlates?.Sum(p => p.Dishes?.Count ?? 0) ?? 0) +
                              (ProcessedPlates?.Sum(p => p.Dishes?.Count ?? 0) ?? 0);

                // Базовая высота: заголовки + отступы
                double height = 300; // Базовый отступ

                // Заголовок для ожидающих тарелок
                if (pendingPlateCount > 0) height += 50;

                // Ожидающие тарелки без раскрытия (~100px)
                height += (pendingPlateCount - expandedPendingCount) * 100;

                // Раскрытые ожидающие тарелки (~250px + блюда)
                height += expandedPendingCount * 250;

                // Заголовок для обработанных тарелок
                if (processedPlateCount > 0) height += 50;

                // Обработанные тарелки без раскрытия (~80px)
                height += (processedPlateCount - expandedProcessedCount) * 80;

                // Раскрытые обработанные тарелки (~200px + блюда)
                height += expandedProcessedCount * 200;

                // Блюда (по 50px)
                height += dishCount * 50;

                return Math.Min(height, 1000); // Максимум 1000px

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
            MembersCount = $"{FamilyMembers.Count} участников";

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

        if (CurrentTab?.Type == TabType.Requests)
        {
            UpdateTabHeight();
        }
    }

    partial void OnPendingPlatesChanged(ObservableCollection<Plate> value)
    {
        HasPlates = (PendingPlates?.Any() == true) || (ProcessedPlates?.Any() == true);

        if (CurrentTab?.Type == TabType.Activity)
        {
            UpdateTabHeight();
        }
    }

    partial void OnProcessedPlatesChanged(ObservableCollection<Plate> value)
    {
        HasPlates = (PendingPlates?.Any() == true) || (ProcessedPlates?.Any() == true);

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

public class DishStatusUpdatedMessage
{
    public string DishId { get; }
    public RequestStatus Status { get; }

    public DishStatusUpdatedMessage(string dishId, RequestStatus status)
    {
        DishId = dishId;
        Status = status;
    }
}