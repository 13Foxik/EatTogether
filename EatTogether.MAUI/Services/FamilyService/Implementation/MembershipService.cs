using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.FamilyService.Interfaces;
using EatTogether.MAUI.Services.Interfaces;

namespace EatTogether.MAUI.Services.FamilyService.Implementation
{
    public class MembershipService : IMembershipService
    {
        private readonly ICloudStoreService _cloudStoreService;
        public MembershipService(ICloudStoreService cloudStoreService)
        {
            _cloudStoreService = cloudStoreService;
        }
        public async Task CreateRequest(string familyId, User user)
        {
            var request = new MembershipRequest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Uid,
                UserDisplayName = user.DisplayName,
                FamilyId = familyId,
                Status = RequestStatus.Pending
            };

            await _cloudStoreService.InsertMembership(request);
        }
    }
}
