using EatTogether.MAUI.Services;
using EatTogether.MAUI.Views.Main;

namespace EatTogether.MAUI
{
    public partial class AppShell : Shell
    {
        private readonly CurrentUserService _currentUserService;

        public AppShell(CurrentUserService currentUserService)
        {
            InitializeComponent();
            _currentUserService = currentUserService;

            // Используем Loaded-событие Shell, чтобы проверка авторизации
            // выполнялась ПОСЛЕ того как UI-дерево полностью построено.
            // Это надёжно работает и в Debug, и в Release (AOT/Trimming).
            this.Loaded += OnShellLoaded;
        }

        private async void OnShellLoaded(object? sender, EventArgs e)
        {
            this.Loaded -= OnShellLoaded;
            await CheckAuthState();
        }

        private async Task CheckAuthState()
        {
            if (_currentUserService.CurrentUser != null)
            {
                Application.Current!.MainPage = new MainPage();
            }
            else
            {
                await GoToAsync("//SignInPage");
            }
        }
    }
}
