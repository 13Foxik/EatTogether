using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.Services.FamilyService.Implementation
{
    public class FamilyService : IFamilyService
    {
        private readonly ICurrentFamilyService _currentFamilyService;
        private readonly ICloudStoreService _cloudStoreService;
        private readonly CurrentUserService _currentUserService;

        public FamilyService(ICurrentFamilyService currentFamilyService, ICloudStoreService cloudStoreService, CurrentUserService currentUserService)
        {
            _currentFamilyService = currentFamilyService;
            _cloudStoreService = cloudStoreService;
            _currentUserService = currentUserService;
        }

        public async Task CreateFamily(Family family)
        {
            family.Id = await _cloudStoreService.GenerateUniqueIdAsync("Families");
            var member = new FamilyMember
            {
                UserId = _currentUserService.GetCurrentUser().Uid,
                DisplayName = _currentUserService.GetCurrentUser().DisplayName,
                Email = _currentUserService.GetCurrentUser().Email,
                Role = FamilyRole.Member
            };
            family.AddMember(member);
            try
            {
                await _cloudStoreService.InsertFamilyModel(family);
                _currentFamilyService.SetCurrentFamily(family);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при попытке добавить семью: {ex}");
            }
        }

        public async Task<bool> HasUserInFamily(string familyId, User user)
        {
            Family? family = null;
            family = await _cloudStoreService.GetFamilyModel(familyId);

            string userId = user.Uid;

            if (family == null)
            {
                return false;
            }
            else if (family.Members.Exists(m => m.UserId == userId))
            {
                return true;
            }
            return false;
        }
    }
}
