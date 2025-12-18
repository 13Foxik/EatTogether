using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Services.MenuService.Interfaces;

namespace EatTogether.MAUI.Services.MenuService.Implementation
{
    public class CurrentPlateService : ICurrentPlateService
    {
        private readonly CurrentUserService _currentUserService;
        private readonly ICurrentFamilyService _currentFamilyService;
        private readonly ICloudStoreService _cloudStoreService;

        // Тарелка должна быть статической или правильно инициализированной
        public Plate CurrentPlate { get; set; }

        public CurrentPlateService(CurrentUserService currentUserService, ICloudStoreService cloudStoreService, ICurrentFamilyService currentFamilyService)
        {
            _currentUserService = currentUserService;
            _cloudStoreService = cloudStoreService;
            _currentFamilyService = currentFamilyService;

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
                DishesId = new List<string>(),
                FamilyId = _currentFamilyService.GetCurrentFamily().Id
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

        public async Task WriteToDB()
        {
            if(CurrentPlate == null)
            {
                return;
            }

            await _cloudStoreService.AddPlateToDB(CurrentPlate);

            foreach(string dishId in CurrentPlate.DishesId)
            {
                DishOnPlate dish = new DishOnPlate(CurrentPlate.Id, dishId);
                await _cloudStoreService.AddDishOnPlateToDB(dish);
            }

            ClearPlate();
        }

    }

}