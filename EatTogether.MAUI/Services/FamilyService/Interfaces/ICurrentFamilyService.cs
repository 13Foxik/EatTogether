using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.FamilyService.Interfaces
{
    public interface ICurrentFamilyService
    {
        event EventHandler<FamilyChangedEventArgs> FamilyChanged;

        Family? CurrentFamily { get; set; }

        Family? GetCurrentFamily();
        void SetCurrentFamily(Family? family);
        void ClearFamily();
    }

    public class FamilyChangedEventArgs : EventArgs
    {
        public Family? Family { get; }

        public FamilyChangedEventArgs(Family? family)
        {
            Family = family;
        }
    }
}