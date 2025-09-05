using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.Services
{
    public class AuthProviderFactory : IAuthProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T GetProvider<T>() where T : IAuthProvider
        {
            return _serviceProvider.GetService<T>();
        }

        public IAuthProvider GetProvider(AuthType type)
        {
            return type switch
            {
                AuthType.Email => _serviceProvider.GetService<IEmailAuth>(),
                _ => throw new NotImplementedException()
            };
        }
    }
}
