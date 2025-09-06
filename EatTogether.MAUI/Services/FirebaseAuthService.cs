using EatTogether.MAUI.Services.Interfaces;
using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Diagnostics;

namespace EatTogether.MAUI.Services
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuthClient _firebaseAuthClient;
        private readonly CurrentUserService _currentUserService;
        public FirebaseAuthService(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;

            var config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyCb2_3YbdpZB78zKbErXZi9zjXQAOkble0",
                AuthDomain = "eattogether-79984.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider(),
                },
                UserRepository = new FileUserRepository("FirebaseSample")
            };
            _firebaseAuthClient = new FirebaseAuthClient(config);
            var currentUserFromFirebase = _firebaseAuthClient.User;


            if (currentUserFromFirebase != null)
            {
                _currentUserService.SetCurrentUser(MapFirebaseUserToAppUser(currentUserFromFirebase));
                Console.WriteLine($"[FirebaseAuthService] User already logged in on startup: {currentUserFromFirebase.Info.Email}");
            }
            else
            {
                _currentUserService.ClearUser();
                Console.WriteLine("[FirebaseAuthService] No user logged in on startup.");
            }
            _firebaseAuthClient.AuthStateChanged += OnAuthStateChanged;
            Console.WriteLine("[FirebaseAuthService] Subscribed to AuthStateChanged event.");
        }
        private void OnAuthStateChanged(object sender, UserEventArgs e)
        {
            if (e.User != null)
            {
                _currentUserService.SetCurrentUser(MapFirebaseUserToAppUser(e.User));
                Console.WriteLine($"[FirebaseAuthService] AuthStateChanged: User logged in: {e.User.Info.Email}");
            }
            else
            {
                _currentUserService.ClearUser();
                Console.WriteLine("[FirebaseAuthService] AuthStateChanged: User logged out.");
            }
        }
        private Models.User MapFirebaseUserToAppUser(Firebase.Auth.User firebaseUser)
        {
            return new Models.User(
                uid: firebaseUser.Uid,
                email: firebaseUser.Info.Email,
                displayName: firebaseUser.Info.DisplayName ?? firebaseUser.Info.Email
            );
        }
        public string GetFirebaseErrorMessage(AuthErrorReason reason)
        {
            return reason switch
            {
                _ => "Произошла неизвестная ошибка аутентификации."
            };
        }
        public async Task SignOutAsync()
        {
            try
            {
                _firebaseAuthClient.SignOut();
                _currentUserService.ClearUser();
                Console.WriteLine("User signed out successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during sign out: {ex.Message}");
                throw;
            }
        }
        public Models.User? GetCurrentUser()
        {
            return _currentUserService.GetCurrentUser();
        }
        public FirebaseAuthClient GetAuthClient()
        {
            return _firebaseAuthClient;
        }
    }
}
