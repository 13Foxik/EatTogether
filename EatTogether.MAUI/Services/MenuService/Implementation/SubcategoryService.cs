using EatTogether.MAUI.Services.MenuService.Interfaces;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.Services.MenuService.Implementation
{
    public class SubcategoryService : ISubcategoryService
    {
        private readonly ICloudStoreService _cloudStoreService;

        public SubcategoryService(ICloudStoreService cloudStoreService)
        {
            _cloudStoreService = cloudStoreService;
        }
        public async Task CreateSubcategoryAsync(string categoryId, string familyId, string name)
        {
            await _cloudStoreService.CreateSubcategoriesAsync(categoryId, familyId, name);
        }

        public async Task<List<Subcategory>> GetSubcategoriesByCategoryAsync(string categoryId, string familyId)
        {
            var subcategories = await _cloudStoreService.GetSubcategoriesAsync(categoryId,familyId);
            return subcategories;
        }
    }
}
