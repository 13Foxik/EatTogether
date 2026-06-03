using Google.Cloud.Firestore;
using Microsoft.Maui.Graphics;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class MembershipRequest
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string FamilyId { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string UserDisplayName { get; set; }

        [FirestoreProperty]
        public string UserAvatarUrl { get; set; }

        // Цвет аватарки пользователя (hex), сохраняется при создании заявки
        [FirestoreProperty]
        public string UserAvatarColor { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [FirestoreProperty]
        public DateTime? RespondedAt { get; set; }

        [FirestoreProperty]
        public string Message { get; set; }

        [FirestoreProperty(ConverterType = typeof(RequestStatusConverter))]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        [FirestoreProperty]
        public string RespondedBy { get; set; }

        // Есть ли аватарка-животное
        public bool HasAvatar => !string.IsNullOrEmpty(UserAvatarUrl);

        // Строковый hex-цвет
        public string DisplayColor => string.IsNullOrEmpty(UserAvatarColor) ? "#1F744D" : UserAvatarColor;

        // Color-версия для XAML-биндинга BackgroundColor
        public Color DisplayColorValue => Color.FromArgb(DisplayColor);

        public MembershipRequest() { }

        public MembershipRequest(string familyId, string userId, string userDisplayName,
                               string userEmail, string userAvatarUrl, string message = null, string userAvatarColor = null)
        {
            Id = Guid.NewGuid().ToString();
            FamilyId = familyId;
            UserId = userId;
            UserDisplayName = userDisplayName;
            UserAvatarUrl = userAvatarUrl;
            UserAvatarColor = userAvatarColor;
            Message = message;
            CreatedAt = DateTime.UtcNow;
            Status = RequestStatus.Pending;
        }
    }
    public enum RequestStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        Cancelled = 3
    }
    public class RequestStatusConverter : IFirestoreConverter<RequestStatus>
    {
        public RequestStatus FromFirestore(object value)
        {
            if (value is string stringValue)
            {
                if (Enum.TryParse<RequestStatus>(stringValue, true, out var status))
                    return status;
            }
            else if (value is long longValue && Enum.IsDefined(typeof(RequestStatus), (int)longValue))
            {
                return (RequestStatus)longValue;
            }

            return RequestStatus.Pending;
        }

        public object ToFirestore(RequestStatus value)
        {
            return value.ToString();
        }
    }
}