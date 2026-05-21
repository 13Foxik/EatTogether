using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.MenuService.Interfaces;
using EatTogether.MAUI.Views.Main;
using EatTogether.MAUI.Views.Main.MenuPages;
using System.Collections.ObjectModel;

namespace EatTogether.MAUI.ViewModels
{
    public partial class MenuViewModel : ObservableObject
    {
        private readonly ICategoryService _categoryService;

        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool hasFamily;

        // Проверяем, есть ли категории (только если пользователь в семье)
        public bool HasCategories => HasFamily && Categories?.Count > 0;

        // Показываем сообщение "нет семьи" только если пользователь не в семье
        public bool ShowNoFamilyMessage => !HasFamily;

        // Показываем контент категорий только если пользователь в семье И есть категории
        public bool ShowCategoriesContent => HasFamily && HasCategories && !IsBusy;

        // Показываем сообщение "нет категорий" только если пользователь в семье И нет категорий
        public bool ShowNoCategoriesMessage => HasFamily && !HasCategories && !IsBusy;

        public MenuViewModel(ICategoryService categoryService)
        {
            _categoryService = categoryService;

            // Инициализируем состояние семьи
            CheckFamilyStatus();

            // Загружаем категории только если пользователь в семье
            if (HasFamily)
            {
                LoadCategoriesAsync();
            }
        }

        public MenuViewModel() : this(Application.Current.Handler.MauiContext.Services.GetService<ICategoryService>()) { }

        // Метод для проверки статуса семьи
        private void CheckFamilyStatus()
        {
            HasFamily = !string.IsNullOrEmpty(Preferences.Get("family_id", string.Empty));
        }

        // Метод для обновления вычисляемых свойств
        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(HasCategories));
            OnPropertyChanged(nameof(ShowNoFamilyMessage));
            OnPropertyChanged(nameof(ShowCategoriesContent));
            OnPropertyChanged(nameof(ShowNoCategoriesMessage));
        }

        // Обновляем вычисляемые свойства при изменении коллекции
        partial void OnCategoriesChanged(ObservableCollection<Category> value)
        {
            UpdateComputedProperties();
        }

        // Обновляем вычисляемые свойства при изменении IsBusy
        partial void OnIsBusyChanged(bool value)
        {
            UpdateComputedProperties();
        }

        // Обновляем вычисляемые свойства при изменении HasFamily
        partial void OnHasFamilyChanged(bool value)
        {
            UpdateComputedProperties();

            // Если появилась семья, загружаем категории
            if (value && Categories.Count == 0)
            {
                LoadCategoriesAsync();
            }
        }

        [RelayCommand]
        private async Task CategoryTapped(Category category)
        {
            if (category == null) return;

            if (Application.Current?.MainPage is MainPage mainPage)
            {
                var currentNavigation = mainPage.CurrentPage as NavigationPage;
                if (currentNavigation != null)
                {
                    var subcategoriesPage = new SubcategoriesPage(new SubcategoriesViewModel(category.Id, category.Name));
                    await currentNavigation.Navigation.PushAsync(subcategoriesPage);
                }
            }
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            if (IsBusy || !HasFamily)
                return;

            try
            {
                IsBusy = true;

                var familyId = Preferences.Get("family_id", string.Empty);
                var categories = await _categoryService.GetEnabledCategoriesAsync(familyId);

                Categories.Clear();
                foreach (var category in categories)
                {
                    Categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке категорий: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }
        [RelayCommand]
        private async Task SeedCategories()
        {
            try
            {
                IsBusy = true;
                await _categoryService.SeedDefaultCategoriesAsync();
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сида категорий: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Settings()
        {
            Console.WriteLine("pizda");
        }

        // Метод для обновления состояния при возвращении на страницу
        public void OnAppearing()
        {
            // Всегда проверяем актуальный статус семьи
            CheckFamilyStatus();
        }
    }
}