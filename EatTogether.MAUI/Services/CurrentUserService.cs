using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services
{
    public class CurrentUserService
    {
        public User? CurrentUser { get; private set; }

        public void SetTestUser()
        {
            CurrentUser = new User(
                uid: "test-uid-123",
                email: "test@test.com",
                displayName: "Тестовый Пользователь"
            );
        }
        public void SetCurrentUser( User user)
        {
            CurrentUser = user; 
        }

        public void ClearUser() => CurrentUser = null;
    }
}
