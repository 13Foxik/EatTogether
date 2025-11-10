using Google.Cloud.Firestore;

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
        public string FamilyId {  get; set; }
        [FirestoreProperty]
        public string CategoryId { get; set; }
        [FirestoreProperty]
        public int SortOrder { get; set; }
        public Subcategory() { }
        public Subcategory(string id, string name, string familyId, string categoryId)
        {
            Id = id;
            Name = name;
            FamilyId = familyId;
            CategoryId = categoryId;
            //SortOrder = sortOrder;
        }
    }
}
