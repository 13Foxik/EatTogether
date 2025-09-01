using Firebase.Auth;
using Firebase.Auth.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EatTogether.MAUI.Services
{
    public abstract class FirebaseAuthLogic
    {
        protected readonly FirebaseAuthClient _firebaseAuthClient;
        protected readonly CurrentUserService _currentUserService;

        protected FirebaseAuthLogic(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
            var config = new FirebaseAuthConfig
            {
                ApiKey = "AIzaSyCb2_3YbdpZB78zKbErXZi9zjXQAOkble0",
                AuthDomain = "eattogether-79984.firebaseapp.com",
                Providers = new FirebaseAuthProvider[]
                {
                    new EmailProvider(),
                }
            };
            _firebaseAuthClient = new FirebaseAuthClient(config);
            _firebaseAuthClient.AuthStateChanged += (sender, e) =>
            {
                if (e.User != null)
                {
                    _currentUserService.SetCurrentUser(MapFirebaseUserToAppUser(e.User));
                    Console.WriteLine($"Firebase User State Changed: User signed in - {e.User.Info.Email}");
                }
                else
                {
                    _currentUserService.ClearUser();
                    Console.WriteLine("Firebase User State Changed: User signed out.");
                }
            };
        }

        protected Models.User MapFirebaseUserToAppUser(Firebase.Auth.User firebaseUser)
        {
            return new Models.User(
                uid: firebaseUser.Uid,
                email: firebaseUser.Info.Email,
                displayName: firebaseUser.Info.DisplayName ?? firebaseUser.Info.Email
            );
        }

        protected virtual string GetFirebaseErrorMessage(AuthErrorReason reason)
        {
            return reason switch
            {
                _ => "Произошла неизвестная ошибка аутентификации."
            };
        }
        //public async Task SignOutAsync()
        //{
        //    await _firebaseAuthClient.SignOutAsync();
        //}
    }
}
