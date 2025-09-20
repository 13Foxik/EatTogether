using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.Interfaces
{
    public interface ICloudStoreService
    {
        Task InsertUserModel(User user);
        Task<User> GetUserModels(string documentId);
    }
}
