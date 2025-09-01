using EatTogether.MAUI.Models;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.Services;
using Firebase.Auth;
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
                throw new Exception(GetFirebaseErrorMessage(ex.Reason));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error during SignInWithEmailAsync: {ex.Message}");
                throw;
            }
        }
        public async Task<User> SignUpAsync(string email, string password, string displayName)
        {
            try
            {
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
        private string GetFirebaseErrorMessage(AuthErrorReason reason)
        {
            return reason switch
            {
                AuthErrorReason.MissingPassword => "Отсутствует пароль.",
                AuthErrorReason.InvalidEmailAddress => "Неверный формат Email.",
                AuthErrorReason.WrongPassword => "Неверный пароль.",
                AuthErrorReason.UserNotFound => "Пользователь не найден.",
                AuthErrorReason.EmailExists => "Пользователь с таким Email уже существует.",
                AuthErrorReason.WeakPassword => "Пароль слишком слабый. Используйте не менее 6 символов.",
                // Добавь другие причины ошибок по мере необходимости
                _ => "Произошла неизвестная ошибка аутентификации."
            };
        }
    }
}
