using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.FamilyService.Interfaces;

namespace EatTogether.MAUI.Services.FamilyService.Implementation
{
    public class CurrentFamilyService : ICurrentFamilyService
    {
        public Family? CurrentFamily {  get; set; }

        public Family? GetCurrentFamily()
        {
            return CurrentFamily;
        }

        public void SetCurrentFamily(Family? family)
        {
            CurrentFamily = family;
        }
    }
}
