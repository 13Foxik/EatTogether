using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;
using Firebase.Auth;
using System.Text.Json;
using User = EatTogether.MAUI.Models.User;

namespace EatTogether.MAUI.Services
{
    public class EmailAuthService : IEmailAuth
    {
        public AuthType Type => AuthType.Email;
        private readonly IFirebaseAuthService _firebaseAuthService;
        private readonly ICloudStoreService _cloudStoreService;

        public EmailAuthService(IFirebaseAuthService firebaseAuthService, ICloudStoreService cloudStoreService)
        {
            _firebaseAuthService = firebaseAuthService;
            _cloudStoreService = cloudStoreService;
        }

        private FirebaseAuthClient GetAuthClient() => _firebaseAuthService.GetAuthClient();

        public async Task<User> SignInAsync()
        {
            throw new NotImplementedException("Use SignInAsync(email, password)");
        }
        public async Task<User> SignInAsync(string email, string password)
        {
            try
            {
                var userCredential = await GetAuthClient().SignInWithEmailAndPasswordAsync(email, password);
                var firebaseUser = userCredential.User;
                return MapFirebaseUserToAppUser(firebaseUser);
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"Ошибка аунтификации Firebase: {ex.Reason} - {ex.Message}");

                if (ex.Message.Contains("INVALID_LOGIN_CREDENTIALS"))
                {
                    throw new Exception("Неверный Email или пароль.");
                }
                throw new Exception(GetFirebaseErrorMessage(ex.Reason));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка при входе в систему с помощью EmailAsync: {ex.Message}");
                throw;
            }
        }
        public async Task<User> SignUpAsync(string email, string password, string confirmPassword, string displayName, string firstName, 
                                            string lastName, DateTime dateOfBirth)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(displayName)) throw new Exception("Введите имя пользователя");

                if (displayName.Length < 4) throw new Exception("Имя пользователя слишком короткое");

                if (string.IsNullOrWhiteSpace(email)) throw new Exception("Введите Email");

                if (string.IsNullOrWhiteSpace(password)) throw new Exception("Введите Пароль");

                if (password != null && password.Length > 30) throw new Exception("Пароль слишком длинный.");

                if (password != null && password.Length < 6) throw new Exception("Пароль слишком короткий.");

                if (string.IsNullOrWhiteSpace(confirmPassword)) throw new Exception("Подтвердите пароль");

                if (confirmPassword != password) throw new Exception("Пароли не совпадают");

                var userCredential = await GetAuthClient().CreateUserWithEmailAndPasswordAsync(email, password);
                var firebaseUser = userCredential.User;

                await firebaseUser.ChangeDisplayNameAsync(displayName);

                var user = new User
                {
                    Uid = firebaseUser.Uid,
                    Email = firebaseUser.Info.Email,
                    DisplayName = displayName,
                    FirstName = firstName,
                    LastName = lastName,
                    CreatedAt = DateTime.UtcNow,
                    DateOfBirthday = dateOfBirth,
                };

                await _cloudStoreService.InsertUserModel(user);

                return MapFirebaseUserToAppUser(firebaseUser);
            }
            catch (FirebaseAuthException ex)
            {
                Console.WriteLine($"Firebase Auth Error during SignUp: {ex.Reason} - {ex.Message}");

                throw new Exception(GetFirebaseErrorMessage(ex.Reason));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Общая ошибка от SignUpAsyncEmailAsync: {ex.Message}");
                throw;
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
        private string GetFirebaseErrorMessage(AuthErrorReason reason)
        {
            return reason switch
            {
                AuthErrorReason.MissingPassword => "Отсутствует пароль.",
                AuthErrorReason.MissingEmail => "Отсутствует Email.",
                AuthErrorReason.InvalidEmailAddress => "Неверный формат Email.",
                AuthErrorReason.WrongPassword => "Неверный Email или пароль.",
                AuthErrorReason.UserNotFound => "Пользователь не найден.",
                AuthErrorReason.EmailExists => "Пользователь с таким Email уже существует.",
                AuthErrorReason.WeakPassword => "Пароль слишком слабый. Используйте не менее 6 символов.",
                _ => "Произошла неизвестная ошибка аутентификации."
            };
        }
    }
}
