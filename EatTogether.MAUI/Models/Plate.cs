using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Models
{
    public class Plate
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string FamilyId { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public List<string> DishesId { get; set; }

        public Plate()
        {

        }
        public Plate(string userId, List<string> dishesId)
        {
            UserId = userId;
            DishesId = dishesId;
        }
    }
}
