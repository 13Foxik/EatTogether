using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthProviderFactory _providerFactory;
        private readonly IFirebaseAuthService _firebaseAuthService;

        public AuthService(IAuthProviderFactory providerFactory,IFirebaseAuthService firebaseAuthService)
        {
            _firebaseAuthService = firebaseAuthService;
            _providerFactory = providerFactory;
        }

        public async Task<User> SignInWithEmailAsync (string email, string password)
        {
            var emailProvider = _providerFactory.GetProvider<IEmailAuth>();
            return await emailProvider.SignInAsync(email, password);
        }
        public Task SignOutAsync()
        {
            return _firebaseAuthService.SignOutAsync();
        }
    }
}
