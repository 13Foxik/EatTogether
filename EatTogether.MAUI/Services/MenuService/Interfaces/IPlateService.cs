using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.MenuService.Interfaces
{
    public interface IPlateService
    {
        Task<List<Plate>> GetFamilyPlates(string familyId);
        Task<List<Dish>> GetDishesOnPlate(string plateId);

        Task EditPlateStatus(string plateId, RequestStatus status);
    }
}
