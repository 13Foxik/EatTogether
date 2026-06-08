using CommunityToolkit.Mvvm.Messaging;
using EatTogether.MAUI.Messages;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Services.MenuService.Interfaces;

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

            WeakReferenceMessenger.Default.Send(new MenuCountsUpdatedMessage(
                categoryId: categoryId,
                subcategoryCountDelta: 1));
        }

        public async Task<List<Subcategory>> GetSubcategoriesByCategoryAsync(string categoryId, string familyId)
        {
            var subcategories = await _cloudStoreService.GetSubcategoriesAsync(categoryId, familyId);
            return subcategories;
        }

        public async Task DeleteSubcategoryAsync(string subcategoryId)
        {
            await _cloudStoreService.DeleteSubcategoryFromDBAsync(subcategoryId);
        }

        public async Task EditSubcategoryAsync(Subcategory subcategory)
        {
            await _cloudStoreService.EditSubcategoryFromDBAsync(subcategory);
        }
    }
}
