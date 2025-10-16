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

    [ObservableProperty]
    private ObservableCollection<FamilyMember> _familyMembers = new();

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
        _currentFamilyService.FamilyChanged += OnFamilyChanged;


        // Загружаем данные при инициализации
        LoadFamilyData();
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

    private void OnFamilyChanged(object sender, FamilyChangedEventArgs e)
    {
        LoadFamilyData();
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

            // Очищаем текущие данные
            FamilyMembers.Clear();

            // Получаем текущую семью пользователя
            Family? currentFamily = _currentFamilyService?.GetCurrentFamily();

            if (currentFamily != null)
            {
                // Устанавливаем основную информацию о семье
                FamilyName = currentFamily.Name ?? "Моя семья";
                FamilyDescription = currentFamily.Description ?? "Описание семьи пока не добавлено";
                //FamilyAvatar = currentFamily.Avatar;
                MembersCount = $"{currentFamily.CountUsers} учатников";

                // Загружаем участников из списка Members
                if (currentFamily.Members != null && currentFamily.Members.Any())
                {
                    var currentUserId = _currentUserService.CurrentUser?.Uid;

                    foreach (var member in currentFamily.Members)
                    {
                        // Устанавливаем флаг текущего пользователя
                        member.IsCurrentUser = member.UserId == currentUserId;

                        FamilyMembers.Add(member);

                        // Сохраняем информацию о текущем пользователе
                        if (member.IsCurrentUser)
                        {
                            CurrentUserMember = member;
                        }
                    }

                    // Обновляем счетчик участников
                    MembersCount = $"{FamilyMembers.Count} Участников";
                }
                else
                {
                    // Если нет участников в списке Members, используем Memberships
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
                    Email = "", // Можно получить из сервиса пользователей
                    AvatarUrl = membership.UserAvatarUrl,
                    Role = FamilyRole.Member, // По умолчанию
                    JoinedAt = membership.CreatedAt,
                    IsCurrentUser = membership.UserId == currentUserId
                };

                FamilyMembers.Add(member);

                if (member.IsCurrentUser)
                {
                    CurrentUserMember = member;
                }
            }

            MembersCount = $"{FamilyMembers.Count} Участников";
        }
    }

    private string GetInitial(string displayName)
    {
        if (string.IsNullOrEmpty(displayName))
            return "?";

        return displayName.Substring(0, 1).ToUpper();
    }

    private string GetRoleDisplayName(FamilyRole role)
    {
        return role switch
        {
            FamilyRole.Owner => "Создатель семьи",
            FamilyRole.Admin => "Администратор",
            FamilyRole.Member => "Участник",
            _ => "Участник"
        };
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
            IsScrolledDown = position > 50;
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

    [RelayCommand]
    private async Task ViewMemberProfile(FamilyMember member)
    {
        if (member == null) return;

        try
        {
            // TODO: Реализовать переход к профилю участника
            await Shell.Current.DisplayAlert("Профиль",
                $"Профиль участника: {member.DisplayName}\nРоль: {GetRoleDisplayName(member.Role)}", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Ошибка",
                $"Не удалось открыть профиль: {ex.Message}", "OK");
        }
    }

    // Команды для работы с запросами
    [RelayCommand]
    private async Task AcceptRequest(MembershipRequest request)
    {
        if (request == null) return;

        try
        {
            // Создаем FamilyMember из запроса
            var newMember = new FamilyMember
            {
                UserId = request.UserId,
                DisplayName = request.UserDisplayName,
                Email = "", // Можно получить из сервиса пользователей
                AvatarUrl = request.UserAvatarUrl,
                Role = FamilyRole.Member,
                JoinedAt = DateTime.UtcNow
            };

            await _familyService.AcceptMember(request);
            await _membershipService.UpdateRequestStatus(request, RequestStatus.Accepted);

            // Обновляем статус запроса
            request.Status = RequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;
            request.RespondedBy = _currentUserService.CurrentUser?.Uid;

            // Удаляем из списка pending запросов
            PendingRequests.Remove(request);

            // Обновляем список участников
            FamilyMembers.Add(newMember);
            MembersCount = $"{FamilyMembers.Count} Участников";

            // Показываем уведомление об успехе
            //await Shell.Current.DisplayAlert("Успех", "Участник принят в семью", "OK");
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
            // Обновляем статус запроса
            await _membershipService.UpdateRequestStatus(request, RequestStatus.Rejected);

            request.Status = RequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;
            request.RespondedBy = _currentUserService.CurrentUser?.Uid;

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
        //try
        //{
        //    var currentFamily = _currentFamilyService?.GetCurrentFamily();
        //    if (currentFamily != null)
        //    {
        //        var invitationLink = await _familyService.GenerateInvitationLink(currentFamily.Id);

        //        await Share.Default.RequestAsync(new ShareTextRequest
        //        {
        //            Title = "Приглашение в семью",
        //            Text = $"Присоединяйтесь к моей семье '{currentFamily.Name}' в EatTogether! {invitationLink}",
        //            Uri = invitationLink
        //        });
        //    }
        //    else
        //    {
        //        await Shell.Current.DisplayAlert("Ошибка", "Не удалось получить информацию о семье", "OK");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    await Shell.Current.DisplayAlert("Ошибка", $"Не удалось поделиться приглашением: {ex.Message}", "OK");
        //}
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

            // При переключении на вкладку участников обновляем список
            if (value.Type == TabType.Members)
            {
                LoadFamilyData();
            }

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

    // Метод для ручного обновления данных семьи
    public void RefreshFamilyData()
    {
        LoadFamilyData();
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