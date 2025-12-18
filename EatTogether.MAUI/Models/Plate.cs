// В файл Plate.cs
using CommunityToolkit.Mvvm.ComponentModel;
using Google.Cloud.Firestore;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public partial class Plate : ObservableObject
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string FamilyId { get; set; }

        [FirestoreProperty]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public List<string> DishesId { get; set; }

        // ДОБАВЛЯЕМ ЭТИ СВОЙСТВА ДЛЯ UI
        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _userInitial;

        [ObservableProperty]
        private Color _statusColor;

        [ObservableProperty]
        private string _statusText;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private List<Dish> _dishes = new();

        public Plate()
        {
        }

        public Plate(string userId, List<string> dishesId, string familyId)
        {
            UserId = userId;
            DishesId = dishesId;
            FamilyId = familyId;
        }
    }
}