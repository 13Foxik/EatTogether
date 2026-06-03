using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services.FamilyService.Interfaces
{
    public interface IMembershipService
    {
        // message — необязательное сообщение от пользователя при подаче заявки
        Task CreateRequest(string familyId, User user, string message = null);
        Task UpdateRequestStatus(MembershipRequest request, RequestStatus status);
        Task DeleteRequest(MembershipRequest request);
    }
}
