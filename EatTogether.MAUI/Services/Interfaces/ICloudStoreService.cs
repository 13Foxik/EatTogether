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
        Task<bool> DeleteSubcategoryFromDBAsync(string subcategoryId);
        Task EditSubcategoryFromDBAsync(Subcategory subcategory);
        Task AddDishToDbAsync(Dish dish);
        Task<Dish> GetDishAsync(string id);
        Task EditDishStatusFromDB(string dishOnPlateId, RequestStatus status);
        Task<List<Dish>> GetDishListFromDbAsync(string subcategoryId);
        Task<bool> DeleteDishFromDBAsync(string id);
        Task EditDishFromDBAsync(Dish dish);
        Task AddPlateToDB(Plate plate);
        Task AddDishOnPlateToDB(DishOnPlate dish);
        Task EditPlateStatus(string plateId, RequestStatus status);
        Task<List<Plate>> GetFamilyPlatesFromDB(string familyId);
        Task<List<Dish>> GetDishesOnPlateFromDb(string plateId);
        Task SetFamilyCategoriesModels(List<FamilyCategory> familyCategories);
        Task<string> GenerateUniqueFamilyIdAsync(string collection);
    }
}
