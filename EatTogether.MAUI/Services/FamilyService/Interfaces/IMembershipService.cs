using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.FamilyService.Interfaces
{
    public interface IMembershipService
    {
        Task CreateRequest(string familyId, User user);
        Task UpdateRequestStatus(MembershipRequest request, RequestStatus status);
    }
}
