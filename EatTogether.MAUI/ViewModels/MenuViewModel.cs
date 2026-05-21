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
        private readonly ISubcategoryService _subcategoryService;

        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool hasFamily;

        public bool HasCategories => HasFamily && Categories?.Count > 0;
        public bool ShowNoFamilyMessage => !HasFamily;
        public bool ShowCategoriesContent => HasFamily && HasCategories && !IsBusy;
        public bool ShowNoCategoriesMessage => HasFamily && !HasCategories && !IsBusy;

        public MenuViewModel(ICategoryService categoryService, ISubcategoryService subcategoryService)
        {
            _categoryService = categoryService;
            _subcategoryService = subcategoryService;

            CheckFamilyStatus();

            if (HasFamily)
                LoadCategoriesAsync();
        }

        public MenuViewModel() : this(
            Application.Current.Handler.MauiContext.Services.GetService<ICategoryService>(),
            Application.Current.Handler.MauiContext.Services.GetService<ISubcategoryService>()) { }

        private void CheckFamilyStatus()
        {
            HasFamily = !string.IsNullOrEmpty(Preferences.Get("family_id", string.Empty));
        }

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(HasCategories));
            OnPropertyChanged(nameof(ShowNoFamilyMessage));
            OnPropertyChanged(nameof(ShowCategoriesContent));
            OnPropertyChanged(nameof(ShowNoCategoriesMessage));
        }

        partial void OnCategoriesChanged(ObservableCollection<Category> value) => UpdateComputedProperties();
        partial void OnIsBusyChanged(bool value) => UpdateComputedProperties();
        partial void OnHasFamilyChanged(bool value)
        {
            UpdateComputedProperties();
            if (value && Categories.Count == 0)
                LoadCategoriesAsync();
        }

        [RelayCommand]
        private async Task CategoryTapped(Category category)
        {
            if (category == null) return;

            if (Application.Current?.MainPage is MainPage mainPage &&
                mainPage.CurrentPage is NavigationPage nav)
            {
                await nav.Navigation.PushAsync(
                    new SubcategoriesPage(new SubcategoriesViewModel(category.Id, category.Name)));
            }
        }

        [RelayCommand]
        private async Task LoadCategoriesAsync()
        {
            if (IsBusy || !HasFamily) return;

            try
            {
                IsBusy = true;

                var familyId = Preferences.Get("family_id", string.Empty);
                var loaded = await _categoryService.GetAllCategoriesAsync();

                // Параллельно загружаем счётчики для каждой категории
                var tasks = loaded.Select(async category =>
                {
                    try
                    {
                        var subcategories = await _subcategoryService
                            .GetSubcategoriesByCategoryAsync(category.Id, familyId);

                        category.SubcategoryCount = subcategories.Count;

                        // Блюда считаем суммой по всем подкатегориям — тоже параллельно
                        // (только если подкатегорий немного, иначе дорого)
                        category.DishCount = 0;
                    }
                    catch
                    {
                        category.SubcategoryCount = 0;
                        category.DishCount = 0;
                    }
                });

                await Task.WhenAll(tasks);

                Categories.Clear();
                foreach (var c in loaded)
                    Categories.Add(c);
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
        private async Task Settings() { }

        public void OnAppearing() => CheckFamilyStatus();
    }
}