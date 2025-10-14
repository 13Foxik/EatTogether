using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Views.Main;
using System.Collections.ObjectModel;
using EatTogether.MAUI.Views.Main.FamilyPages;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.ViewModels;

public partial class FamilyViewModel : ObservableObject
{
    private readonly CurrentUserService _currentUserService;
    private readonly CreateFamilyViewModel _createFamilyViewModel;
    private readonly ICurrentFamilyService _currentFamilyService;
    private readonly IMembershipService _membershipService;
    private readonly IFamilyService _familyService;

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

    public ObservableCollection<TabItem> Tabs { get; } = new()
    {
        new TabItem { Type = TabType.Activity },
        new TabItem { Type = TabType.Members },
        new TabItem { Type = TabType.Requests }
    };

    public FamilyViewModel(CurrentUserService currentUserService, CreateFamilyViewModel createFamilyViewModel, 
        ICurrentFamilyService currentFamilyService, IMembershipService membershipService, IFamilyService familyService)
    {
        CurrentTab = Tabs.FirstOrDefault() ?? Tabs[0];
        _currentUserService = currentUserService;
        _createFamilyViewModel = createFamilyViewModel;
        _currentFamilyService = currentFamilyService;
        _membershipService = membershipService;
        _familyService = familyService;

        _currentUserService.UserChanged += OnUserChanged;

        // Загружаем запросы при инициализации
        LoadPendingRequests();
    }

    public FamilyViewModel() : this(
        Application.Current.Handler.MauiContext.Services.GetService<CurrentUserService>(),
        Application.Current.Handler.MauiContext.Services.GetService<CreateFamilyViewModel>(),
        Application.Current.Handler.MauiContext.Services.GetService<ICurrentFamilyService>(),
        Application.Current.Handler.MauiContext.Services.GetService<IMembershipService>(),
        Application.Current.Handler.MauiContext.Services.GetService<IFamilyService>())
    {
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

    [RelayCommand]
    private void SelectTab(object parameter)
    {
        if (parameter is string indexStr && int.TryParse(indexStr, out int index) && index >= 0 && index < Tabs.Count)
        {
            SelectedTabIndex = index;
            CurrentTab = Tabs[index];
        }
    }

    [RelayCommand]
    private void SwipeLeft()
    {
        if (IsScrolledDown && SelectedTabIndex < Tabs.Count - 1)
        {
            SelectedTabIndex++;
            CurrentTab = Tabs[SelectedTabIndex];
        }
    }

    [RelayCommand]
    private void SwipeRight()
    {
        if (IsScrolledDown && SelectedTabIndex > 0)
        {
            SelectedTabIndex--;
            CurrentTab = Tabs[SelectedTabIndex];
        }
    }

    [RelayCommand]
    private void ScrollPositionChanged(object parameter)
    {
        if (parameter is double position)
        {
            ScrollPosition = position;
            IsScrolledDown = position > 120;
        }
    }

    [RelayCommand]
    private async Task CreateFamily()
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
    private async Task JoinFamily()
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

    // Команды для работы с запросами
    [RelayCommand]
    private async Task AcceptRequest(MembershipRequest request)
    {
        if (request == null) return;

        try
        {
            // TODO: Реализовать логику принятия запроса
            await _familyService.AcceptMember(request);
            await _membershipService.UpdateRequestStatus(request, RequestStatus.Accepted);

            // Обновляем статус запроса
            request.Status = RequestStatus.Accepted;

            // Удаляем из списка pending запросов
            PendingRequests.Remove(request);

            // Показываем уведомление об успехе
            //await Shell.Current.DisplayAlert("Успех", "Запрос принят", "OK");
        }
        catch (Exception ex)
        {
            //await Shell.Current.DisplayAlert("Ошибка", $"Не удалось принять запрос: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RejectRequest(MembershipRequest request)
    {
        if (request == null) return;

        try
        {
            // TODO: Реализовать логику отклонения запроса
            // await _familyService.RejectMembershipRequest(request.Id);

            // Обновляем статус запроса
            request.Status = RequestStatus.Rejected;

            // Удаляем из списка pending запросов
            PendingRequests.Remove(request);

            // Показываем уведомление об успехе
            //await Shell.Current.DisplayAlert("Успех", "Запрос отклонен", "OK");
        }
        catch (Exception ex)
        {
            //await Shell.Current.DisplayAlert("Ошибка", $"Не удалось отклонить запрос: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ShareInvitation()
    {
        try
        {
            // TODO: Реализовать логику поделиться приглашением
            // var invitationLink = await _familyService.GenerateInvitationLink();
            // await Share.RequestAsync(new ShareTextRequest
            // {
            //     Title = "Приглашение в семью",
            //     Text = $"Присоединяйтесь к моей семье в EatTogether! {invitationLink}"
            // });

            await Shell.Current.DisplayAlert("Поделиться", "Функция поделиться приглашением будет реализована скоро", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка", $"Не удалось поделиться приглашением: {ex.Message}", "OK");
        }
    }

    // Метод для загрузки запросов
    private void LoadPendingRequests()
    {
        try
        {
            // Очищаем текущие запросы
            PendingRequests.Clear();

            // Получаем текущую семью пользователя
            Family? currentFamily = _currentFamilyService?.GetCurrentFamily();

            if (currentFamily?.Memberships != null)
            {
                // Фильтруем только pending запросы
                var pending = currentFamily.Memberships
                    .Where(r => r.Status == RequestStatus.Pending)
                    .ToList();

                // Добавляем в коллекцию
                foreach (var request in pending)
                {
                    PendingRequests.Add(request);
                }
            }

            // Обновляем флаг наличия запросов
            HasPendingRequests = PendingRequests.Any();
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не падаем
            System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке запросов: {ex.Message}");
        }
    }

    // Частичные методы для реакций на изменения свойств
    partial void OnCurrentTabChanged(TabItem value)
    {
        if (value != null)
        {
            SelectedTabIndex = Tabs.IndexOf(value);

            // При переключении на вкладку запросов обновляем список
            if (value.Type == TabType.Requests)
            {
                LoadPendingRequests();
            }
        }
    }

    partial void OnIsScrolledDownChanged(bool value)
    {
        (App.Current.MainPage as MainPage)?.SetSwipeEnabled(!value);
    }

    partial void OnPendingRequestsChanged(ObservableCollection<MembershipRequest> value)
    {
        // Автоматически обновляем флаг при изменении коллекции
        HasPendingRequests = value?.Any() == true;
    }

    // Метод для ручного обновления запросов (можно вызвать извне)
    public void RefreshRequests()
    {
        LoadPendingRequests();
    }
}

public class TabItem
{
    public TabType Type { get; set; }
    public bool IsActivity => Type == TabType.Activity;
    public bool IsMembers => Type == TabType.Members;
    public bool IsRequests => Type == TabType.Requests;
}

public enum TabType
{
    Activity,
    Members,
    Requests
}