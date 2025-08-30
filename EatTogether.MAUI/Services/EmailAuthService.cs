using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services
{
    public class EmailAuthService : IEmailAuth, IAuthProvider
    {
        public AuthType Type => AuthType.Email;

        private readonly CurrentUserService _currentUserService;

        public EmailAuthService(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }
        public async Task<User> SignInAsync()
        {
            throw new NotImplementedException("Use SignInAsync(email, password)");
        }
        public async Task<User> SignInAsync(string email, string password)
        {
            await Task.Delay(1000); // Имитация задержки сети

            if (email == "test@test.com" && password == "qwerty")
            {
                var user = new User(
                    uid: "test-uid-123",
                    email: email,
                    displayName: "Тестовый Пользователь"
                );

                _currentUserService.SetCurrentUser(user); // Устанавливаем текущего
                return user; // И возвращаем для гибкости
            }

            throw new Exception("Неверный email или пароль");
        }
    }
}
