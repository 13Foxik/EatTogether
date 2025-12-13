using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.MenuService.Interfaces;

namespace EatTogether.MAUI.Services.MenuService.Implementation
{
    public class PlateService : IPlateService
    {
        private readonly CurrentUserService _currentUserService;

        // Тарелка должна быть статической или правильно инициализированной
        public Plate CurrentPlate { get; set; }

        public PlateService(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;

            // Проверяем, не создана ли уже тарелка
            if (CurrentPlate == null)
            {
                CreatePlate();
            }
        }

        public void CreatePlate()
        {
            CurrentPlate = new Plate
            {
                UserId = _currentUserService.GetCurrentUser()?.Uid ?? "default_user",
                DishesId = new List<string>()
            };
        }

        public void ClearPlate()
        {
            CurrentPlate.DishesId.Clear();
        }

        public void AddDish(string dishId)
        {
            if (CurrentPlate == null)
            {
                CreatePlate();
            }

            if (!CurrentPlate.DishesId.Contains(dishId))
            {
                CurrentPlate.DishesId.Add(dishId);
            }
        }

        public void RemoveDish(string dishId)
        {
            CurrentPlate?.DishesId.Remove(dishId);
        }

        public int GetDishCount()
        {
            return CurrentPlate?.DishesId?.Count ?? 0;
        }

        public bool HasDish(string dishId)
        {
            return CurrentPlate?.DishesId?.Contains(dishId) == true;
        }

        public List<string> GetDishIds()
        {
            return CurrentPlate?.DishesId ?? new List<string>();
        }

    }

}