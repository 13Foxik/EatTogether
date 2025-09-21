using EatTogether.MAUI.Models;

namespace EatTogether.MAUI.Services
{
    public class CurrentUserService
    {
        public event EventHandler<UserChangedEventArgs> UserChanged;

        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            private set
            {
                _currentUser = value;
                OnUserChanged(value);
            }
        }

        public User? GetCurrentUser() => CurrentUser;

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public void ClearUser() => CurrentUser = null;

        protected virtual void OnUserChanged(User? user)
        {
            UserChanged?.Invoke(this, new UserChangedEventArgs(user));
        }
    }

    public class UserChangedEventArgs : EventArgs
    {
        public User? User { get; }

        public UserChangedEventArgs(User? user)
        {
            User = user;
        }
    }
}
