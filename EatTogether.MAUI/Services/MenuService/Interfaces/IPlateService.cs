using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.MenuService.Interfaces
{
    public interface IPlateService
    {
        Plate CurrentPlate { get; set; }
        void CreatePlate();
        void ClearPlate();
        void AddDish(string dishId);
        void RemoveDish(string dishId);
        int GetDishCount();
        List<string> GetDishIds();
        bool HasDish(string dishId);
    }
}