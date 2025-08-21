namespace EatTogether.MAUI.Models
{
    public class User
    {
        public string Uid { get; private set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; private set; }
        public int Age { get; set; }
        
        public User(string uid, string displayName, string email, DateTime createdAt, int age)
        {
            Uid = uid;
            DisplayName = displayName;
            Email = email;
            CreatedAt = createdAt;
            Age = age;
        }
    }
    //Нужен будет FirestoreDataConverter
    //Доработать в момент создания регистрации
}
