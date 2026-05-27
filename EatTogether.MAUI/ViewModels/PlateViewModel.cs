using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using EatTogether.MAUI.Messages;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Views.Main;
using EatTogether.MAUI.Services.MenuService.Interfaces;
using System.Collections.ObjectModel;

namespace EatTogether.MAUI.ViewModels
{
    public partial class PlateViewModel : ObservableObject
    {
        private readonly ICurrentPlateService _plateService;
        private readonly IDishService _dishService;

        [ObservableProperty]
        private ObservableCollection<Dish> dishesInPlate = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isEmptyPlate;

        [ObservableProperty]
        private bool isNotEmptyPlate;

        [ObservableProperty]
        private int totalDishesCount;

        public PlateViewModel(ICurrentPlateService plateService, IDishService dishService)
        {
            _plateService = plateService;
            _dishService = dishService;

            // Загружаем данные при создании ViewModel

            DishesInPlate = new ObservableCollection<Dish>();
        }

        public PlateViewModel() : this(
            App.Services.GetService<ICurrentPlateService>(),
            App.Services.GetService<IDishService>())
        {
        }

        [RelayCommand]
        public async Task OnPageAppearingAsync()
        {
            await LoadPlateDishesAsync();
        }


        [RelayCommand]
        public async Task LoadPlateDishesAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                var tempList = new ObservableCollection<Dish>();
                var dishIds = _plateService.GetDishIds();

                tempList = await test(tempList, dishIds);

                DishesInPlate = tempList;
                TotalDishesCount = dishIds.Count();
                UpdateEmptyState();
                IsLoading = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке блюд в тарелке: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось загрузить блюда в тарелке", "OK");
            }
        }

        private async Task<ObservableCollection<Dish>> test(ObservableCollection<Dish> tempList, List<string> dishIds)
        {
            foreach (var dishId in dishIds)
            {
                var dish = await _dishService.GetDishAsync(dishId);
                if (dish != null)
                {
                    dish.IsInPlate = true;
                    tempList.Add(dish);
                }
            }
            return tempList;
        }

        [RelayCommand]
        private async Task RemoveDish(Dish dish)
        {
            if (dish == null) return;

            try
            {
                _plateService.RemoveDish(dish.Id);

                // Удаляем блюдо из коллекции
                var dishToRemove = DishesInPlate.FirstOrDefault(d => d.Id == dish.Id);
                if (dishToRemove != null)
                {
                    DishesInPlate.Remove(dishToRemove);
                }

                TotalDishesCount = DishesInPlate.Count;
                UpdateEmptyState();

                WeakReferenceMessenger.Default.Send(new PlateUpdatedMessage());

                await Application.Current.MainPage.DisplayAlert("Удалено",
                    $"Блюдо \"{dish.Name}\" удалено из тарелки", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении блюда: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось удалить блюдо", "OK");
            }
        }

        [RelayCommand]
        private async Task ClearPlate()
        {
            if (TotalDishesCount == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Тарелка пуста",
                    "В тарелке нет блюд для очистки", "OK");
                return;
            }

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Очистка тарелки",
                $"Вы уверены, что хотите очистить всю тарелку?\n\nУдалить все {TotalDishesCount} блюд?",
                "Очистить",
                "Отмена");

            if (!confirm) return;

            try
            {
                _plateService.ClearPlate();
                DishesInPlate.Clear();
                TotalDishesCount = 0;
                UpdateEmptyState();

                WeakReferenceMessenger.Default.Send(new PlateUpdatedMessage());

                await Application.Current.MainPage.DisplayAlert("Успех",
                    "Тарелка успешно очищена", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при очистке тарелки: {ex}");
                await Application.Current.MainPage.DisplayAlert("Ошибка", "Не удалось очистить тарелку", "OK");
            }
        }

        [RelayCommand]
        private async Task CookNow()
        {
            if (TotalDishesCount == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Тарелка пуста",
                    "Добавьте блюда в тарелку для приготовления", "OK");
                return;
            }

            // Собираем список блюд для отображения
            var dishNames = string.Join("\n", DishesInPlate.Select(d => $"• {d.Name}"));

            await _plateService.WriteToDB();
            DishesInPlate.Clear();
            TotalDishesCount = 0;
            WeakReferenceMessenger.Default.Send(new PlateUpdatedMessage());
            await GoToMenu();
        }

        [RelayCommand]
        private async Task GoToMenu()
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

        private void UpdateEmptyState()
        {
            IsEmptyPlate = TotalDishesCount == 0;
            IsNotEmptyPlate = !IsEmptyPlate;
        }

        partial void OnTotalDishesCountChanged(int value)
        {
            UpdateEmptyState();
        }
    }
}