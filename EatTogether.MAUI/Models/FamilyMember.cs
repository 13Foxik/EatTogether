
using CommunityToolkit.Mvvm.ComponentModel;
using Google.Cloud.Firestore;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public partial class FamilyMember : ObservableObject
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
        public string AvatarColor { get; set; }

        private FamilyRole _role = FamilyRole.Member;
        [FirestoreProperty]
        public FamilyRole Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        [FirestoreProperty]
        public DateTime JoinedAt { get; set; }

        // Дополнительные свойства для UI — все через SetProperty
        public bool IsCurrentUser { get; set; }

        private string _roleText;
        public string RoleText
        {
            get => _roleText;
            set => SetProperty(ref _roleText, value);
        }

        private Color _roleColor;
        public Color RoleColor
        {
            get => _roleColor;
            set => SetProperty(ref _roleColor, value);
        }

        private bool _canPromote;
        public bool CanPromote
        {
            get => _canPromote;
            set => SetProperty(ref _canPromote, value);
        }

        private bool _canDemote;
        public bool CanDemote
        {
            get => _canDemote;
            set => SetProperty(ref _canDemote, value);
        }

        private bool _canKick;
        public bool CanKick
        {
            get => _canKick;
            set => SetProperty(ref _canKick, value);
        }

        public bool HasAvatar => !string.IsNullOrEmpty(AvatarUrl);

        // Возвращает цвет или дефолтный зелёный если не задан
        public string DisplayColor => string.IsNullOrEmpty(AvatarColor) ? "#1F744D" : AvatarColor;

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
