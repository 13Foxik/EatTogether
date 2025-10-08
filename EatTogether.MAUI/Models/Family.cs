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

        [FirestoreProperty(ConverterType = typeof(FamilyMemberListConverter))]
        public List<FamilyMember> Members { get; set; } = new();

        [FirestoreProperty(ConverterType = typeof(MembershipRequestListConverter))]
        public List<MembershipRequest> Memberships { get; set; } = new();

        [FirestoreProperty]
        public int CountUsers { get; set; }

        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }

        public Family() { }

        public void AddMember(FamilyMember member)
        {
            if (!Members.Any(m => m.UserId == member.UserId))
            {
                Members.Add(member);
                CountUsers = Members.Count();
            }
        }
    }
    public class FamilyMemberListConverter : IFirestoreConverter<List<FamilyMember>>
    {
        public object ToFirestore(List<FamilyMember> value)
        {
            if (value == null || !value.Any())
                return new List<object>();

            return value.Select(member => new Dictionary<string, object>
            {
                ["userId"] = member.UserId,
                ["displayName"] = member.DisplayName ?? "",
                ["email"] = member.Email ?? "",
                ["avatarUrl"] = member.AvatarUrl ?? "",
                ["role"] = (int)member.Role, // Сохраняем как число
                ["joinedAt"] = member.JoinedAt,
            }).ToList();
        }

        public List<FamilyMember> FromFirestore(object value)
        {
            var members = new List<FamilyMember>();

            if (value is List<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        var member = new FamilyMember
                        {
                            UserId = dict.ContainsKey("userId") ? dict["userId"]?.ToString() : "",
                            DisplayName = dict.ContainsKey("displayName") ? dict["displayName"]?.ToString() : "",
                            Email = dict.ContainsKey("email") ? dict["email"]?.ToString() : "",
                            AvatarUrl = dict.ContainsKey("avatarUrl") ? dict["avatarUrl"]?.ToString() : "",
                            Role = GetRoleFromFirestore(dict),
                            JoinedAt = dict.ContainsKey("joinedAt") ? ((Timestamp)dict["joinedAt"]).ToDateTime() : DateTime.UtcNow,
                        };
                        members.Add(member);
                    }
                }
            }

            return members;
        }

        private FamilyRole GetRoleFromFirestore(Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("role"))
                return FamilyRole.Member;

            try
            {
                // Пробуем разные варианты десериализации
                var roleValue = dict["role"];

                if (roleValue is long longValue)
                    return (FamilyRole)longValue;

                if (roleValue is int intValue)
                    return (FamilyRole)intValue;

                if (roleValue is string stringValue && int.TryParse(stringValue, out int parsedValue))
                    return (FamilyRole)parsedValue;

                return FamilyRole.Member;
            }
            catch
            {
                return FamilyRole.Member;
            }
        }
    }
    public class MembershipRequestListConverter : IFirestoreConverter<List<MembershipRequest>>
    {
        public object ToFirestore(List<MembershipRequest> value)
        {
            if (value == null || !value.Any())
                return new List<object>();

            return value.Select(request => new Dictionary<string, object>
            {
                ["id"] = request.Id,
                ["familyId"] = request.FamilyId,
                ["userId"] = request.UserId,
                ["createdAt"] = request.CreatedAt,
                ["message"] = request.Message ?? ""
            }).ToList();
        }

        public List<MembershipRequest> FromFirestore(object value)
        {
            var requests = new List<MembershipRequest>();

            if (value is List<object> list)
            {
                foreach (var item in list)
                {
                    if (item is Dictionary<string, object> dict)
                    {
                        var request = new MembershipRequest
                        {
                            Id = dict.ContainsKey("id") ? dict["id"]?.ToString() : "",
                            FamilyId = dict.ContainsKey("familyId") ? dict["familyId"]?.ToString() : "",
                            UserId = dict.ContainsKey("userId") ? dict["userId"]?.ToString() : "",
                            CreatedAt = dict.ContainsKey("createdAt") ? ((Timestamp)dict["createdAt"]).ToDateTime() : DateTime.UtcNow,
                            Message = dict.ContainsKey("message") ? dict["message"]?.ToString() : ""
                        };
                        requests.Add(request);
                    }
                }
            }

            return requests;
        }
    }
}
