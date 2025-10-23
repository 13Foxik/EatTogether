using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.FamilyService.Interfaces;

namespace EatTogether.MAUI.Services.FamilyService.Implementation
{
    public class CurrentFamilyService : ICurrentFamilyService
    {
        public event EventHandler<FamilyChangedEventArgs> FamilyChanged;

        private Family? _currentFamily;
        public Family? CurrentFamily
        {
            get => _currentFamily;
            set
            {
                _currentFamily = value;
                OnFamilyChanged(value);
            }
        }

        public Family? GetCurrentFamily()
        {
            return CurrentFamily;
        }

        public void SetCurrentFamily(Family? family)
        {
            CurrentFamily = family;
            Preferences.Set("family_id", family.Id);
        }

        public void ClearFamily()
        {
            CurrentFamily = null;
            Preferences.Remove("family_id");
        }
        protected virtual void OnFamilyChanged(Family? user)
        {
            FamilyChanged?.Invoke(this, new FamilyChangedEventArgs(user));
        }
    }
}
