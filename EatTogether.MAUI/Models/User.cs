using Google.Cloud.Firestore;
using System.Data;

namespace EatTogether.MAUI.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string Uid { get; set; }
        [FirestoreProperty]
        public string Email { get; set; }
        [FirestoreProperty]
        public string DisplayName { get; set; }
        [FirestoreProperty]
        public DateTime CreatedAt { get; set; }
        [FirestoreProperty]
        public DateTime DateOfBitrhDay { get; set; }
        [FirestoreProperty]
        public bool stateOfSubscribe { get; set; }
        [FirestoreProperty]
        public string Avatar { get; set; }

        public User() { }

        public User(string uid, string email, string displayName)
        {
            Uid = uid;
            Email = email;
            DisplayName = displayName;
        }
    }
    public class DateTimeToTimestampConverter : IFirestoreConverter<DateTime>
    {
        public object ToFirestore(DateTime value) => Timestamp.FromDateTime(value.ToUniversalTime());
        public DateTime FromFirestore(object value)
        {
            if(value is Timestamp timestamp)
            {
                return timestamp.ToDateTime();
            }
            throw new ArgumentException("Invalid value");
        }
    }
}
