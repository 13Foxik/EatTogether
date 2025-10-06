

using Google.Cloud.Firestore;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class FamilyMember
    {
        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string DisplayName { get; set; }

        [FirestoreProperty]
        public string Email { get; set; }

        [FirestoreProperty]
        public string AvatarUrl { get; set; }

        [FirestoreProperty]
        public FamilyRole Role { get; set; } = FamilyRole.Member;

        [FirestoreProperty]
        public DateTime JoinedAt { get; set; }


        public FamilyMember() { }

        public FamilyMember(string userId, string displayName, string email, FamilyRole role = FamilyRole.Member)
        {
            UserId = userId;
            DisplayName = displayName;
            Email = email;
            Role = role;
            JoinedAt = DateTime.UtcNow;
        }
    }
    public enum FamilyRole
    {
       Member = 0,
       Admin = 1
    }

}
