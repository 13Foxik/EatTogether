using Google.Cloud.Firestore;
using System.Reflection.Metadata;
namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class Dish
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string FamilyId { get; set; }

        [FirestoreProperty]
        public string SubCategoryId { get; set; }

        public bool IsInPlate { get; set; }

        public RequestStatus Status { get; set; }
        public string dishOnPlateId { get; set; }

        // В классе Dish добавьте следующие свойства:
        public string StatusText { get; set; }
        public Color StatusColor { get; set; }
        public Color ButtonBackgroundColor { get; set; }
        public Color ButtonTextColor { get; set; }
        public bool IsStatusVisible { get; set; }
        public bool CanShowActions { get; set; } = true; // По умолчанию показываем кнопки

        public Dish() { }
        public Dish(string name, string familyId, string subcategoryId) 
        {
            Name = name;
            FamilyId = familyId;
            SubCategoryId = subcategoryId;
        }

    }
}
