using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Messages
{
    public class DishStatusUpdatedMessage
    {
        public string DishOnPlateId { get; }
        public RequestStatus Status { get; }

        public DishStatusUpdatedMessage(string dishOnPlateId, RequestStatus status)
        {
            DishOnPlateId = dishOnPlateId;
            Status = status;
        }
    }
}