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

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Изменяем на DateTime

        [FirestoreProperty]
        public DateTime? ProcessedAt { get; set; }

        public List<string> DishesId { get; set; }

        // ДОБАВЛЯЕМ ЭТИ СВОЙСТВА ДЛЯ UI
        [ObservableProperty]
        private string _userName;

        [ObservableProperty]
        private string _userInitial;

        [ObservableProperty]
        private string _userAvatarUrl;

        [ObservableProperty]
        private string _userAvatarColor = "#1F744D";

        [ObservableProperty]
        private Color _statusColor;

        [ObservableProperty]
        private string _statusText;

        [ObservableProperty]
        private bool _isExpanded;

        [ObservableProperty]
        private List<Dish> _dishes = new();

        [ObservableProperty]
        private int _processedCount;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private string _createdDateText; // Текст для отображения даты

        public bool IsProcessed { get; set; }
        public bool CanShowActions { get; set; } = true;
        public bool HasAnyAcceptedDish { get; set; }

        public Plate()
        {
        }

        public Plate(string userId, List<string> dishesId, string familyId)
        {
            UserId = userId;
            DishesId = dishesId;
            FamilyId = familyId;
            CreatedAt = DateTime.UtcNow; // Устанавливаем текущую дату
        }
    }
}