using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.FamilyService.Interfaces
{
    public interface IFamilyMemberControlService
    {
        Task<bool> CanPromote(FamilyMember targetMember);
        Task<bool> CanDemote(FamilyMember targetMember);
        Task<bool> CanKick(FamilyMember targetMember);
        Task<bool> Promote(string userId, string familyId);
        Task<bool> Demote(string userId, string familyId);
        Task<bool> Kick(string userId, string familyId);
        Task<bool> LeaveFamily(string userId, string familyId);
        string GetRoleText(FamilyRole role);
        Color GetRoleColor(FamilyRole role);
    }
}