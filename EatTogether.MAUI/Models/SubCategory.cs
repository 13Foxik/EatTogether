using CommunityToolkit.Mvvm.ComponentModel;
using Google.Cloud.Firestore;
using System.Collections.ObjectModel;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public partial class Subcategory : ObservableObject
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

        // UI-свойства с уведомлением об изменениях
        [ObservableProperty]
        private bool isExpanded;

        [ObservableProperty]
        private string newDishName;

        [ObservableProperty]
        private bool isAddingDish;

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
