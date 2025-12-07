using EatTogether.MAUI.Services.MenuService.Interfaces;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.Services.MenuService.Implementation
{
    public class DishService : IDishService
    {
        private readonly ICloudStoreService _cloudStoreService;
        public DishService(ICloudStoreService cloudStoreService)
        {
            _cloudStoreService = cloudStoreService;
        }

        public async Task CreateDishAsync(string name, string familyId, string subcategoryId)
        {
            Dish dish = new Dish(name, familyId, subcategoryId);
            await _cloudStoreService.AddDishToDbAsync(dish);
        }
        public async Task<List<Dish>> GetDishListAsync(string subcategoryId)
        {
            return await _cloudStoreService.GetDishListFromDbAsync(subcategoryId);
        }

        public async Task DeleteDishAsync(string dishId)
        {
            await _cloudStoreService.DeleteDishFromDBAsync(dishId);
        }

        public async Task EditDishAsync(Dish dish)
        {
            await _cloudStoreService.EditDishFromDBAsync(dish);
        }
    }
}
