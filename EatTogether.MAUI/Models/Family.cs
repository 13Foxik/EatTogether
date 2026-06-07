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
                ["UserId"] = member.UserId,
                ["DisplayName"] = member.DisplayName ?? "",
                ["Email"] = member.Email ?? "",
                ["AvatarUrl"] = member.AvatarUrl ?? "",
                ["AvatarColor"] = member.AvatarColor ?? "",
                ["Role"] = (int)member.Role, // Сохраняем как число
                ["JoinedAt"] = member.JoinedAt,
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
                            UserId = dict.ContainsKey("UserId") ? dict["UserId"]?.ToString() : "",
                            DisplayName = dict.ContainsKey("DisplayName") ? dict["DisplayName"]?.ToString() : "",
                            Email = dict.ContainsKey("Email") ? dict["Email"]?.ToString() : "",
                            AvatarUrl = dict.ContainsKey("AvatarUrl") ? dict["AvatarUrl"]?.ToString() : "",
                            AvatarColor = dict.ContainsKey("AvatarColor") ? dict["AvatarColor"]?.ToString() : "",
                            Role = GetRoleFromFirestore(dict),
                            JoinedAt = dict.ContainsKey("JoinedAt") ? ((Timestamp)dict["JoinedAt"]).ToDateTime() : DateTime.UtcNow,
                        };
                        members.Add(member);
                    }
                }
            }

            return members;
        }

        private FamilyRole GetRoleFromFirestore(Dictionary<string, object> dict)
        {
            if (!dict.ContainsKey("Role"))
                return FamilyRole.Member;

            try
            {
                // Пробуем разные варианты десериализации
                var roleValue = dict["Role"];

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
                ["Id"] = request.Id ?? "",
                ["FamilyId"] = request.FamilyId ?? "",
                ["UserId"] = request.UserId ?? "",
                ["UserDisplayName"] = request.UserDisplayName ?? "",
                ["UserAvatarUrl"] = request.UserAvatarUrl ?? "",
                ["UserAvatarColor"] = request.UserAvatarColor ?? "#1F744D",
                ["CreatedAt"] = request.CreatedAt,
                ["RespondedAt"] = request.RespondedAt,
                ["Message"] = request.Message ?? "",
                ["Status"] = request.Status.ToString(),
                ["RespondedBy"] = request.RespondedBy ?? ""
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
                            Id = GetString(dict, "Id", "id"),
                            FamilyId = GetString(dict, "FamilyId", "familyId"),
                            UserId = GetString(dict, "UserId", "userId"),
                            UserDisplayName = GetString(dict, "UserDisplayName", "userDisplayName"),
                            UserAvatarUrl = GetString(dict, "UserAvatarUrl", "userAvatarUrl"),
                            UserAvatarColor = GetString(dict, "UserAvatarColor", "userAvatarColor"),
                            CreatedAt = GetDateTime(dict, "CreatedAt", "createdAt") ?? DateTime.UtcNow,
                            RespondedAt = GetDateTime(dict, "RespondedAt", "respondedAt"),
                            Message = GetString(dict, "Message", "message"),
                            Status = GetStatus(dict),
                            RespondedBy = GetString(dict, "RespondedBy", "respondedBy")
                        };
                        requests.Add(request);
                    }
                }
            }

            return requests;
        }

        private static string GetString(Dictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (dict.ContainsKey(key))
                {
                    return dict[key]?.ToString() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static DateTime? GetDateTime(Dictionary<string, object> dict, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (!dict.ContainsKey(key) || dict[key] == null)
                    continue;

                if (dict[key] is Timestamp timestamp)
                    return timestamp.ToDateTime();

                if (dict[key] is DateTime dateTime)
                    return dateTime;
            }

            return null;
        }

        private static RequestStatus GetStatus(Dictionary<string, object> dict)
        {
            if (dict.ContainsKey("Status"))
                return new RequestStatusConverter().FromFirestore(dict["Status"]);

            if (dict.ContainsKey("status"))
                return new RequestStatusConverter().FromFirestore(dict["status"]);

            return RequestStatus.Pending;
        }
    }
}
