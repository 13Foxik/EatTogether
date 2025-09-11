using EatTogether.MAUI.Services;

namespace EatTogether.MAUI
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        private readonly CurrentUserService _currentUserService;
        public App(CurrentUserService currentUserService, IServiceProvider serviceProvider)
        {
            Services = serviceProvider;
            _currentUserService = currentUserService;

            InitializeComponent();

            MainPage = new AppShell(_currentUserService);
        }
    }
}
