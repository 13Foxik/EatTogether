using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services
{
    public class CurrentUserService
    {
        public User? CurrentUser { get; private set; }

        public User? GetCurrentUser()
        {
            return CurrentUser;
        }
        public void SetCurrentUser( User user)
        {
            CurrentUser = user; 
        }

        public void ClearUser() => CurrentUser = null;
    }
}
