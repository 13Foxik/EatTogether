using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.Interfaces
{
    public interface IUserService
    {
        Task UpdateUser(User user);
    }
}
