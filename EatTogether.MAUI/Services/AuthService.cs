using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services
{
    public class AuthService : IAuthService
    {
        private readonly Dictionary<AuthType, IAuthProvider> _providers;
        public AuthService(IEnumerable<IAuthProvider> providers)
        {
            _providers = providers.ToDictionary(p => p.Type);
        }

        public Task<User> SignInAsync(string email, string password)
        {
            return _providers[AuthType.Email].SignInAsync(email, password);
        }
    }
}
