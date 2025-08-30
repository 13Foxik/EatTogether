using CommunityToolkit.Maui;
using EatTogether.MAUI.Services;
using EatTogether.MAUI.Services.Interfaces;
using EatTogether.MAUI.ViewModels;
using EatTogether.MAUI.Views.Auth;
using Microsoft.Extensions.Logging;

namespace EatTogether.MAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("FontAwesome.ttf", "FontAwesome");
                });

            builder.Services.AddSingleton<CurrentUserService>();
            builder.Services.AddSingleton<IAuthProvider, EmailAuthService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddTransient<SignInViewModel>();
            builder.Services.AddTransient<SignInPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif
            

            return builder.Build();
        }
    }
}
