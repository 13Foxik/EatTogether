using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Services.MenuService.Interfaces;

namespace EatTogether.MAUI.Services.MenuService.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICloudStoreService _cloudStoreService;

        public CategoryService(ICloudStoreService cloudStoreService)
        {
            _cloudStoreService = cloudStoreService;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = await _cloudStoreService.GetCategoriesAsync();
            return categories.OrderBy(c => c.SortOrder).ToList();
        }

        public async Task SeedDefaultCategoriesAsync()
        {
            await _cloudStoreService.SeedDefaultCategoriesAsync();
        }
    }
}
