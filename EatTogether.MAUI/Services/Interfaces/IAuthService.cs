using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User> SignInAsync(string email, string password);
    }
}
