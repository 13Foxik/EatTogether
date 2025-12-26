using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.MenuService.Interfaces
{
    public interface IDishService
    {
        Task CreateDishAsync(string name, string familyId, string subcategoryId);
        Task<List<Dish>> GetDishListAsync(string subcategoryId);
        Task DeleteDishAsync(string Id);
        Task EditDishAsync(Dish dish);
        Task<Dish> GetDishAsync(string id);
        Task EditDishStatus(string dishOnPlateId, RequestStatus status);
    }
}
