using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.FamilyService.Interfaces;

namespace EatTogether.MAUI.Services
{
    public class UserService : IUserService
    {
        private readonly ICloudStoreService _cloudStoreService;
        private readonly IFamilyService _familyService;
        private readonly CurrentUserService _currentUserService;
        public UserService(ICloudStoreService cloudStoreService, CurrentUserService currentUserService, IFamilyService familyService)
        {
            _cloudStoreService = cloudStoreService;
            _currentUserService = currentUserService;
            _familyService = familyService;
        }
        public async Task UpdateUser(User user)
        {
            await _cloudStoreService.InsertUserModel(user);
        }
        public async Task CheckFamilies()
        {
            User user = _currentUserService.GetCurrentUser();
            foreach(var family in user.UserFamilies)
            {
                if (!await _familyService.HasUserInFamily(family, user))
                {
                    user.UserFamilies.Remove(family);
                    await _cloudStoreService.UpdateUserFamilies(user);
                }
            }
        }
    }
}
