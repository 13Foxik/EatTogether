using Google.Cloud.Firestore;
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

        public Dish() { }
        public Dish(string name, string familyId, string subcategoryId) 
        {
            Name = name;
            FamilyId = familyId;
            SubCategoryId = subcategoryId;
        }

    }
}
