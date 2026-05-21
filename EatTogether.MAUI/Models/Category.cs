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
        public string ImageFile { get; set; }
        [FirestoreProperty]
        public int SortOrder { get; set; }

        // Показываем картинку если она есть, иначе эмодзи
        public bool HasImage => !string.IsNullOrEmpty(ImageFile);
        public bool HasNoImage => string.IsNullOrEmpty(ImageFile);

        public Category () { }
    }
}
