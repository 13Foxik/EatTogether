using Firebase.Auth;
using Firebase.Auth.Providers;

namespace EatTogether.MAUI.Services.Interfaces
{
    public interface IFirebaseAuthService
    {
        Task SignOutAsync();
        Models.User? GetCurrentUser();
        FirebaseAuthClient GetAuthClient();
    }
}
