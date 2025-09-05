
namespace EatTogether.MAUI.Services.Interfaces
{
    public interface IAuthProviderFactory
    {
        IAuthProvider GetProvider(AuthType type);
        T GetProvider<T>() where T : IAuthProvider;
    }
}
