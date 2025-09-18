namespace EatTogether.MAUI.Models
{
    public class User
    {
        public string Uid { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime DateOfBitrhDay { get; set; } = DateTime.UtcNow;
        public bool stateOfSubscribe { get; set; } = false;
        public string Avatar { get; set; } = string.Empty;

        public User() { }

        public User(string uid, string email, string displayName)
        {
            Uid = uid;
            Email = email;
            DisplayName = displayName;
        }
    }
}
