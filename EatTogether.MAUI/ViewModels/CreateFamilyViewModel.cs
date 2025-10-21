using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Services.MenuService.Interfaces;
using EatTogether.MAUI.Views.Main;

namespace EatTogether.MAUI.ViewModels
{
    public partial class CreateFamilyViewModel : ObservableObject
    {
        private readonly IFamilyService _familyService;
        private readonly IUserService _userService;
        private readonly ICurrentFamilyService _currentFamilyService;
        private readonly ICategoryService _categoryService;
        private readonly CurrentUserService _currentUserService;

        [ObservableProperty]
        private string _familyName;

        [ObservableProperty]
        private string _familyDescription;

        [ObservableProperty]
        private bool _isPrivateFamily;

        public CreateFamilyViewModel(IFamilyService familyService, CurrentUserService currentUserService, 
            ICurrentFamilyService currentFamilyService, IUserService userService, ICategoryService categoryService)
        {
            _familyService = familyService;
            _currentUserService = currentUserService;
            _currentFamilyService = currentFamilyService;
            _userService = userService;
            _categoryService = categoryService;
        }
        public CreateFamilyViewModel() : this(Application.Current.Handler.MauiContext.Services.GetService<IFamilyService>(), 
                                     Application.Current.Handler.MauiContext.Services.GetService<CurrentUserService>(), 
                                     Application.Current.Handler.MauiContext.Services.GetService<ICurrentFamilyService>(),
                                     Application.Current.Handler.MauiContext.Services.GetService<IUserService>(),
                                     Application.Current.Handler.MauiContext.Services.GetService<ICategoryService>()) { }

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

                // Запускаем установку категорий в фоне без ожидания
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _categoryService.SetFamilyCategoriesAsync(_currentFamilyService.GetCurrentFamily().Id);
                        Console.WriteLine("Family categories set successfully");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error setting family categories: {ex}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании семьи: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось создать семью", "OK");
                return;
            }

            // Переход на страницу семьи
            await NavigateToFamilyPage();
        }

        private async Task NavigateToFamilyPage()
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