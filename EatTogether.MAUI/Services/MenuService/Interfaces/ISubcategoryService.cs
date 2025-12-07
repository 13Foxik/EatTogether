using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.MenuService.Interfaces
{
    public interface ISubcategoryService
    {
        Task CreateSubcategoryAsync(string categoryId, string familyId, string name);
        Task<List<Subcategory>> GetSubcategoriesByCategoryAsync(string categoryId, string familyId);
        Task DeleteSubcategoryAsync(string subcategoryId);
        Task EditSubcategoryAsync(Subcategory subcategory);
    }
}
