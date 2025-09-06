using EatTogether.MAUI.Services;

namespace EatTogether.MAUI
{
    public partial class AppShell : Shell
    {
        private readonly CurrentUserService _currentUserService;

        public AppShell(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
            InitializeComponent();

            Task.Delay(500).ContinueWith(async _ =>
            {
                await CheckAuthState();
            });
        }

        private async Task CheckAuthState()
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (_currentUserService.CurrentUser != null)
                {
                    await GoToAsync("//ProfilePage");
                }
                else
                {
                    await GoToAsync("//SignInPage");
                }
            });
        }
    }
}
