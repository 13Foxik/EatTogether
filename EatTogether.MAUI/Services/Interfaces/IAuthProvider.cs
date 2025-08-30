using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.Interfaces
{
    public enum AuthType
    {
        Email,
        Google,
        Apple
    }
    public interface IAuthProvider
    {
        AuthType Type { get; }
        Task<User> SignInAsync();
        Task<User> SignInAsync(string email, string password);
        //Task<User> SignOutAsync();
    }
}
