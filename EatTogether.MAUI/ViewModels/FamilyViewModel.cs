using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Views.Main;
using System.Collections.ObjectModel;
using EatTogether.MAUI.Views.Main.FamilyPages;

namespace EatTogether.MAUI.ViewModels;

public partial class FamilyViewModel : ObservableObject
{
    private readonly CurrentUserService _currentUserService;
    private readonly CreateFamilyViewModel _createFamilyViewModel;

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

    public ObservableCollection<TabItem> Tabs { get; } = new()
    {
        new TabItem { Type = TabType.Activity },
        new TabItem { Type = TabType.Members },
        new TabItem { Type = TabType.Requests }
    };

    public FamilyViewModel(CurrentUserService currentUserService, CreateFamilyViewModel createFamilyViewModel)
    {
        CurrentTab = Tabs.FirstOrDefault() ?? Tabs[0];
        _currentUserService = currentUserService;

        _currentUserService.UserChanged += OnUserChanged;
        _createFamilyViewModel = createFamilyViewModel;
    }
    public FamilyViewModel() : this(Application.Current.Handler.MauiContext.Services.GetService<CurrentUserService>(), Application.Current.Handler.MauiContext.Services.GetService<CreateFamilyViewModel>())
    {
    }

    private void OnUserChanged(object sender, UserChangedEventArgs e)
    {
        UpadateUserInfo();
    }
    private void UpadateUserInfo()
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

    // Команда для обновления позиции скролла
    [RelayCommand]
    private void ScrollPositionChanged(object parameter)
    {
        if (parameter is double position)
        {
            ScrollPosition = position;
            // Считаем что скролл достаточно опустился если позиция > 200 пикселей
            IsScrolledDown = position > 120;
        }
    }

    partial void OnCurrentTabChanged(TabItem value)
    {
        if (value != null)
        {
            SelectedTabIndex = Tabs.IndexOf(value);
        }
    }

    partial void OnIsScrolledDownChanged(bool value)
    {
        // Уведомляем MainPage об изменении состояния скролла
        (App.Current.MainPage as MainPage)?.SetSwipeEnabled(!value);
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