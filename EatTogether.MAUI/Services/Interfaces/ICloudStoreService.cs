using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.Interfaces
{
    public interface ICloudStoreService
    {
        Task InsertUserModel(User user);
        Task UpdateUserFamilies(User user);
        Task UpdateRequestStatus(MembershipRequest request, RequestStatus status);
        Task InsertFamilyModel(Family family);
        Task AddMemberToFamily(string familyId, FamilyMember member);
        Task InsertMembership(MembershipRequest request);
        Task<User> GetUserModel(string documentId);
        Task<Family> GetFamilyModel(string documentId);
        Task<List<Category>> GetCategoriesAsync();
        Task<List<FamilyCategory>> GetFamilyCategoriesAsync(string documentId);
        Task CreateSubcategoriesAsync(string categoryId, string familyId, string name);
        Task<List<Subcategory>> GetSubcategoriesAsync(string categoryId, string familyId);
        Task SetFamilyCategoriesModels(List<FamilyCategory> familyCategories);
        Task<string> GenerateUniqueFamilyIdAsync(string collection);
    }
}
