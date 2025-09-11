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
                    Application.Current.MainPage = new MainPage();
                }
                //else
                //{
                //    await GoToAsync("//SignInPage");
                //}
            });
        }
    }
}
