
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

        // Цвет аватарки (hex). Сохраняется в Firestore при добавлении участника.
        [FirestoreProperty]
        public string AvatarColor { get; set; }

        [FirestoreProperty]
        public FamilyRole Role { get; set; } = FamilyRole.Member;

        [FirestoreProperty]
        public DateTime JoinedAt { get; set; }

        // Дополнительные свойства для UI
        public bool IsCurrentUser { get; set; }
        public string RoleText { get; set; }
        public Color RoleColor { get; set; }
        public bool CanPromote { get; set; }
        public bool CanDemote { get; set; }
        public bool CanKick { get; set; }

        public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);

        // Строковый hex-цвет (fallback зелёный)
        public string DisplayColor => string.IsNullOrEmpty(AvatarColor) ? "#1F744D" : AvatarColor;

        // Color-версия для XAML-биндинга BackgroundColor
        public Color DisplayColorValue => Color.FromArgb(DisplayColor);

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
        Editor = 1,
        Admin = 2,
        Owner = 3
    }
}
