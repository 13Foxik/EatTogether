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

            // Собираем семьи для удаления отдельно, чтобы не изменять коллекцию во время foreach
            var familiesToRemove = new List<string>();

            foreach (var familyId in user.UserFamilies)
            {
                if (!await _familyService.HasUserInFamily(familyId, user))
                {
                    familiesToRemove.Add(familyId);
                }
            }

            if (familiesToRemove.Count > 0)
            {
                foreach (var familyId in familiesToRemove)
                {
                    user.UserFamilies.Remove(familyId);
                }
                await _cloudStoreService.UpdateUserFamilies(user);
            }
        }
    }
}
