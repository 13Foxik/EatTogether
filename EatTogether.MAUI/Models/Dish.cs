using CommunityToolkit.Mvvm.ComponentModel;
using Google.Cloud.Firestore;
using System.Reflection.Metadata;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public partial class Dish : ObservableObject
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string FamilyId { get; set; }

        [FirestoreProperty]
        public string SubCategoryId { get; set; }

        // ObservableProperty — UI обновляется без замены объекта в коллекции
        [ObservableProperty]
        private bool _isInPlate;

        public RequestStatus Status { get; set; }
        public string dishOnPlateId { get; set; }

        public string StatusText { get; set; }
        public Color StatusColor { get; set; }
        public Color ButtonBackgroundColor { get; set; }
        public Color ButtonTextColor { get; set; }
        public bool IsStatusVisible { get; set; }
        public bool CanShowActions { get; set; } = true;

        public Dish() { }
        public Dish(string name, string familyId, string subcategoryId)
        {
            Name = name;
            FamilyId = familyId;
            SubCategoryId = subcategoryId;
        }
    }
}
