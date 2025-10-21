using Google.Cloud.Firestore;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class SubCategory
    {
        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string FamilyId {  get; set; }
        [FirestoreProperty]
        public string CategoryId { get; set; }
        [FirestoreProperty]
        public int SortOrder { get; set; }
        public SubCategory() { }

    }
}
