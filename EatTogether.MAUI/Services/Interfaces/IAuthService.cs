using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> SignInWithEmailAsync(string email, string password);
        Task SignOutAsync();
    }
}
