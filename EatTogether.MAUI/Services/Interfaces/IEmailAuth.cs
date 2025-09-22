using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.Interfaces
{
    public interface IEmailAuth : IAuthProvider
    {
        Task<User> SignInAsync(string email, string password);
        Task<User> SignUpAsync(string email, string password, string confirmPassword, string displayName, 
                               string firstName, string lastName, DateTime dateOfBirth);
    }
}
