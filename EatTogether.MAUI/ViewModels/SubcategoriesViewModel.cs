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
        private readonly string _categoryId;

        [ObservableProperty]
        private string categoryName;

        [ObservableProperty]
        private ObservableCollection<Subcategory> subcategories = new();

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private int totalRecipesInCategory;

        [ObservableProperty]
        private string mostPopularSubcategory = "Загрузка...";

        // Свойства для диалогового окна
        [ObservableProperty]
        private bool isCreateDialogVisible;

        [ObservableProperty]
        private string newSubcategoryName;

        // Вычисляемые свойства для управления видимостью
        public bool HasSubcategories => Subcategories?.Count > 0;
        public bool ShowNoSubcategoriesMessage => !IsBusy && !HasSubcategories;
        public bool ShowSubcategoriesContent => !IsBusy && HasSubcategories;
        public string SubcategoriesCount => HasSubcategories ? $"{Subcategories.Count}" : "0";

        public SubcategoriesViewModel(string categoryId, string categoryName, ISubcategoryService subcategoryService)
        {
            _categoryId = categoryId;
            _subcategoryService = subcategoryService;
            CategoryName = categoryName;

            // Загружаем подкатегории при создании ViewModel
            LoadSubcategoriesAsync();
        }

        // Конструктор для дизайнера
        public SubcategoriesViewModel(string categoryId, string categoryName)
            : this(categoryId, categoryName, Application.Current.Handler.MauiContext.Services.GetService<ISubcategoryService>())
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

                // Загружаем подкатегории из сервиса
                var subcategories = await _subcategoryService.GetSubcategoriesByCategoryAsync(_categoryId, familyId);

                Subcategories.Clear();
                foreach (var subcategory in subcategories)
                {
                    Subcategories.Add(subcategory);
                }

                // Обновляем статистику
                UpdateStatistics();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке подкатегорий: {ex}");
                // Можно показать сообщение об ошибке
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось загрузить подкатегории", "OK");
            }
            finally
            {
                IsBusy = false;
                UpdateComputedProperties();
            }
        }

        [RelayCommand]
        private async Task SubcategoryTapped(Subcategory subcategory)
        {
            if (subcategory == null) return;

            // Здесь будет переход на страницу рецептов подкатегории
            await Application.Current.MainPage.DisplayAlert(
                "Подкатегория",
                $"Выбрана: {subcategory.Name}\nID: {subcategory.Id}",
                "OK");

            // Позже замените на:
            // await Shell.Current.GoToAsync($"{nameof(RecipesPage)}?SubcategoryId={subcategory.Id}&SubcategoryName={subcategory.Name}");
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
            // Открываем диалоговое окно
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

                // Создаем подкатегорию через сервис
                await _subcategoryService.CreateSubcategoryAsync(
                    _categoryId,
                    Preferences.Get("family_id", string.Empty),
                    NewSubcategoryName.Trim());

                // Закрываем диалоговое окно
                IsCreateDialogVisible = false;
                NewSubcategoryName = string.Empty;

                // Обновляем список подкатегорий
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
            // Закрываем диалоговое окно без создания
            IsCreateDialogVisible = false;
            NewSubcategoryName = string.Empty;
        }

        [RelayCommand]
        private async Task Settings()
        {
            // Настройки категории
            await Application.Current.MainPage.DisplayAlert(
                "Настройки",
                $"Настройки категории: {CategoryName}",
                "OK");
        }

        private void UpdateStatistics()
        {
            //// Обновляем общее количество рецептов
            //TotalRecipesInCategory = Subcategories.Sum(s => s.RecipeCount);

            //// Находим самую популярную подкатегорию
            //var mostPopular = Subcategories.OrderByDescending(s => s.RecipeCount).FirstOrDefault();
            //MostPopularSubcategory = mostPopular?.Name ?? "Нет данных";
        }

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(HasSubcategories));
            OnPropertyChanged(nameof(ShowNoSubcategoriesMessage));
            OnPropertyChanged(nameof(ShowSubcategoriesContent));
            OnPropertyChanged(nameof(SubcategoriesCount));
        }

        // Обновляем вычисляемые свойства при изменении коллекции
        partial void OnSubcategoriesChanged(ObservableCollection<Subcategory> value)
        {
            UpdateComputedProperties();
            UpdateStatistics();
        }

        partial void OnIsBusyChanged(bool value)
        {
            UpdateComputedProperties();
        }
    }
}