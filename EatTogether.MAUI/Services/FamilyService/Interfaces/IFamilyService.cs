using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.FamilyService.Interfaces
{
    public interface IFamilyService
    {
        Task CreateFamily(Family family);
        Task<bool> HasUserInFamily(string familyId, User user);
        Task AcceptMember(MembershipRequest request);
        Task<bool> LeaveFamily(string familyId, string userId);
    }
}
