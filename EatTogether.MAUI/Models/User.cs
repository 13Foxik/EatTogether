namespace EatTogether.MAUI.Models
{
    public class User
    {
        public string Uid { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User() { }

        public User(string uid, string email, string displayName)
        {
            Uid = uid;
            Email = email;
            DisplayName = displayName;
        }
    }
    //Нужен будет FirestoreDataConverter
    //Доработать в момент создания регистрации
    // Сервис текущего пользователя
}
