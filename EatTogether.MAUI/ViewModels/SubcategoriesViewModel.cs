using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.MenuService.Interfaces;
using EatTogether.MAUI.Views.Main;
using EatTogether.MAUI.Views.Main.MenuPages;
using System.Collections.ObjectModel;

namespace EatTogether.MAUI.ViewModels
{
    public partial class SubcategoriesViewModel : ObservableObject
    {
        private readonly ISubcategoryService _subcategoryService;
        private readonly IDishService _dishService;
        private readonly string _categoryId;

        [ObservableProperty]
        private string categoryName;

        [ObservableProperty]
        private ObservableCollection<Subcategory> subcategories = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isCreateDialogVisible;

        [ObservableProperty]
        private string newSubcategoryName;

        public bool HasSubcategories => Subcategories?.Count > 0;
        public bool ShowNoSubcategoriesMessage => !IsBusy && !HasSubcategories;
        public bool ShowSubcategoriesContent => !IsBusy && HasSubcategories;
        public string SubcategoriesCount => HasSubcategories ? $"{Subcategories.Count}" : "0";

        public SubcategoriesViewModel(string categoryId, string categoryName, ISubcategoryService subcategoryService, IDishService dishService)
        {
            _categoryId = categoryId;
            _subcategoryService = subcategoryService;
            _dishService = dishService;
            CategoryName = categoryName;

            LoadSubcategoriesAsync();
        }

        public SubcategoriesViewModel(string categoryId, string categoryName)
            : this(categoryId, categoryName,
                  Application.Current.Handler.MauiContext.Services.GetService<ISubcategoryService>(),
                  Application.Current.Handler.MauiContext.Services.GetService<IDishService>())
        {
        }

        [RelayCommand]
        private async Task LoadSubcategoriesAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var familyId = Preferences.Get("family_id", string.Empty);
                var subcategories = await _subcategoryService.GetSubcategoriesByCategoryAsync(_categoryId, familyId);

                Subcategories.Clear();
                foreach (var subcategory in subcategories)
                {
                    var dishes = await _dishService.GetDishListAsync(subcategory.Id);
                    subcategory.Dishes = dishes;
                    subcategory.IsExpanded = false;
                    subcategory.IsAddingDish = false;
                    subcategory.NewDishName = string.Empty;

                    Subcategories.Add(subcategory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке подкатегорий: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось загрузить подкатегории", "OK");
            }
            finally
            {
                IsBusy = false;
                UpdateComputedProperties();
            }
        }

        [RelayCommand]
        private void ToggleSubcategory(Subcategory subcategory)
        {
            if (subcategory == null) return;

            Console.WriteLine($"ToggleSubcategory called for: {subcategory.Name}");
            Console.WriteLine($"Current IsExpanded: {subcategory.IsExpanded}");

            // Переключаем состояние раскрытия
            subcategory.IsExpanded = !subcategory.IsExpanded;

            Console.WriteLine($"New IsExpanded: {subcategory.IsExpanded}");

            // Обновляем только измененную подкатегорию
            var index = Subcategories.IndexOf(subcategory);
            if (index != -1)
            {
                // Создаем новый объект для принудительного обновления
                var updatedSubcategory = new Subcategory
                {
                    Id = subcategory.Id,
                    Name = subcategory.Name,
                    FamilyId = subcategory.FamilyId,
                    CategoryId = subcategory.CategoryId,
                    SortOrder = subcategory.SortOrder,
                    Dishes = subcategory.Dishes,
                    IsExpanded = subcategory.IsExpanded,
                    IsAddingDish = subcategory.IsAddingDish,
                    NewDishName = subcategory.NewDishName
                };

                Subcategories[index] = updatedSubcategory;
                Console.WriteLine($"Subcategory updated in collection");
            }
            else
            {
                Console.WriteLine($"Subcategory not found in collection");
            }
        }

        [RelayCommand]
        private void StartAddingDish(Subcategory subcategory)
        {
            if (subcategory == null) return;

            // Сбрасываем режим добавления у всех подкатегорий
            foreach (var item in Subcategories)
            {
                item.IsAddingDish = false;
                item.NewDishName = string.Empty;
            }

            // Включаем режим добавления для выбранной подкатегории
            subcategory.IsAddingDish = true;
            subcategory.NewDishName = string.Empty;

            var index = Subcategories.IndexOf(subcategory);
            if (index != -1)
            {
                Subcategories[index] = subcategory;
            }
        }

        [RelayCommand]
        private async Task AddDish(Subcategory subcategory)
        {
            if (subcategory == null || string.IsNullOrWhiteSpace(subcategory.NewDishName))
            {
                return;
            }

            try
            {
                var dishName = subcategory.NewDishName.Trim();

                // Создаем блюдо через сервис
                await _dishService.CreateDishAsync(
                    dishName,
                    Preferences.Get("family_id", string.Empty),
                    subcategory.Id);

                // Выключаем режим добавления
                subcategory.IsAddingDish = false;
                subcategory.NewDishName = string.Empty;

                // Обновляем список блюд для этой подкатегории
                var dishes = await _dishService.GetDishListAsync(subcategory.Id);
                subcategory.Dishes = dishes;

                var index = Subcategories.IndexOf(subcategory);
                if (index != -1)
                {
                    Subcategories[index] = subcategory;
                }

                await Application.Current.MainPage.DisplayAlert("Успех", $"Блюдо \"{dishName}\" добавлено", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении блюда: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось добавить блюдо", "OK");
            }
        }

        [RelayCommand]
        private void CancelAddingDish(Subcategory subcategory)
        {
            if (subcategory == null) return;

            subcategory.IsAddingDish = false;
            subcategory.NewDishName = string.Empty;

            var index = Subcategories.IndexOf(subcategory);
            if (index != -1)
            {
                Subcategories[index] = subcategory;
            }
        }

        [RelayCommand]
        private async Task GoBack()
        {
            if (Application.Current?.MainPage is MainPage mainPage)
            {
                var currentNavigation = mainPage.CurrentPage as NavigationPage;
                if (currentNavigation != null)
                {
                    await currentNavigation.Navigation.PushAsync(new MenuPage());
                }
            }
        }

        [RelayCommand]
        private async Task CreateSubcategory()
        {
            IsCreateDialogVisible = true;
            NewSubcategoryName = string.Empty;
        }

        [RelayCommand]
        private async Task ConfirmCreateSubcategory()
        {
            if (string.IsNullOrWhiteSpace(NewSubcategoryName))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название подкатегории", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                await _subcategoryService.CreateSubcategoryAsync(
                    _categoryId,
                    Preferences.Get("family_id", string.Empty),
                    NewSubcategoryName.Trim());

                IsCreateDialogVisible = false;
                NewSubcategoryName = string.Empty;

                await LoadSubcategoriesAsync();
                await Application.Current.MainPage.DisplayAlert("Успех", "Подкатегория создана", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании подкатегории: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось создать подкатегорию", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void CancelCreateSubcategory()
        {
            IsCreateDialogVisible = false;
            NewSubcategoryName = string.Empty;
        }

        [RelayCommand]
        private async Task Settings()
        {
            await Application.Current.MainPage.DisplayAlert(
                "Настройки",
                $"Настройки категории: {CategoryName}",
                "OK");
        }

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(HasSubcategories));
            OnPropertyChanged(nameof(ShowNoSubcategoriesMessage));
            OnPropertyChanged(nameof(ShowSubcategoriesContent));
            OnPropertyChanged(nameof(SubcategoriesCount));
        }

        partial void OnSubcategoriesChanged(ObservableCollection<Subcategory> value)
        {
            UpdateComputedProperties();
        }

        partial void OnIsBusyChanged(bool value)
        {
            UpdateComputedProperties();
        }
    }
}