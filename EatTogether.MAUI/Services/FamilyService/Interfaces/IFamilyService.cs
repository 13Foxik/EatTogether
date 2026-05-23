using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.FamilyService.Interfaces
{
    public interface IFamilyService
    {
        Task CreateFamily(Family family);
        Task UpdateFamily(string familyId, string name, string description);
        Task<bool> HasUserInFamily(string familyId, User user);
        Task AcceptMember(MembershipRequest request);
    }
}
