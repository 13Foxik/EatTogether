using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services
{
    public class AuthService : IAuthService
    {
        private readonly Dictionary<AuthType, IAuthProvider> _providers;
        public AuthService(IAuthProvider emailAuth)
        {
            _providers = new()
            {
                [AuthType.Email] = emailAuth
            };
        }
        public Task<User> SignInAsync(string email, string password)
        {
            return _providers[AuthType.Email].SignInAsync(email, password);
        }
        public Task<User> SignInAsync(AuthType type)
        {
            return _providers[type].SignInAsync();
        }
    }
}
