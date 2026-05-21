using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.MenuService.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task SeedDefaultCategoriesAsync();
    }
}
