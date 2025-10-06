using EatTogether.MAUI.Models;
namespace EatTogether.MAUI.Services.FamilyService.Interfaces
{
    public interface ICurrentFamilyService
    {
        public Family? CurrentFamily { get; set; }

        public Family? GetCurrentFamily();
        public void SetCurrentFamily(Family family);
    }
}
