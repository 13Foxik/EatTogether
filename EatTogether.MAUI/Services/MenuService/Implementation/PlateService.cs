using EatTogether.MAUI.Services.MenuService.Interfaces;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.Services.MenuService.Implementation
{
    public class PlateService : IPlateService
    {
        private readonly ICloudStoreService _cloudStoreService;

        public PlateService(ICloudStoreService cloudStoreService)
        {
            _cloudStoreService = cloudStoreService;
        }
        public async Task<List<Plate>> GetFamilyPlates(string familyId)
        {
            return await _cloudStoreService.GetFamilyPlatesFromDB(familyId);
        }
        public async Task<List<Dish>> GetDishesOnPlate(string plateId)
        {
            return await _cloudStoreService.GetDishesOnPlateFromDb(plateId);
        }
        public async Task EditPlateStatus(string plateId, RequestStatus status)
        {
            await _cloudStoreService.EditPlateStatus(plateId, status);
        }
    }
}
