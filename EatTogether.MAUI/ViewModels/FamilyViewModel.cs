using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.MenuService.Interfaces;
using EatTogether.MAUI.Views.Main;
using EatTogether.MAUI.Views.Main.FamilyPages;using System.Collections.ObjectModel;
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
    private readonly IFamilyMemberControlService _memberControlService;

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
    private string _familyId;

    [ObservableProperty]
    private string _familyAvatar;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isLoadingMembers;

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
        IDishService dishService,
        IFamilyMemberControlService memberControlService)
    {
        _currentUserService = currentUserService;
        _createFamilyViewModel = createFamilyViewModel;
        _currentFamilyService = currentFamilyService;
        _membershipService = membershipService;
        _familyService = familyService;
        _plateService = plateService;
        _dishService = dishService;
        _memberControlService = memberControlService;

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
                    UpdateDishStatus(message.DishOnPlateId, message.Status);
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
        Application.Current.Handler.MauiContext.Services.GetService<IDishService>(),
        Application.Current.Handler.MauiContext.Services.GetService<IFamilyMemberControlService>())
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
        // Обновляем аватар текущего пользователя в списке участников
        RefreshCurrentUserAvatar();
    }

    private void RefreshCurrentUserAvatar()
    {
        var currentUser = _currentUserService.CurrentUser;
        if (currentUser == null) return;

        var member = FamilyMembers.FirstOrDefault(m => m.UserId == currentUser.Uid);
        if (member != null)
        {
            member.AvatarUrl = currentUser.Avatar ?? string.Empty;
            member.AvatarColor = currentUser.AvatarColor ?? "#1F744D";
            // Обновляем ссылку чтобы триггернуть UI
            var idx = FamilyMembers.IndexOf(member);
            if (idx >= 0)
            {
                FamilyMembers[idx] = member;
            }
        }
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
                FamilyId = currentFamily.Id ?? "";
                MembersCount = $"{currentFamily.CountUsers} участников";

                if (currentFamily.Members != null && currentFamily.Members.Any())
                {
                    var currentUserId = _currentUserService.CurrentUser?.Uid;

                    foreach (var member in currentFamily.Members)
                    {
                        member.IsCurrentUser = member.UserId == currentUserId;

                        // Устанавливаем текст роли
                        member.RoleText = _memberControlService.GetRoleText(member.Role);
                        member.RoleColor = _memberControlService.GetRoleColor(member.Role);

                        FamilyMembers.Add(member);

                        if (member.IsCurrentUser)
                        {
                            CurrentUserMember = member;
                        }

                        _userNames[member.UserId] = member.DisplayName;
                    }

                    MembersCount = $"{FamilyMembers.Count} участников";

                    // Загружаем права для каждого участника
                    await LoadMemberPermissions();
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

            // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ ПОСЛЕ ЗАГРУЗКИ ДАННЫХ
            if (CurrentTab?.Type == TabType.Members)
            {
                UpdateTabHeight();
            }
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
                    RoleText = _memberControlService.GetRoleText(FamilyRole.Member),
                    RoleColor = _memberControlService.GetRoleColor(FamilyRole.Member),
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

            // Загружаем права для каждого участника
            await LoadMemberPermissions();
        }
    }

    private async Task LoadMemberPermissions()
    {
        if (FamilyMembers == null || !FamilyMembers.Any())
            return;

        foreach (var member in FamilyMembers)
        {
            try
            {
                member.CanPromote = await _memberControlService.CanPromote(member);
                member.CanDemote = await _memberControlService.CanDemote(member);
                member.CanKick = await _memberControlService.CanKick(member);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке прав для участника {member.UserId}: {ex.Message}");
                member.CanPromote = false;
                member.CanDemote = false;
                member.CanKick = false;
            }
        }

        // ОБНОВЛЯЕМ ВЫСОТУ ПОСЛЕ ЗАГРУЗКИ ПРАВ
        if (CurrentTab?.Type == TabType.Members)
        {
            UpdateTabHeight();
        }
    }

    [RelayCommand]
    private async Task PromoteMember(FamilyMember member)
    {
        if (member == null) return;

        try
        {
            var currentFamily = _currentFamilyService?.GetCurrentFamily();
            if (currentFamily == null) return;

            bool result = await _memberControlService.Promote(member.UserId, currentFamily.Id);

            if (result)
            {
                // Обновляем роль в локальных данных
                var updatedMember = FamilyMembers.FirstOrDefault(m => m.UserId == member.UserId);
                if (updatedMember != null && updatedMember.Role < FamilyRole.Admin)
                {
                    updatedMember.Role = updatedMember.Role + 1;
                    updatedMember.RoleText = _memberControlService.GetRoleText(updatedMember.Role);
                    updatedMember.RoleColor = _memberControlService.GetRoleColor(updatedMember.Role);

                    // Обновляем права для всех участников
                    await LoadMemberPermissions();

                    await Shell.Current.DisplayAlert("Успех",
                        $"{member.DisplayName} повышен в роли", "OK");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка",
                    "Не удалось повысить участника. Проверьте ваши права.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка",
                $"Ошибка при повышении участника: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task DemoteMember(FamilyMember member)
    {
        if (member == null) return;

        try
        {
            var currentFamily = _currentFamilyService?.GetCurrentFamily();
            if (currentFamily == null) return;

            bool result = await _memberControlService.Demote(member.UserId, currentFamily.Id);

            if (result)
            {
                // Обновляем роль в локальных данных
                var updatedMember = FamilyMembers.FirstOrDefault(m => m.UserId == member.UserId);
                if (updatedMember != null && updatedMember.Role > FamilyRole.Member)
                {
                    updatedMember.Role = updatedMember.Role - 1;
                    updatedMember.RoleText = _memberControlService.GetRoleText(updatedMember.Role);
                    updatedMember.RoleColor = _memberControlService.GetRoleColor(updatedMember.Role);

                    // Обновляем права для всех участников
                    await LoadMemberPermissions();

                    await Shell.Current.DisplayAlert("Успех",
                        $"{member.DisplayName} понижен в роли", "OK");
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка",
                    "Не удалось понизить участника. Проверьте ваши права.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка",
                $"Ошибка при понижении участника: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task KickMember(FamilyMember member)
    {
        if (member == null) return;

        try
        {
            bool confirm = await Shell.Current.DisplayAlert("Подтверждение",
                $"Вы уверены, что хотите исключить {member.DisplayName} из семьи?",
                "Исключить", "Отмена");

            if (!confirm) return;

            var currentFamily = _currentFamilyService?.GetCurrentFamily();
            if (currentFamily == null) return;

            bool result = await _memberControlService.Kick(member.UserId, currentFamily.Id);

            if (result)
            {
                // Удаляем из локальных данных
                var memberToRemove = FamilyMembers.FirstOrDefault(m => m.UserId == member.UserId);
                if (memberToRemove != null)
                {
                    FamilyMembers.Remove(memberToRemove);
                    MembersCount = $"{FamilyMembers.Count} участников";

                    // ОБНОВЛЯЕМ ВЫСОТУ ПОСЛЕ УДАЛЕНИЯ УЧАСТНИКА
                    if (CurrentTab?.Type == TabType.Members)
                    {
                        UpdateTabHeight();
                    }
                }

                await Shell.Current.DisplayAlert("Успех",
                    $"{member.DisplayName} исключен из семьи", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Ошибка",
                    "Не удалось исключить участника. Проверьте ваши права.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка",
                $"Ошибка при исключении участника: {ex.Message}", "OK");
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

                    // Устанавливаем текст даты создания
                    if (plate.CreatedAt != DateTime.MinValue)
                    {
                        plate.CreatedDateText = GetFormattedDate(plate.CreatedAt);
                    }
                    else
                    {
                        plate.CreatedDateText = "Сегодня";
                    }

                    // Загружаем блюда для тарелки
                    await LoadDishesForPlate(plate);

                    platesWithDishes.Add(plate);
                }

                // СОРТИРУЕМ ТАРЕЛКИ ПО ДАТЕ (самые старые сверху)
                platesWithDishes = platesWithDishes
                    .OrderBy(p => p.CreatedAt)
                    .ToList();

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

                // Сортируем обработанные тарелки: сначала по дате (старые сверху), потом по статусу
                var sortedProcessedPlates = ProcessedPlates
                    .OrderBy(p => p.CreatedAt) // Сначала старые
                    .ThenByDescending(p => p.HasAnyAcceptedDish) // Затем с принятыми блюдами
                    .ThenByDescending(p => p.Status == RequestStatus.Accepted) // Затем принятые тарелки
                    .ToList();

                ProcessedPlates.Clear();
                foreach (var plate in sortedProcessedPlates)
                {
                    ProcessedPlates.Add(plate);
                }

                // Сортируем ожидающие тарелки по дате (старые сверху)
                var sortedPendingPlates = PendingPlates
                    .OrderBy(p => p.CreatedAt)
                    .ToList();

                PendingPlates.Clear();
                foreach (var plate in sortedPendingPlates)
                {
                    PendingPlates.Add(plate);
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

            // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ ПОСЛЕ ЗАГРУЗКИ ТАРЕЛОК
            if (SelectedTabIndex == 0 || CurrentTab?.Type == TabType.Activity)
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
                        // ВАЖНО: Сохраняем dishOnPlateId из связи между тарелкой и блюдом
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

        // Рассчитываем статистику блюд
        int totalDishes = plate.Dishes.Count;
        int acceptedCount = plate.Dishes.Count(d => d.Status == RequestStatus.Accepted);
        int rejectedCount = plate.Dishes.Count(d => d.Status == RequestStatus.Rejected);
        int pendingCount = plate.Dishes.Count(d => d.Status == RequestStatus.Pending);

        bool allProcessed = pendingCount == 0;
        bool hasAcceptedDish = acceptedCount > 0;
        bool hasRejectedDish = rejectedCount > 0;

        // Обновляем флаги
        plate.HasAnyAcceptedDish = hasAcceptedDish;
        plate.IsProcessed = allProcessed;
        plate.ProcessedCount = acceptedCount + rejectedCount;
        plate.TotalCount = totalDishes;

        if (allProcessed)
        {
            // ВСЕ блюда обработаны - определяем финальный статус тарелки
            if (hasAcceptedDish)
            {
                // Есть хотя бы одно принятое блюдо - тарелка считается принятой
                plate.Status = RequestStatus.Accepted;
                plate.StatusText = "Принята";
                plate.StatusColor = GetStatusColor(RequestStatus.Accepted); // Зеленый
            }
            else if (hasRejectedDish)
            {
                // Все блюда отклонены
                plate.Status = RequestStatus.Rejected;
                plate.StatusText = "Отклонена";
                plate.StatusColor = GetStatusColor(RequestStatus.Rejected); // Красный
            }
        }
        else
        {
            // Есть ожидающие блюда
            plate.Status = RequestStatus.Pending;

            if (acceptedCount > 0 || rejectedCount > 0)
            {
                // Частично обработано
                plate.StatusText = $"Обработка ({acceptedCount + rejectedCount}/{totalDishes})";
                plate.StatusColor = Color.FromArgb("#2196F3"); // Синий для обработки
            }
            else
            {
                // Все еще ожидают
                plate.StatusText = "Ожидает";
                plate.StatusColor = GetStatusColor(RequestStatus.Pending); // Оранжевый
            }
        }

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

                // Проверяем, если тарелка теперь полностью обработана
                bool allProcessed = plate.Dishes?.All(d =>
                    d.Status == RequestStatus.Accepted || d.Status == RequestStatus.Rejected) == true;

                if (allProcessed && PendingPlates.Contains(plate))
                {
                    // Перемещаем в обработанные и ОБНОВЛЯЕМ статус
                    PendingPlates.Remove(plate);

                    // Важно: нужно пересчитать статус после перемещения
                    UpdatePlateStatusBasedOnDishes(plate);

                    ProcessedPlates.Insert(0, plate);
                    SortProcessedPlates();
                }
                else
                {
                    // Если не все блюда обработаны, обновляем в текущем списке
                    var index = PendingPlates.IndexOf(plate);
                    if (index >= 0)
                    {
                        PendingPlates[index] = plate;
                    }
                }
            }

            WeakReferenceMessenger.Default.Send(new DishStatusUpdatedMessage(dish.dishOnPlateId, dish.Status));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при принятии блюда: {ex.Message}");
            dish.Status = RequestStatus.Pending;
            UpdateDishUIProperties(dish);
        }

        // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ
        if (CurrentTab?.Type == TabType.Activity)
        {
            UpdateTabHeight();
        }
    }

    [RelayCommand]
    private async void RejectDish(Dish dish)
    {
        if (dish == null || dish.Status != RequestStatus.Pending) return;

        try
        {
            dish.Status = RequestStatus.Rejected;
            UpdateDishUIProperties(dish);
            await _dishService.EditDishStatus(dish.dishOnPlateId, dish.Status);

            var plate = FindPlateForDish(dish);
            if (plate != null)
            {
                UpdatePlateStatusBasedOnDishes(plate);

                bool allProcessed = plate.Dishes?.All(d =>
                    d.Status == RequestStatus.Accepted || d.Status == RequestStatus.Rejected) == true;

                if (allProcessed && PendingPlates.Contains(plate))
                {
                    PendingPlates.Remove(plate);

                    // Важно: нужно пересчитать статус после перемещения
                    UpdatePlateStatusBasedOnDishes(plate);

                    ProcessedPlates.Add(plate);
                    SortProcessedPlates();
                }
                else
                {
                    var index = PendingPlates.IndexOf(plate);
                    if (index >= 0)
                    {
                        PendingPlates[index] = plate;
                    }
                }
            }

            WeakReferenceMessenger.Default.Send(new DishStatusUpdatedMessage(dish.dishOnPlateId, dish.Status));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка при отклонении блюда: {ex.Message}");
            dish.Status = RequestStatus.Pending;
            UpdateDishUIProperties(dish);
        }

        // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ
        if (CurrentTab?.Type == TabType.Activity)
        {
            UpdateTabHeight();
        }
    }

    private Plate FindPlateForDish(Dish dish)
    {
        if (string.IsNullOrEmpty(dish.dishOnPlateId)) return null;

        // Ищем в ожидающих тарелках по dishOnPlateId
        var plate = PendingPlates.FirstOrDefault(p =>
            p.Dishes?.Any(d => d.dishOnPlateId == dish.dishOnPlateId) == true);
        if (plate != null) return plate;

        // Ищем в обработанных тарелках по dishOnPlateId
        return ProcessedPlates.FirstOrDefault(p =>
            p.Dishes?.Any(d => d.dishOnPlateId == dish.dishOnPlateId) == true);
    }

    private void SortProcessedPlates()
    {
        var sortedProcessedPlates = ProcessedPlates
            .OrderBy(p => p.CreatedAt) // Старые сверху
            .ThenByDescending(p => p.HasAnyAcceptedDish) // С принятыми блюдами
            .ThenByDescending(p => p.Status == RequestStatus.Accepted) // Принятые тарелки
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

    private void UpdateDishStatus(string dishOnPlateId, RequestStatus status)
    {
        if (string.IsNullOrEmpty(dishOnPlateId)) return;

        // Ищем блюдо во всех тарелках по dishOnPlateId
        foreach (var plate in PendingPlates.Concat(ProcessedPlates))
        {
            var dish = plate.Dishes?.FirstOrDefault(d => d.dishOnPlateId == dishOnPlateId);
            if (dish != null)
            {
                dish.Status = status;
                UpdateDishUIProperties(dish);
                UpdatePlateStatusBasedOnDishes(plate);

                // Не выходим из цикла, так как dishOnPlateId уникален и найден только в одной тарелке
            }
        }

        // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ
        if (CurrentTab?.Type == TabType.Activity)
        {
            UpdateTabHeight();
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

                    WeakReferenceMessenger.Default.Send(new DishStatusUpdatedMessage(dish.dishOnPlateId, dish.Status));
                }

                // Обновляем статус тарелки
                UpdatePlateStatusBasedOnDishes(plate);

                // Перемещаем в обработанные
                if (plate.IsProcessed && PendingPlates.Contains(plate))
                {
                    PendingPlates.Remove(plate);

                    // Еще раз обновляем статус для правильного цвета и текста
                    UpdatePlateStatusBasedOnDishes(plate);

                    ProcessedPlates.Insert(0, plate);
                    SortProcessedPlates();
                }

                System.Diagnostics.Debug.WriteLine($"Тарелка {plateId} принята полностью");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при принятии тарелки: {ex.Message}");
            }

            // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ
            if (CurrentTab?.Type == TabType.Activity)
            {
                UpdateTabHeight();
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

                    WeakReferenceMessenger.Default.Send(new DishStatusUpdatedMessage(dish.dishOnPlateId, dish.Status));
                }

                UpdatePlateStatusBasedOnDishes(plate);

                if (plate.IsProcessed && PendingPlates.Contains(plate))
                {
                    PendingPlates.Remove(plate);

                    // Еще раз обновляем статус для правильного цвета и текста
                    UpdatePlateStatusBasedOnDishes(plate);

                    ProcessedPlates.Add(plate);
                    SortProcessedPlates();
                }

                System.Diagnostics.Debug.WriteLine($"Тарелка {plateId} отклонена полностью");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при отклонении тарелки: {ex.Message}");
            }

            // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ
            if (CurrentTab?.Type == TabType.Activity)
            {
                UpdateTabHeight();
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
        IsLoadingMembers = true;
        await Task.Delay(800);
        IsLoadingMembers = false;

        // ОБНОВЛЯЕМ ВЫСОТУ ПОСЛЕ ЗАГРУЗКИ УЧАСТНИКОВ
        if (CurrentTab?.Type == TabType.Members)
        {
            UpdateTabHeight();
        }
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
                if (FamilyMembers == null || !FamilyMembers.Any()) return 250;

                // Учитываем высоту для каждого участника
                // Каждый участник: ~120px (высота карточки) + отступы
                double membersHeight = 100; // Базовый отступ + заголовок

                foreach (var member in FamilyMembers)
                {
                    membersHeight += 120; // Высота карточки участника

                    // Если есть подсказка управления, добавляем высоту
                    if (!member.IsCurrentUser)
                    {
                        membersHeight += 10; // Дополнительный отступ для кнопок
                    }
                }

                return Math.Min(membersHeight, 800); // Максимум 800px

            case TabType.Requests:
                if (!HasPendingRequests) return 250;

                // ~140px на запрос
                double requestsHeight = 100 + (PendingRequests.Count * 140);
                return Math.Min(requestsHeight, 600);

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
    private async void EditFamily()
    {
        var family = _currentFamilyService?.GetCurrentFamily();
        if (family == null) return;

        if (Application.Current?.MainPage is MainPage mainPage &&
            mainPage.CurrentPage is NavigationPage nav)
        {
            var editVm = new EditFamilyViewModel(_familyService, _currentFamilyService)
            {
                EditName = family.Name ?? "",
                EditDescription = family.Description ?? "",
                FamilyId = family.Id ?? ""
            };
            await nav.Navigation.PushAsync(new EditFamilyPage(editVm));
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
                $"Профиль участника: {member.DisplayName}\n" +
                $"Роль: {member.RoleText}", "OK");
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
                RoleText = _memberControlService.GetRoleText(FamilyRole.Member),
                RoleColor = _memberControlService.GetRoleColor(FamilyRole.Member),
                JoinedAt = DateTime.UtcNow
            };

            await _familyService.AcceptMember(request);
            await _membershipService.UpdateRequestStatus(request, RequestStatus.Accepted);

            request.Status = RequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;
            request.RespondedBy = _currentUserService.CurrentUser?.Uid;

            PendingRequests.Remove(request);

            // Загружаем права для нового участника
            newMember.CanPromote = await _memberControlService.CanPromote(newMember);
            newMember.CanDemote = await _memberControlService.CanDemote(newMember);
            newMember.CanKick = await _memberControlService.CanKick(newMember);

            FamilyMembers.Add(newMember);
            MembersCount = $"{FamilyMembers.Count} участников";

            _userNames[request.UserId] = request.UserDisplayName;

            // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ ПОСЛЕ ДОБАВЛЕНИЯ УЧАСТНИКА
            if (CurrentTab?.Type == TabType.Members)
            {
                UpdateTabHeight();
            }

            // Обновляем высоту вкладки запросов
            if (CurrentTab?.Type == TabType.Requests)
            {
                UpdateTabHeight();
            }
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

            // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ ЗАПРОСОВ
            if (CurrentTab?.Type == TabType.Requests)
            {
                UpdateTabHeight();
            }
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

            // ОБНОВЛЯЕМ ВЫСОТУ ВКЛАДКИ ЗАПРОСОВ
            if (CurrentTab?.Type == TabType.Requests)
            {
                UpdateTabHeight();
            }
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

    partial void OnFamilyMembersChanged(ObservableCollection<FamilyMember> value)
    {
        // ОБНОВЛЯЕМ ВЫСОТУ ПРИ ИЗМЕНЕНИИ СПИСКА УЧАСТНИКОВ
        if (CurrentTab?.Type == TabType.Members)
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

    private string GetFormattedDate(DateTime dateTime)
    {
        var dateUtc = dateTime.Kind == DateTimeKind.Local ? dateTime.ToUniversalTime() : dateTime;
        var now = DateTime.UtcNow;

        if (dateUtc > now)
        {
            return "Только что";
        }

        var timeSpan = now - dateUtc;

        if (timeSpan.TotalDays < 1)
        {
            if (timeSpan.TotalHours < 1)
            {
                if (timeSpan.TotalMinutes < 1)
                {
                    return "Только что";
                }
                var minutes = (int)timeSpan.TotalMinutes;
                return minutes == 1 ? "1 минуту назад" : $"{minutes} минут назад";
            }
            var hours = (int)timeSpan.TotalHours;
            return hours == 1 ? "1 час назад" : $"{hours} часов назад";
        }
        else if (timeSpan.TotalDays < 2)
        {
            return "Вчера в " + dateUtc.ToLocalTime().ToString("HH:mm");
        }
        else if (timeSpan.TotalDays < 7)
        {
            var days = (int)timeSpan.TotalDays;
            var localDate = dateUtc.ToLocalTime();
            return $"{days} дня назад в {localDate:HH:mm}";
        }
        else if (timeSpan.TotalDays < 30)
        {
            var weeks = (int)(timeSpan.TotalDays / 7);
            var localDate = dateUtc.ToLocalTime();
            return $"{weeks} недель назад, {localDate:dd MMM в HH:mm}";
        }
        else
        {
            var localDate = dateUtc.ToLocalTime();
            return localDate.ToString("dd MMM yyyy в HH:mm");
        }
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