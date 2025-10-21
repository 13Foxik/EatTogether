using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.MenuService.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategoriesAsync();
        Task SetFamilyCategoriesAsync(string familyId);
        Task<List<Category>> GetEnabledCategoriesAsync(string familyId);
    }
}
