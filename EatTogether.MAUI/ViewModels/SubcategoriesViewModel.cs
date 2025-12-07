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

        [ObservableProperty]
        private bool isAddDishDialogVisible;

        [ObservableProperty]
        private string newDishName;

        [ObservableProperty]
        private Subcategory selectedSubcategoryForDish;

        [ObservableProperty]
        private bool isEditDishDialogVisible;

        [ObservableProperty]
        private string editDishName;

        [ObservableProperty]
        private Subcategory selectedSubcategoryForEdit;

        [ObservableProperty]
        private bool isEditSubcategoryDialogVisible;

        [ObservableProperty]
        private string editSubcategoryName;

        [ObservableProperty]
        private Subcategory subcategoryToEdit;

        [ObservableProperty]
        private Dish dishToEdit;

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
                    foreach(var dish in dishes)
                    {
                        Console.WriteLine(dish.Name);
                    }
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
        private async Task AddDish()
        {
            if (!HasSubcategories)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Сначала создайте подкатегорию", "OK");
                return;
            }

            IsAddDishDialogVisible = true;
            NewDishName = string.Empty;
            SelectedSubcategoryForDish = null;
        }

        [RelayCommand]
        private async Task ConfirmAddDish()
        {
            if (string.IsNullOrWhiteSpace(NewDishName))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название блюда", "OK");
                return;
            }

            if (SelectedSubcategoryForDish == null)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите подкатегорию", "OK");
                return;
            }

            try
            {
                var dishName = NewDishName.Trim();

                // Создаем блюдо через сервис
                await _dishService.CreateDishAsync(
                    dishName,
                    Preferences.Get("family_id", string.Empty),
                    SelectedSubcategoryForDish.Id);

                IsAddDishDialogVisible = false;
                NewDishName = string.Empty;
                SelectedSubcategoryForDish = null;

                // Обновляем список подкатегорий для отображения обновленного количества блюд
                await LoadSubcategoriesAsync();
                await Application.Current.MainPage.DisplayAlert("Успех", $"Блюдо \"{dishName}\" добавлено", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении блюда: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось добавить блюдо", "OK");
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
        private void CancelAddDish()
        {
            IsAddDishDialogVisible = false;
            NewDishName = string.Empty;
            SelectedSubcategoryForDish = null;
        }

        [RelayCommand]
        private async Task DeleteDish(Dish dish)
        {
            if (dish == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление блюда",
                $"Вы уверены, что хотите удалить блюдо \"{dish.Name}\"?",
                "Удалить",
                "Отмена");

            if (!confirm) return;

            try
            {
                await _dishService.DeleteDishAsync(dish.Id);

                // Находим и обновляем подкатегорию, содержащую это блюдо
                var subcategory = Subcategories.FirstOrDefault(s => s.Dishes?.Any(d => d.Id == dish.Id) == true);
                if (subcategory != null)
                {
                    // Удаляем блюдо из коллекции
                    var dishToRemove = subcategory.Dishes.FirstOrDefault(d => d.Id == dish.Id);
                    if (dishToRemove != null)
                    {
                        subcategory.Dishes.Remove(dishToRemove);

                        // Обновляем подкатегорию в коллекции для принудительного обновления UI
                        var index = Subcategories.IndexOf(subcategory);
                        if (index != -1)
                        {
                            Subcategories[index] = subcategory;
                        }
                    }
                }
                await LoadSubcategoriesAsync();
                await Application.Current.MainPage.DisplayAlert("Успех", "Блюдо удалено", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении блюда: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось удалить блюдо", "OK");
            }
        }

        [RelayCommand]
        private async Task EditDish(Dish dish)
        {
            if (dish == null) return;

            try
            {
                DishToEdit = dish;
                EditDishName = dish.Name;

                // Находим подкатегорию, к которой принадлежит блюдо
                SelectedSubcategoryForEdit = Subcategories.FirstOrDefault(s => s.Dishes?.Any(d => d.Id == dish.Id) == true);

                IsEditDishDialogVisible = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при подготовке редактирования блюда: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось загрузить данные для редактирования", "OK");
            }
        }

        [RelayCommand]
        private async Task ConfirmEditDish()
        {
            if (string.IsNullOrWhiteSpace(EditDishName))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название блюда", "OK");
                return;
            }

            if (SelectedSubcategoryForEdit == null)
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Выберите подкатегорию", "OK");
                return;
            }

            if (DishToEdit == null) return;

            try
            {
                var updatedDish = new Dish
                {
                    Id = DishToEdit.Id,
                    Name = EditDishName.Trim(),
                    FamilyId = DishToEdit.FamilyId,
                    SubCategoryId = SelectedSubcategoryForEdit.Id
                };

                // Обновляем блюдо через сервис
                await _dishService.EditDishAsync(updatedDish);

                IsEditDishDialogVisible = false;
                EditDishName = string.Empty;
                SelectedSubcategoryForEdit = null;
                DishToEdit = null;

                // Обновляем список подкатегорий для отображения изменений
                await LoadSubcategoriesAsync();
                await Application.Current.MainPage.DisplayAlert("Успех", "Блюдо обновлено", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при редактировании блюда: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось обновить блюдо", "OK");
            }
        }

        [RelayCommand]
        private void CancelEditDish()
        {
            IsEditDishDialogVisible = false;
            EditDishName = string.Empty;
            SelectedSubcategoryForEdit = null;
            DishToEdit = null;
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
        private async Task DeleteSubcategory(Subcategory subcategory)
        {
            if (subcategory == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Удаление подкатегории",
                $"Вы уверены, что хотите удалить подкатегорию \"{subcategory.Name}\"?\n\nВсе блюда в этой подкатегории также будут удалены.",
                "Удалить",
                "Отмена");

            if (!confirm) return;

            try
            {
                await _subcategoryService.DeleteSubcategoryAsync(subcategory.Id);

                // Удаляем подкатегорию из коллекции
                var subcategoryToRemove = Subcategories.FirstOrDefault(s => s.Id == subcategory.Id);
                if (subcategoryToRemove != null)
                {
                    Subcategories.Remove(subcategoryToRemove);
                }

                await Application.Current.MainPage.DisplayAlert("Успех", "Подкатегория удалена", "OK");
                UpdateComputedProperties();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении подкатегории: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось удалить подкатегорию", "OK");
            }
        }

        [RelayCommand]
        private async Task EditSubcategory(Subcategory subcategory)
        {
            if (subcategory == null) return;

            try
            {
                SubcategoryToEdit = subcategory;
                EditSubcategoryName = subcategory.Name;
                IsEditSubcategoryDialogVisible = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при подготовке редактирования подкатегории: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось загрузить данные для редактирования", "OK");
            }
        }

        [RelayCommand]
        private async Task ConfirmEditSubcategory()
        {
            if (string.IsNullOrWhiteSpace(EditSubcategoryName))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название подкатегории", "OK");
                return;
            }

            if (SubcategoryToEdit == null) return;

            try
            {
                var updatedSubcategory = new Subcategory
                {
                    Id = SubcategoryToEdit.Id,
                    Name = EditSubcategoryName.Trim(),
                    FamilyId = SubcategoryToEdit.FamilyId,
                    CategoryId = SubcategoryToEdit.CategoryId,
                    SortOrder = SubcategoryToEdit.SortOrder
                };

                // Обновляем подкатегорию через сервис
                await _subcategoryService.EditSubcategoryAsync(updatedSubcategory);

                IsEditSubcategoryDialogVisible = false;
                EditSubcategoryName = string.Empty;
                SubcategoryToEdit = null;

                // Обновляем список подкатегорий для отображения изменений
                await LoadSubcategoriesAsync();
                await Application.Current.MainPage.DisplayAlert("Успех", "Подкатегория обновлена", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при редактировании подкатегории: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось обновить подкатегорию", "OK");
            }
        }

        [RelayCommand]
        private void CancelEditSubcategory()
        {
            IsEditSubcategoryDialogVisible = false;
            EditSubcategoryName = string.Empty;
            SubcategoryToEdit = null;
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