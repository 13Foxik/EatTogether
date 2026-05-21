using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
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
        public async Task SeedDefaultCategoriesAsync()
        {
            await _cloudStoreService.SeedDefaultCategoriesAsync();
        }

        public async Task SetFamilyCategoriesAsync(string familyId)
        {
            List<Category> categories = await _cloudStoreService.GetCategoriesAsync();
            List<FamilyCategory> familyCategories = new List<FamilyCategory>();

            foreach(var category in categories)
            {
                familyCategories.Add(new FamilyCategory(familyId, category.Id, true, category.SortOrder));
            }
            try
            {
                await _cloudStoreService.SetFamilyCategoriesModels(familyCategories);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка категории семьи: {ex}");
            }
        }
        public async Task<List<Category>> GetEnabledCategoriesAsync(string familyId)
        {
            // Загружаем все глобальные категории напрямую — без фильтрации через FamilyCategories.
            // Любая категория добавленная в коллекцию Categories будет видна всем семьям.
            var allCategories = await _cloudStoreService.GetCategoriesAsync();
            return allCategories.OrderBy(c => c.SortOrder).ToList();
        }
    }
}
