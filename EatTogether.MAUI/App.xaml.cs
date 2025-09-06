using EatTogether.MAUI.Services;

namespace EatTogether.MAUI
{
    public partial class App : Application
    {
        private readonly CurrentUserService _currentUserService;
        public App(CurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;

            InitializeComponent();

            MainPage = new AppShell(_currentUserService);
        }
    }
}
