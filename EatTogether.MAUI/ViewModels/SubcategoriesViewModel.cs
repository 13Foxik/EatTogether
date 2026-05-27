using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EatTogether.MAUI.Messages;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
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
        private readonly ICurrentPlateService _plateService;
        private readonly ICurrentFamilyService _currentFamilyService;
        private readonly Services.CurrentUserService _currentUserService;
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

        [ObservableProperty]
        private int dishesInPlateCount;

        // Новое свойство для отображения плавающей панели
        [ObservableProperty]
        private bool showPlatePanel;

        // Права на редактирование меню: Editor, Admin, Owner
        [ObservableProperty]
        private bool canEditMenu;

        public bool HasSubcategories => Subcategories?.Count > 0;
        public bool ShowNoSubcategoriesMessage => !IsBusy && !HasSubcategories;
        public bool ShowSubcategoriesContent => !IsBusy && HasSubcategories;
        public string SubcategoriesCount => HasSubcategories ? $"{Subcategories.Count}" : "0";

        public SubcategoriesViewModel(string categoryId, string categoryName,
            ISubcategoryService subcategoryService,
            IDishService dishService,
            ICurrentPlateService plateService,
            ICurrentFamilyService currentFamilyService = null,
            Services.CurrentUserService currentUserService = null)
        {
            _categoryId = categoryId;
            _subcategoryService = subcategoryService;
            _dishService = dishService;
            _plateService = plateService;
            _currentFamilyService = currentFamilyService;
            _currentUserService = currentUserService;
            CategoryName = categoryName;

            // Инициализация
            Subcategories = new ObservableCollection<Subcategory>();

            // Определяем права текущего пользователя
            CheckUserRole();

            // Подписываемся на изменение семьи чтобы обновить роль
            if (_currentFamilyService != null)
                _currentFamilyService.FamilyChanged += (s, e) => CheckUserRole();

            WeakReferenceMessenger.Default.Register<PlateUpdatedMessage>(
                this,
                (recipient, message) =>
                {
                    // Обновляем в основном потоке UI
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        UpdateDishesInPlateCount();
                        await RefreshDishesStateAsync();
                    });
                });

            // Загружаем данные
            MainThread.BeginInvokeOnMainThread(async () => await LoadSubcategoriesAsync());
        }

        private async Task RefreshDishesStateAsync()
        {
            try
            {
                foreach (var subcategory in Subcategories)
                {
                    foreach (var dish in subcategory.Dishes)
                    {
                        dish.IsInPlate = _plateService.HasDish(dish.Id);
                    }
                }

                // Обновляем коллекцию, чтобы UI отреагировал
                var tempList = new ObservableCollection<Subcategory>(Subcategories);
                Subcategories = tempList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении состояния блюд: {ex}");
            }
        }

        public SubcategoriesViewModel(string categoryId, string categoryName)
            : this(categoryId, categoryName,
                  App.Services.GetService<ISubcategoryService>(),
                  App.Services.GetService<IDishService>(),
                  App.Services.GetService<ICurrentPlateService>(),
                  App.Services.GetService<ICurrentFamilyService>(),
                  App.Services.GetService<Services.CurrentUserService>())
        {
        }

        private void CheckUserRole()
        {
            try
            {
                var user = _currentUserService?.GetCurrentUser();
                var family = _currentFamilyService?.GetCurrentFamily();
                if (user == null || family?.Members == null)
                {
                    CanEditMenu = false;
                    return;
                }
                var member = family.Members.FirstOrDefault(m => m.UserId == user.Uid);
                CanEditMenu = (member?.Role ?? Models.FamilyRole.Member) >= Models.FamilyRole.Editor;
            }
            catch
            {
                CanEditMenu = false;
            }
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

                var tempList = new ObservableCollection<Subcategory>();

                foreach (var subcategory in subcategories)
                {
                    var dishes = await _dishService.GetDishListAsync(subcategory.Id);

                    // Создаем ObservableCollection для блюд
                    var dishCollection = new ObservableCollection<Dish>();

                    // Обновляем состояние "в тарелке" для каждого блюда
                    foreach (var dish in dishes)
                    {
                        dish.IsInPlate = _plateService.HasDish(dish.Id);
                        dishCollection.Add(dish);
                    }

                    var newSubcategory = new Subcategory
                    {
                        Id = subcategory.Id,
                        Name = subcategory.Name,
                        FamilyId = subcategory.FamilyId,
                        CategoryId = subcategory.CategoryId,
                        SortOrder = subcategory.SortOrder,
                        Dishes = dishCollection,
                        IsExpanded = false,
                        IsAddingDish = false,
                        NewDishName = string.Empty
                    };

                    tempList.Add(newSubcategory);
                }

                Subcategories = tempList;
                UpdateDishesInPlateCount();
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

            subcategory.IsExpanded = !subcategory.IsExpanded;

            var index = Subcategories.IndexOf(subcategory);
            if (index != -1)
            {
                var updatedSubcategory = new Subcategory
                {
                    Id = subcategory.Id,
                    Name = subcategory.Name,
                    FamilyId = subcategory.FamilyId,
                    CategoryId = subcategory.CategoryId,
                    SortOrder = subcategory.SortOrder,
                    Dishes = new ObservableCollection<Dish>(subcategory.Dishes),
                    IsExpanded = subcategory.IsExpanded,
                    IsAddingDish = subcategory.IsAddingDish,
                    NewDishName = subcategory.NewDishName
                };

                var tempList = Subcategories.ToList();
                tempList[index] = updatedSubcategory;
                Subcategories = new ObservableCollection<Subcategory>(tempList);
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
            // Subcategory теперь ObservableObject — UI обновится автоматически
            subcategory.IsAddingDish = true;
            subcategory.NewDishName = string.Empty;
        }

        [RelayCommand]
        private async Task ConfirmAddingDish(Subcategory subcategory)
        {
            if (subcategory == null) return;

            if (string.IsNullOrWhiteSpace(subcategory.NewDishName))
            {
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Введите название блюда", "OK");
                return;
            }

            try
            {
                var dishName = subcategory.NewDishName.Trim();

                await _dishService.CreateDishAsync(
                    dishName,
                    Preferences.Get("family_id", string.Empty),
                    subcategory.Id);

                // Сбрасываем форму сразу
                subcategory.IsAddingDish = false;
                subcategory.NewDishName = string.Empty;

                await LoadSubcategoriesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении блюда: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось добавить блюдо", "OK");
            }
        }

        [RelayCommand]
        private async Task TogglePlate(Dish dish)
        {
            if (dish == null) return;

            try
            {
                if (dish.IsInPlate)
                {
                    // Если блюдо уже в тарелке - удаляем
                    _plateService.RemoveDish(dish.Id);
                    dish.IsInPlate = false;

                    await Application.Current.MainPage.DisplayAlert("Удалено",
                        $"Блюдо \"{dish.Name}\" удалено из тарелки",
                        "OK");
                }
                else
                {
                    // Если блюдо не в тарелке - добавляем
                    _plateService.AddDish(dish.Id);
                    dish.IsInPlate = true;

                    await ShowAddToPlateNotification(dish.Name);

                }

                // Обновляем UI
                await UpdateDishInCollection(dish);
                UpdateDishesInPlateCount();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при переключении тарелки: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка",
                    "Не удалось изменить состояние тарелки",
                    "OK");
            }
        }

        [RelayCommand]
        private async Task ViewPlateDetails()
        {
            if (DishesInPlateCount == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Тарелка пуста",
                    "Добавьте блюда в тарелку",
                    "OK");
                return;
            }

            // Показываем детали тарелки
            await ShowPlateDetailsDialog();
        }

        private async Task ShowPlateDetailsDialog()
        {
            // Собираем названия блюд в тарелке
            var dishIds = new List<string>();
            foreach (var dishId in _plateService.GetDishIds())
            {
                Dish dish = await _dishService.GetDishAsync(dishId);
                if (dish != null && _plateService.HasDish(dish.Id))
                {
                    dishIds.Add(dishId);
                }
            }

            //var message = $"В вашей тарелке:\n\n";
            //foreach (var dishName in dishNames)
            //{
            //    message += $"• {dishName}\n";
            //}
            //message += $"\nВсего: {DishesInPlateCount} блюд";

            //await Application.Current.MainPage.DisplayAlert("Ваша тарелка", message, "OK");

            if (Application.Current?.MainPage is MainPage mainPage)
            {
                var currentNavigation = mainPage.CurrentPage as NavigationPage;
                if (currentNavigation != null)
                {
                    // Переходим на страницу тарелки через DI
                    var platePage = App.Services.GetService<PlatePage>();
                    if (platePage != null)
                        await currentNavigation.Navigation.PushAsync(platePage);
                }
            }
        }

        private async Task ShowAddToPlateNotification(string dishName)
        {
            await Application.Current.MainPage.DisplayAlert("Добавлено в тарелку",
                $"Блюдо \"{dishName}\" добавлено в тарелку\n\nВ тарелке: {DishesInPlateCount + 1} блюд",
                "OK");
        }

        private async Task UpdateDishInCollection(Dish dish)
        {
            try
            {
                // Находим подкатегорию
                var subcategory = Subcategories.FirstOrDefault(s => s.Dishes?.Any(d => d.Id == dish.Id) == true);
                if (subcategory != null)
                {
                    // Находим индекс блюда
                    var dishIndex = subcategory.Dishes.ToList().FindIndex(d => d.Id == dish.Id);
                    if (dishIndex != -1)
                    {
                        // Обновляем блюдо
                        subcategory.Dishes[dishIndex] = dish;

                        // Создаем новую коллекцию для обновления UI
                        var updatedDishes = new ObservableCollection<Dish>(subcategory.Dishes);
                        subcategory.Dishes = updatedDishes;

                        // Обновляем подкатегорию в основной коллекции
                        var subcategoryIndex = Subcategories.IndexOf(subcategory);
                        if (subcategoryIndex != -1)
                        {
                            var updatedSubcategory = new Subcategory
                            {
                                Id = subcategory.Id,
                                Name = subcategory.Name,
                                FamilyId = subcategory.FamilyId,
                                CategoryId = subcategory.CategoryId,
                                SortOrder = subcategory.SortOrder,
                                Dishes = new ObservableCollection<Dish>(subcategory.Dishes),
                                IsExpanded = subcategory.IsExpanded,
                                IsAddingDish = subcategory.IsAddingDish,
                                NewDishName = subcategory.NewDishName
                            };

                            var tempList = Subcategories.ToList();
                            tempList[subcategoryIndex] = updatedSubcategory;
                            Subcategories = new ObservableCollection<Subcategory>(tempList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении коллекции: {ex}");
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

                // Если блюдо было в тарелке, удаляем его
                if (dish.IsInPlate)
                {
                    _plateService.RemoveDish(dish.Id);
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
                    SubCategoryId = SelectedSubcategoryForEdit.Id,
                    IsInPlate = DishToEdit.IsInPlate
                };

                await _dishService.EditDishAsync(updatedDish);

                IsEditDishDialogVisible = false;
                EditDishName = string.Empty;
                SelectedSubcategoryForEdit = null;
                DishToEdit = null;

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
                    await currentNavigation.Navigation.PopAsync();
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

                // Удаляем все блюда из тарелки, которые относятся к этой подкатегории
                foreach (var dish in subcategory.Dishes)
                {
                    if (dish.IsInPlate)
                    {
                        _plateService.RemoveDish(dish.Id);
                    }
                }

                // Удаляем подкатегорию из коллекции
                var tempList = Subcategories.ToList();
                var subcategoryToRemove = tempList.FirstOrDefault(s => s.Id == subcategory.Id);
                if (subcategoryToRemove != null)
                {
                    tempList.Remove(subcategoryToRemove);
                    Subcategories = new ObservableCollection<Subcategory>(tempList);
                }

                await Application.Current.MainPage.DisplayAlert("Успех", "Подкатегория удалена", "OK");
                UpdateComputedProperties();
                UpdateDishesInPlateCount();
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

                await _subcategoryService.EditSubcategoryAsync(updatedSubcategory);

                IsEditSubcategoryDialogVisible = false;
                EditSubcategoryName = string.Empty;
                SubcategoryToEdit = null;

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
        private async Task ClearPlate()
        {
            if (DishesInPlateCount == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Тарелка пуста",
                    "В тарелке нет блюд",
                    "OK");
                return;
            }

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Очистка тарелки",
                $"Вы уверены, что хотите очистить всю тарелку? ({DishesInPlateCount} блюд)",
                "Очистить",
                "Отмена");

            if (!confirm) return;

            try
            {
                _plateService.ClearPlate();

                await LoadSubcategoriesAsync();
                await Application.Current.MainPage.DisplayAlert("Успех", "Тарелка очищена", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при очистке тарелки: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось очистить тарелку", "OK");
            }
        }

        [RelayCommand]
        private async Task Settings()
        {
            await Application.Current.MainPage.DisplayAlert(
                "Настройки",
                $"Настройки категории: {CategoryName}",
                "OK");
        }

        [RelayCommand]
        private async Task CancelAddingDish(Subcategory subcategory)
        {
            if (subcategory == null) return;

            subcategory.IsAddingDish = false;
            subcategory.NewDishName = string.Empty;
        }

        private void UpdateComputedProperties()
        {
            OnPropertyChanged(nameof(HasSubcategories));
            OnPropertyChanged(nameof(ShowNoSubcategoriesMessage));
            OnPropertyChanged(nameof(ShowSubcategoriesContent));
            OnPropertyChanged(nameof(SubcategoriesCount));
        }

        private void UpdateDishesInPlateCount()
        {
            DishesInPlateCount = _plateService.GetDishCount();
            ShowPlatePanel = DishesInPlateCount > 0;

            OnPropertyChanged(nameof(DishesInPlateCount));
            OnPropertyChanged(nameof(ShowPlatePanel));
        }

        partial void OnSubcategoriesChanged(ObservableCollection<Subcategory> value)
        {
            UpdateComputedProperties();
        }

        partial void OnIsBusyChanged(bool value)
        {
            UpdateComputedProperties();
        }

        partial void OnDishesInPlateCountChanged(int value)
        {
            ShowPlatePanel = value > 0;
            OnPropertyChanged(nameof(ShowPlatePanel));
        }
    }
}