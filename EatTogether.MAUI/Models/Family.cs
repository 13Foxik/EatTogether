using CommunityToolkit.Maui.Behaviors;
using Google.Cloud.Firestore;
using System.Data;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class Family
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Avatar { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        [FirestoreProperty]
        public List<User?> members { get; set; }

        [FirestoreProperty]
        public int CountUsers { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        public Family() { }
    }
}
