using Google.Cloud.Firestore;
using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class DishOnPlate
    {
        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string PlateId { get; set; }
        [FirestoreProperty]
        public string DishId { get; set; }
        [FirestoreProperty]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DishOnPlate(string plateId, string dishId)
        {
            PlateId = plateId;
            DishId = dishId;
        }
        public DishOnPlate()
        {

        }
    }
}
