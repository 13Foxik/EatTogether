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
            family.Id = await _cloudStoreService.GenerateUniqueFamilyIdAsync("Families");
            var currentUser = _currentUserService.GetCurrentUser();
            var member = new FamilyMember
            {
                UserId = currentUser.Uid,
                DisplayName = currentUser.DisplayName,
                Email = currentUser.Email,
                AvatarUrl = currentUser.Avatar ?? string.Empty,
                Role = FamilyRole.Admin,
                JoinedAt = DateTime.UtcNow
            };
            family.AddMember(member);
            try
            {
                await _cloudStoreService.InsertFamilyModel(family);
                _currentFamilyService.SetCurrentFamily(family);
            }
            catch (Exception ex)
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

        public async Task AcceptMember(MembershipRequest request)
        {
            User user = await _cloudStoreService.GetUserModel(request.UserId);

            var member = new FamilyMember
            {
                UserId = user.Uid,
                DisplayName = user.DisplayName,
                Email = user.Email,
                AvatarUrl = user.Avatar ?? string.Empty,
                JoinedAt = DateTime.UtcNow,
                Role = FamilyRole.Member
            };
            var family = _currentFamilyService.GetCurrentFamily();
            if (family != null &&request.FamilyId == family.Id)
            {
                family.AddMember(member);
            }
            await _cloudStoreService.AddMemberToFamily(request.FamilyId, member);
        }
    }
}
