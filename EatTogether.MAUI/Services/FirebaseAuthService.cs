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
        private readonly ICloudStoreService _cloudStoreService;
        public FirebaseAuthService(CurrentUserService currentUserService, ICloudStoreService cloudStoreService)
        {
            _currentUserService = currentUserService;
            _cloudStoreService = cloudStoreService;

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
        private async void OnAuthStateChanged(object sender, UserEventArgs e)
        {
            if (e.User != null)
            {
                Models.User? firestoreUser = null;
                int maxRetries = 3;
                int delayMs = 500;

                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        firestoreUser = await _cloudStoreService.GetUserModels(e.User.Uid);
                        _currentUserService.SetCurrentUser(firestoreUser);
                        Console.WriteLine($"[FirebaseAuthService] AuthStateChanged: User {firestoreUser.Email} (Firestore) logged in.");
                        attempt = 4;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[FirebaseAuthService] Attempt {attempt}: Error fetching user {e.User.Uid}: {ex.Message}");

                        if (attempt < maxRetries)
                        {
                            await Task.Delay(delayMs);
                            delayMs *= 2; // Экспоненциальная задержка
                        }
                        else
                        {
                            Console.WriteLine($"[FirebaseAuthService] Failed to fetch user {e.User.Uid} after {maxRetries} attempts");
                        }
                    }
                }
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
