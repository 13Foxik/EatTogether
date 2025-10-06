using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Views.Main;

namespace EatTogether.MAUI.ViewModels
{
    public partial class CreateFamilyViewModel : ObservableObject
    {
        private readonly IFamilyService _familyService;
        private readonly IUserService _userService;
        private readonly ICurrentFamilyService _currentFamilyService;
        private readonly CurrentUserService _currentUserService;

        [ObservableProperty]
        private string _familyName;

        [ObservableProperty]
        private string _familyDescription;

        [ObservableProperty]
        private bool _isPrivateFamily;

        public CreateFamilyViewModel(IFamilyService familyService, CurrentUserService currentUserService, 
            ICurrentFamilyService currentFamilyService, IUserService userService)
        {
            _familyService = familyService;
            _currentUserService = currentUserService;
            _currentFamilyService = currentFamilyService;
            _userService = userService;
        }
        public CreateFamilyViewModel() : this(Application.Current.Handler.MauiContext.Services.GetService<IFamilyService>(), 
                                     Application.Current.Handler.MauiContext.Services.GetService<CurrentUserService>(), 
                                     Application.Current.Handler.MauiContext.Services.GetService<ICurrentFamilyService>(),
                                     Application.Current.Handler.MauiContext.Services.GetService<IUserService>()) { }

        [RelayCommand]
        private async Task GoBack()
        {
            if (Application.Current?.MainPage is MainPage mainPage)
            {
                var currentNavigation = mainPage.CurrentPage as NavigationPage;
                if (currentNavigation != null)
                {
                    await currentNavigation.Navigation.PushAsync(new FamilyPage());
                }
            }
        }

        [RelayCommand]
        private async Task CreateFamily()
        {
            if (string.IsNullOrWhiteSpace(FamilyName))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название семьи", "OK");
                return;
            }

            var family = new Family
            {
                Name = FamilyName,
                Description = FamilyDescription,
            };

            try
            {
                await _familyService.CreateFamily(family);
                var user = _currentUserService.GetCurrentUser();

                if (user.UserFamilies == null)
                {
                    user.UserFamilies = new List<string>();
                }

                user.UserFamilies.Add(_currentFamilyService.GetCurrentFamily().Id);
                _userService.UpdateUser(user);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Ошибка при создании семьи: {ex}");
            }

            if (Application.Current?.MainPage is MainPage mainPage)
            {
                var currentNavigation = mainPage.CurrentPage as NavigationPage;
                if (currentNavigation != null)
                {
                    await currentNavigation.Navigation.PushAsync(new FamilyPage());
                }
            }
        }

        [RelayCommand]
        private async Task NavigateToJoin()
        {
            await Shell.Current.GoToAsync("//family/join");
        }

        [RelayCommand]
        private void ChangeAvatar()
        {
            // Логика изменения аватара
        }
    }
}