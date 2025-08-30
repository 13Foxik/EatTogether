using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.Interfaces
{
    public interface IEmailAuth : IAuthProvider
    {
        Task<User> SignInAsync(string email, string password);

    }
}
