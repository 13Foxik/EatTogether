using Google.Cloud.Firestore;
using System.Data;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class Category
    {
        [FirestoreProperty]
        public string Id {  get; set; }
        [FirestoreProperty]
        public string Name { get; set; }
        [FirestoreProperty]
        public string Icon { get; set; }
        [FirestoreProperty]
        public int SortOrder { get; set; }

        public Category () { }
    }
}
