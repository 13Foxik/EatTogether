using Google.Cloud.Firestore;
using System.Collections.ObjectModel;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class Subcategory
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string FamilyId { get; set; }

        [FirestoreProperty]
        public string CategoryId { get; set; }

        [FirestoreProperty]
        public int SortOrder { get; set; }

        // Изменяем на ObservableCollection для автоматического обновления UI
        public ObservableCollection<Dish> Dishes { get; set; } = new ObservableCollection<Dish>();

        // Эти свойства не хранятся в Firestore, они для UI
        public bool IsExpanded { get; set; }
        public string NewDishName { get; set; }
        public bool IsAddingDish { get; set; }

        public Subcategory() { }

        public Subcategory(string id, string name, string familyId, string categoryId)
        {
            Id = id;
            Name = name;
            FamilyId = familyId;
            CategoryId = categoryId;
            Dishes = new ObservableCollection<Dish>();
        }
    }
}