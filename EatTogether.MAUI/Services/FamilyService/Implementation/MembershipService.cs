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

        public async Task CreateRequest(string familyId, User user, string message = null)
        {
            var request = new MembershipRequest
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Uid,
                UserDisplayName = user.DisplayName,
                UserAvatarUrl = user.Avatar ?? string.Empty,
                UserAvatarColor = user.AvatarColor ?? "#1F744D",
                Message = message,
                FamilyId = familyId,
                Status = RequestStatus.Pending
            };

            await _cloudStoreService.InsertMembership(request);
        }

        public async Task UpdateRequestStatus(MembershipRequest request, RequestStatus status)
        {
            await _cloudStoreService.UpdateRequestStatus(request, status);
        }

        public async Task DeleteRequest(MembershipRequest request)
        {
            await _cloudStoreService.DeleteMembership(request);
        }
    }
}
