using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;
using Firebase.Auth;
using System.Text.Json;
using User = EatTogether.MAUI.Models.User;

namespace EatTogether.MAUI.Services
{
    public class EmailAuthService : FirebaseAuthLogic, IEmailAuth
    {
        public AuthType Type => AuthType.Email;

        private readonly CurrentUserService _currentUserService;

        public EmailAuthService(CurrentUserService currentUserService) : base (currentUserService)
        {
            _currentUserService = currentUserService;
        }
        public async Task<User> SignInAsync()
        {
            throw new NotImplementedException("Use SignInAsync(email, password)");
        }
        public async Task<User> SignInAsync(string email, string password)
        {
            try
            {
                var userCredential = await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(email, password);
                var firebaseUser = userCredential.User;
                return MapFirebaseUserToAppUser(firebaseUser);
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"Firebase Auth Error: {ex.Reason} - {ex.Message}");
                // Обработай различные ошибки Firebase (например, неверный пароль, пользователь не найден)

                if (ex.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                {
                    throw new Exception("Неверный Email или пароль.");
                }

                throw new Exception(GetFirebaseErrorMessage(ex.Reason));
            }

            catch (Exception ex)
            {
                Console.WriteLine($"General Error during SignInWithEmailAsync: {ex.Message}");
                throw;
            }
        }
        public async Task<User> SignUpAsync(string email, string password, string confirmPassword, string displayName)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(displayName)) throw new Exception("Введите имя пользователя");

                if (displayName.Length < 4) throw new Exception("Имя пользователя слишком короткое");

                if (string.IsNullOrWhiteSpace(email)) throw new Exception("Введите Email");

                if (string.IsNullOrWhiteSpace(password)) throw new Exception("Введите Пароль");

                if (password != null && password.Length > 15) throw new Exception("Пароль слишком длинный.");

                if (password != null && password.Length < 6) throw new Exception("Пароль слишком короткий.");

                if (string.IsNullOrWhiteSpace(confirmPassword)) throw new Exception("Подтвердите пароль");

                if (confirmPassword != password) throw new Exception("Пароли не совпадают");

                var userCredential = await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(email, password);
                var firebaseUser = userCredential.User;
                // Опционально: обновить DisplayName сразу после регистрации
                await firebaseUser.ChangeDisplayNameAsync(displayName);
                return MapFirebaseUserToAppUser(firebaseUser);
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"Firebase Auth Error during SignUp: {ex.Reason} - {ex.Message}");

                


                throw new Exception(GetFirebaseErrorMessage(ex.Reason));
            }
            catch (Exception ex)
            {

                Console.WriteLine($"General Error during SignUpWithEmailAsync: {ex.Message}");
                throw;
            }
        }
        protected override string GetFirebaseErrorMessage(AuthErrorReason reason)
        {
            var specificError = reason switch
            {
                AuthErrorReason.MissingPassword => "Отсутствует пароль.",
                AuthErrorReason.MissingEmail => "Отсутствует Email.",
                AuthErrorReason.InvalidEmailAddress => "Неверный формат Email.",
                AuthErrorReason.WrongPassword => "Неверный Email или пароль.",
                AuthErrorReason.UserNotFound => "Пользователь не найден.",
                AuthErrorReason.EmailExists => "Пользователь с таким Email уже существует.",
                AuthErrorReason.WeakPassword => "Пароль слишком слабый. Используйте не менее 6 символов.",
                _ => null
            };

            if (specificError != null)
                return specificError;

            return base.GetFirebaseErrorMessage(reason);
        }
    }
}
