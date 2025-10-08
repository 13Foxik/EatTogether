using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.Interfaces
{
    public interface ICloudStoreService
    {
        Task InsertUserModel(User user);
        Task InsertFamilyModel(Family family);
        Task InsertMembership(MembershipRequest request);
        Task<User> GetUserModel(string documentId);
        //Task<Family> GetFamilyModel(string documentId); 
        Task<string> GenerateUniqueIdAsync(string collection);
    }
}
